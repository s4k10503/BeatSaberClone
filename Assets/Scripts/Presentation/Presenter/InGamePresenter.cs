using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.XR;
using UnityEditor;
using UniRx;
using Cysharp.Threading.Tasks;
using Zenject;
using BeatSaberClone.Domain;
using BeatSaberClone.UseCase;

namespace BeatSaberClone.Presentation
{
    public sealed class InGamePresenter : IInitializable, ITickable, IFixedTickable, ILateTickable, IUniTaskAsyncDisposable
    {
        #region Dependencies

        // UseCase
        private readonly INotesManagementUseCase _notesManagementUseCase;
        private readonly IScoreUseCase _scoreUseCase;
        private readonly IViewLogUseCase _viewLogUseCase;
        private readonly IAudioDataProcessingUseCase _audioDataProcessingUseCase;
        private readonly IHapticFeedbackUseCase _hapticFeedback;
        private readonly ISoundUseCase _soundUseCase;
        private readonly IExceptionHandlingUseCase _exceptionHandlingUseCase;

        // View
        private readonly IAudioVisualEffecter _audioVisualEffecter;
        private readonly IInGameUiView _inGameUiView;
        private readonly IBoxSpawner _boxSpawner;
        private readonly IObjectSlicer _objectSlicerR;
        private readonly IObjectSlicer _objectSlicerL;

        #endregion

        #region State

        private GameState _currentState;
        private float _trackStartTime;
        private readonly TimeSpan _startDelay = TimeSpan.FromSeconds(0.2f);

        private readonly CompositeDisposable _disposables;
        private readonly CancellationTokenSource _cts;

        // Management list of issued tasks
        private readonly List<UniTask> _runningTasks = new();

        #endregion

        #region Constructor & Injection

        [Inject]
        public InGamePresenter(
            INotesManagementUseCase notesManagementUseCase,
            IScoreUseCase scoreUseCase,
            IViewLogUseCase viewLogUseCase,
            IHapticFeedbackUseCase hapticFeedbackUseCase,
            ISoundUseCase soundUseCase,
            IAudioDataProcessingUseCase audioDataProcessingUseCase,
            IAudioVisualEffecter audioVisualEffecter,
            IInGameUiView inGameUiView,
            IBoxSpawner boxSpawner,
            [Inject(Id = "RightSlicer")] IObjectSlicer objectSlicerR,
            [Inject(Id = "LeftSlicer")] IObjectSlicer objectSlicerL,
            IExceptionHandlingUseCase exceptionHandlingUseCase)
        {
            _notesManagementUseCase = notesManagementUseCase;
            _scoreUseCase = scoreUseCase;
            _viewLogUseCase = viewLogUseCase;
            _audioDataProcessingUseCase = audioDataProcessingUseCase;
            _audioVisualEffecter = audioVisualEffecter;
            _hapticFeedback = hapticFeedbackUseCase;
            _soundUseCase = soundUseCase;
            _inGameUiView = inGameUiView;
            _boxSpawner = boxSpawner;
            _objectSlicerR = objectSlicerR;
            _objectSlicerL = objectSlicerL;
            _exceptionHandlingUseCase = exceptionHandlingUseCase;

            _disposables = new CompositeDisposable();
            _cts = new CancellationTokenSource();
        }

        #endregion

        #region Initialization & Game Lifecycle

        public void Initialize()
        {
            // Issued each process of initializing and starting games as a task
            _runningTasks.Add(InitializeGameAsync());
            _runningTasks.Add(StartGameAsync());
        }

        private async UniTask InitializeGameAsync()
        {
            await _exceptionHandlingUseCase.SafeExecuteAsync(async () =>
            {
                _currentState = GameState.Initialized;
                await UniTask.WhenAll(
                    InitializeUseCasesAsync(),
                    InitializeViewAsync()
                );
            }, _cts.Token);
        }

        private async UniTask StartGameAsync()
        {
            await _exceptionHandlingUseCase.SafeExecuteAsync(async () =>
            {
                _currentState = GameState.Playing;
                await UniTask.Delay(_startDelay, cancellationToken: _cts.Token);
                _soundUseCase.PlayTrack();
                _trackStartTime = Time.time;
                SubscribeToEvents();
            }, _cts.Token);
        }

