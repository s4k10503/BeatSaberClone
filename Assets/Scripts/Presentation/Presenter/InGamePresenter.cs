using System;
using UnityEngine;
using UniRx;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;
using BeatSaberClone.Domain;
using BeatSaberClone.UseCase;
using UnityEngine.XR;
using UnityEditor;

namespace BeatSaberClone.Presentation
{
    public sealed class InGamePresenter : IInitializable, ITickable, IFixedTickable, ILateTickable
    {
        // UseCases
        private readonly INotesManagementUseCase _notesManagementUseCase;
        private readonly IScoreUseCase _scoreUseCase;
        private readonly IViewLogUseCase _viewLogUseCase;
        private readonly IAudioDataProcessingUseCase _audioDataProcessingUseCase;
        private readonly IHapticFeedback _hapticFeedback;
        private readonly ISoundUseCase _soundUseCase;

        // View
        private readonly IAudioVisualEffecter _audioVisualEffecter;
        private readonly IInGameUiView _inGameUiView;
        private readonly IBoxSpawner _boxSpawner;
        private readonly IObjectSlicer _objectSlicerR;
        private readonly IObjectSlicer _objectSlicerL;

        // Others
        private GameState _currentState;
        private readonly float _boxMoveSpeed;
        private float _trackStartTime;
        private CompositeDisposable _disposables;
        private CancellationTokenSource _cts;

        [Inject]
        public InGamePresenter(
            INotesManagementUseCase notesManagementUseCase,
            IScoreUseCase scoreUseCase,
            IViewLogUseCase viewLogUseCase,
            IHapticFeedback hapticFeedback,
            ISoundUseCase soundUseCase,
            IAudioDataProcessingUseCase audioDataProcessingUseCase,
            IAudioVisualEffecter audioVisualEffecter,
            IInGameUiView InGameUiView,
            IBoxSpawner boxSpawner,
            [Inject(Id = "RightSlicer")] IObjectSlicer objectSlicerR,
            [Inject(Id = "LeftSlicer")] IObjectSlicer objectSlicerL,
            [Inject(Id = "BoxMoveSpeed")] float boxMoveSpeed)
        {
            _notesManagementUseCase = notesManagementUseCase;
            _scoreUseCase = scoreUseCase;
            _viewLogUseCase = viewLogUseCase;
            _audioDataProcessingUseCase = audioDataProcessingUseCase;
            _audioVisualEffecter = audioVisualEffecter;
            _hapticFeedback = hapticFeedback;
            _soundUseCase = soundUseCase;
            _inGameUiView = InGameUiView;
            _boxSpawner = boxSpawner;
            _boxMoveSpeed = boxMoveSpeed;
            _objectSlicerL = objectSlicerL;
            _objectSlicerR = objectSlicerR;

            _disposables = new CompositeDisposable();
            _cts = new CancellationTokenSource();
        }

        public void Initialize()
        {
            InitializeGame().Forget();
            StartGame().Forget();
        }

        public void Tick()
        {
            if (_currentState == GameState.Playing)
            {
                UpdateGame().Forget();

                if (IsGameOver())
                {
                    _currentState = GameState.Ended;
                    Application.Quit();
#if UNITY_EDITOR
                    EditorApplication.isPlaying = false;
#endif
                }
            }
        }

        public void FixedTick()
        {
            if (_currentState == GameState.Playing)
            {
                FixedUpdateGame();
            }
        }

        public void LateTick()
        {
            _objectSlicerR.UpdateTrail();
            _objectSlicerL.UpdateTrail();
        }

        public async UniTaskVoid InitializeGame()
        {
            _currentState = GameState.Initialized;

            await UniTask.WhenAll(
                InitializeUseCases(),
                InitialiseView()
            );
        }

        public async UniTaskVoid StartGame()
        {
            _currentState = GameState.Playing;

            await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: _cts.Token);

            _soundUseCase.PlayTrack();
            _trackStartTime = Time.time;

            SubscribeToEvents();
        }

        public async UniTaskVoid UpdateGame()
        {
            if (_soundUseCase.GetTrackIsPlaying())
            {
                float currentTime = Time.time - _trackStartTime;
                _inGameUiView.UpdateTimer(currentTime);

                var notes = await _notesManagementUseCase.GetNotesToSpawn(currentTime, _cts.Token);

                await UniTask.WhenAll(
                    notes.Select(note => _boxSpawner.SpawnNote(note, _boxMoveSpeed, _cts.Token)));

                var average = await _audioDataProcessingUseCase.GetAverageSpectrumAsync(_cts.Token);
                var spectrumData = _audioDataProcessingUseCase.GetSpectrumData();
                _audioVisualEffecter.UpdateEffect(average, spectrumData);
            }
            else if (!_soundUseCase.GetTrackIsPlaying() && _trackStartTime > 0)
            {
                await _scoreUseCase.SaveScore(_cts.Token);
            }
        }