        #endregion

        #region Update Loops

        public void Tick()
        {
            if (_currentState != GameState.Playing) return;

            // Issued each frame update process as a task
            _runningTasks.Add(UpdateGameAsync());
            CleanCompletedTasks();

            if (IsGameOver())
            {
                EndGame();
            }
        }

        public void FixedTick()
        {
            if (_currentState != GameState.Playing) return;
            _audioDataProcessingUseCase.UpdateSpectrumData();
            CleanCompletedTasks();
        }

        public void LateTick()
        {
            // Notice:Comment out because there is a trail drawing bug
            //_runningTasks.Add(LateUpdateGameAsync());
            //CleanCompletedTasks();
        }

        private async UniTask UpdateGameAsync()
        {
            await _exceptionHandlingUseCase.SafeExecuteAsync(async () =>
            {
                if (_soundUseCase.GetTrackIsPlaying())
                {
                    float currentTime = Time.time - _trackStartTime;
                    _inGameUiView.UpdateTimer(currentTime);

                    // Acquisition of notes and audio data
                    var notesTask = _notesManagementUseCase.GetNotesToSpawnAsync(currentTime, _cts.Token);
                    var spectrumTask = _audioDataProcessingUseCase.GetAverageSpectrumAsync(_cts.Token);
                    var spectrumDataTask = _audioDataProcessingUseCase.GetSpectrumDataAsync(_cts.Token);

                    var notesToSpawn = await notesTask;
                    var averageSpectrum = await spectrumTask;
                    var spectrumData = await spectrumDataTask;

                    // Execute the processing of notes generation in parallel
                    var spawnTasks = new UniTask[notesToSpawn.Count];
                    for (int i = 0; i < notesToSpawn.Count; i++)
                    {
                        spawnTasks[i] = _boxSpawner.SpawnNote(notesToSpawn[i], _cts.Token);
                    }
                    await UniTask.WhenAll(spawnTasks);

                    _audioVisualEffecter.UpdateEffect(averageSpectrum, spectrumData);
                }
                else if (_trackStartTime > 0)
                {
                    await _scoreUseCase.SaveScore(_cts.Token);
                }
            }, _cts.Token);
        }

        private async UniTask LateUpdateGameAsync()
        {
            await UniTask.WhenAll(
                _objectSlicerR.UpdateTrailAsync(_cts.Token),
                _objectSlicerL.UpdateTrailAsync(_cts.Token));
        }

        private bool IsGameOver()
        {
            if (_trackStartTime == 0)
                return false;
            return !_soundUseCase.GetTrackIsPlaying();
        }

        private void EndGame()
        {
            _currentState = GameState.Ended;
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }

        #endregion

        #region UseCases and View Initialization

        private async UniTask InitializeUseCasesAsync()
        {
            await _exceptionHandlingUseCase.SafeExecuteAsync(async () =>
            {
                // Defining ScriptableObjects in DDD is difficult
                // For convenience, the values ​​installed from ScriptableObject have been published from View.
                var initialMoveSpeed = _boxSpawner.MovementSettings.InitialMoveSpeed;
                var finalMoveSpeed = _boxSpawner.MovementSettings.FinalMoveSpeed;
                var slowDownDistance = _boxSpawner.MovementSettings.SlowDownDistance;
                _notesManagementUseCase.Initialize(initialMoveSpeed, finalMoveSpeed, slowDownDistance);
                _notesManagementUseCase.SetDistance(_boxSpawner.SpawnPoints[0], _boxSpawner.PlayerPoint);
                _notesManagementUseCase.PrepareNotes();

                await _soundUseCase.InitializeAsync(_cts.Token);
                _soundUseCase.SetTrackVolume(1.0f);
                _soundUseCase.SetSeVolume(0.25f);
                await _soundUseCase.SaveVolume(_cts.Token);
            }, _cts.Token);
        }

        private async UniTask InitializeViewAsync()
        {
            await _exceptionHandlingUseCase.SafeExecuteAsync(async () =>
            {
                _audioVisualEffecter.Initialize();
                await UniTask.WhenAll(
                    _objectSlicerR.InitializeAsync(_cts.Token),
                    _objectSlicerL.InitializeAsync(_cts.Token)
                );
                _inGameUiView.SetTotalDuration(_soundUseCase.GetTotalDuration());
            }, _cts.Token);
        }

        #endregion

        #region Event Subscriptions

        private void SubscribeToEvents()
        {
            SubscribeToScoreUpdates();
            SubscribeToBoxSpawnerEvents();
            SubscribeToSlicerEvents(_objectSlicerR);
            SubscribeToSlicerEvents(_objectSlicerL);
            SubscribeToUiEvents();
        }

        private void SubscribeToScoreUpdates()
        {
            _scoreUseCase.CurrentScore
                .Subscribe(_inGameUiView.DispalyScore)
                .AddTo(_disposables);

            _scoreUseCase.CurrentCombo
                .Subscribe(_inGameUiView.DispalyCombo)
                .AddTo(_disposables);

            _scoreUseCase.CurrentComboMultiplier
                .Subscribe(_inGameUiView.DispalyComboMultiplier)
                .AddTo(_disposables);

            _scoreUseCase.CurrentComboProgress
                .Subscribe(progress => _inGameUiView.UpdateComboProgress(progress))
                .AddTo(_disposables);
        }

        private void SubscribeToBoxSpawnerEvents()
        {
            _boxSpawner.BoxViewCreated
                .Subscribe(SubscribeToBoxViewEvents)
                .AddTo(_disposables);
        }

        private void SubscribeToBoxViewEvents(BoxView boxView)
        {
            boxView.OnComboReset
                .Subscribe(_ => _scoreUseCase.UpdateCombo(0))
                .AddTo(_disposables);
        }

        private void SubscribeToUiEvents()
        {
            _inGameUiView.OnErrorOccurred
                .Subscribe(_viewLogUseCase.LogError)
                .AddTo(_disposables);
        }

        private void SubscribeToSlicerEvents(IObjectSlicer slicer)
        {
            // Create Slicer specific delegates and cash
            void onHit(GameObject hit) => HandleSlicerHit(slicer, hit);
            slicer.HitObject
                .Where(hit => hit != null)
                .Subscribe(onHit)
                .AddTo(_disposables);
        }

        private void HandleSlicerHit(IObjectSlicer slicer, GameObject hit)
        {
            BoxView boxView = hit.GetComponent<BoxView>();
            if (boxView == null || boxView.IsSliced)
                return;

            // At the same time, execute sound playback, processing of target objects and haptic feedback
            var playSoundTask = _soundUseCase.PlaySoundEffect(SoundEffect.Slice, _cts.Token);
            var feedbackTask = _hapticFeedback.TriggerFeedback(
                slicer.Side == SlicerSide.Left ? XRNode.LeftHand : XRNode.RightHand,
                _cts.Token);

            var sliceTask = boxView.Sliced(
                slicer.TipTransform.position,
                slicer.PlaneNormal,
                slicer.CrossSectionMaterial,
                slicer.CutForce,
                _cts.Token);

            UniTask.WhenAll(playSoundTask, feedbackTask, sliceTask).Forget();

            int slicerId = (slicer.Side == SlicerSide.Left) ? 0 : 1;
            float scoreMultiplier = boxView.CheckSliceDirection(slicer.Velocity, slicerId);
            _scoreUseCase.UpdateScore(scoreMultiplier);
            _scoreUseCase.UpdateCombo(scoreMultiplier);
        }

        #endregion

        #region Task management

        private void CleanCompletedTasks()
        {
            for (int i = _runningTasks.Count - 1; i >= 0; i--)
            {
                if (_runningTasks[i].GetAwaiter().IsCompleted)
                {
                    _runningTasks.RemoveAt(i);
                }
            }
        }

        #endregion

        #region IAsyncDisposable & IDisposable

        public async UniTask DisposeAsync()
        {
            _cts?.Cancel();

            try
            {
                await UniTask.WhenAll(_runningTasks.ToArray());
            }
            catch (OperationCanceledException)
            {
                // Ignore the exception at the time of cancellation
            }
            catch (Exception ex)
            {
                _viewLogUseCase.LogError($"Error awaiting tasks in DisposeAsync: {ex.Message}");
            }
            finally
            {
                _cts?.Dispose();
                _disposables?.Dispose();
            }
        }

        public void Dispose()
        {
            DisposeAsync().Forget();
        }

        #endregion
    }
}