        public void FixedUpdateGame()
        {
            if (!_soundUseCase.GetTrackIsPlaying()) return;

            _audioDataProcessingUseCase.UpdateSpectrumData();

            _objectSlicerR.SliceDetection(_cts.Token);
            _objectSlicerL.SliceDetection(_cts.Token);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private async UniTask InitializeUseCases()
        {
            // Initialize UseCases
            _notesManagementUseCase.SetDistance(_boxSpawner.SpawnPoints[0], _boxSpawner.PlayerPoint);
            _notesManagementUseCase.PrepareNotes(_boxMoveSpeed);

            await _soundUseCase.InitializeAsync(_cts.Token);

            // Note: Provisional processing.The following description is not required when creating a setting screen etc.
            _soundUseCase.SetTrackVolume(1.0f);
            _soundUseCase.SetSeVolume(0.25f);
            await _soundUseCase.SaveVolume(_cts.Token);
        }

        private async UniTask InitialiseView()
        {
            // Initialize View
            _audioVisualEffecter.Initialize();

            await UniTask.WhenAll(
                _objectSlicerR.Initialize(_cts.Token),
                _objectSlicerL.Initialize(_cts.Token)
            );

            _inGameUiView.SetTotalDuration(_soundUseCase.GetTotalDuration());
        }

        public bool IsGameOver()
        {
            if (_trackStartTime == 0)
                return false;
            return !_soundUseCase.GetTrackIsPlaying();
        }

        private void SubscribeToEvents()
        {
            SubscribeToScoreUseCase(_scoreUseCase);
            SubscribeToBoxSpawner(_boxSpawner);
            SubscribeToSlicer(_objectSlicerR);
            SubscribeToSlicer(_objectSlicerL);
            SubscribeToUi(_inGameUiView);
        }

        private void SubscribeToScoreUseCase(IScoreUseCase scoreUseCase)
        {
            scoreUseCase.CurrentScore
                .Subscribe(_inGameUiView.DispalyScore)
                .AddTo(_disposables);

            scoreUseCase.CurrentCombo
                .Subscribe(_inGameUiView.DispalyCombo)
                .AddTo(_disposables);

            scoreUseCase.CurrentComboMultiplier
                .Subscribe(_inGameUiView.DispalyComboMultiplier)
                .AddTo(_disposables);
        }

        private void SubscribeToBoxSpawner(IBoxSpawner boxSpawner)
        {
            boxSpawner.BoxViewCreated
                .Subscribe(boxView =>
                {
                    boxView.OnComboReset
                        .Subscribe(_ => _scoreUseCase.UpdateCombo(0))
                        .AddTo(_disposables);

                    boxView.OnErrorOccurred
                        .Subscribe(_viewLogUseCase.LogError)
                        .AddTo(_disposables);
                })
                .AddTo(_disposables);

            boxSpawner.OnErrorOccurred
                .Subscribe(_viewLogUseCase.LogError)
                .AddTo(_disposables);
        }

        private void SubscribeToUi(IInGameUiView inGameUiView)
        {
            inGameUiView.OnErrorOccurred
                .Subscribe(_viewLogUseCase.LogError)
                .AddTo(_disposables);
        }

        private void SubscribeToSlicer(IObjectSlicer slicer)
        {
            slicer.HitObject
                .Where(hit => hit != null)
                .Subscribe(hit =>
                {
                    var boxView = hit.GetComponent<BoxView>();
                    if (boxView != null && !boxView.IsSliced)
                    {
                        // Play SE
                        var playSoundTask = _soundUseCase.PlaySoundEffect(SoundEffect.Slice, _cts.Token);

                        // Execute the vibration of the controller
                        var feedbackTask = _hapticFeedback.TriggerFeedback(
                            slicer.Side == SlicerSide.Left
                                ? XRNode.LeftHand
                                : XRNode.RightHand,
                                _cts.Token);

                        UniTask.WhenAll(playSoundTask, feedbackTask).Forget();

                        // Execute slice process
                        boxView.Sliced(
                            slicer.TipTransform.position,
                            slicer.PlaneNormal,
                            slicer.CrossSectionMaterial,
                            _cts.Token).Forget();

                        // Scores and combos updated based on correct slice direction
                        int slicerId = slicer.Side == SlicerSide.Left ? 0 : 1;
                        float scoreMultiplier = boxView.CheckSliceDirection(slicer.Velocity, slicerId);
                        _scoreUseCase.UpdateScore(scoreMultiplier);
                        _scoreUseCase.UpdateCombo(scoreMultiplier);
                    }
                })
                .AddTo(_disposables);
        }
    }
}
