using BeatSaberClone.Domain;
using System;
using System.Linq;
using UnityEngine;
using UniRx;
using Zenject;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace BeatSaberClone.Presentation
{
    public sealed class BoxSpawner : MonoBehaviour, IBoxSpawner
    {
        [Header("Game Objects")]
        [SerializeField] private GameObject _particleSpawnLeft;
        [SerializeField] private GameObject _particleSpawnRight;
        [SerializeField] private Transform _playerPoint;
        [SerializeField] private Transform[] _spawnPoints;

        private readonly List<BoxView> _activeBoxViews = new();

        private Vector3 _particleRotation;
        private float _particleDelaytime;
        private int _defaultPoolCapacity;
        private int _maxParticlePoolSize;
        private CustomBoxViewPool _boxViewPool;
        private IParticleEffectHandler _particleEffectHandler;

        public Transform PlayerPoint => _playerPoint;
        public Transform[] SpawnPoints => _spawnPoints;
        private float _lowestY;
        private float _maxDeviation;

        public MovementSettings MovementSettings { get; set; }
        private RotationSettings _rotationSettings;
        private DestroySettings _destroySettings;
        private SlicedSettings _slicedSettings;

        private readonly Subject<BoxView> _boxViewCreated = new();
        public IObservable<BoxView> BoxViewCreated => _boxViewCreated.AsObservable();

        [Inject]
        public void Construct(
            IParticleEffectHandler particleEffectHandler,
            CustomBoxViewPool boxViewFactory,
            BoxSettings boxSettings,
            ParticleEffectSettings particleEffectSettings)
        {
            _particleEffectHandler = particleEffectHandler;
            _boxViewPool = boxViewFactory;

            _particleRotation = boxSettings.ParticleEffectRotation;
            _maxDeviation = boxSettings.MaxRotationDeviation;

            _particleDelaytime = particleEffectSettings.ParticleDelayTime;
            _defaultPoolCapacity = particleEffectSettings.DefaultPoolCapacity;
            _maxParticlePoolSize = particleEffectSettings.MaxPoolSize;

            MovementSettings = new MovementSettings
            {
                LerpSpeed = boxSettings.LerpSpeed,
                InitialMoveSpeed = boxSettings.InitialMoveSpeed,
                FinalMoveSpeed = boxSettings.FinalMoveSpeed,
                SlowDownDistance = boxSettings.SlowDownDistance,
                LowestY = boxSettings.LowestY
            };

            _rotationSettings = new RotationSettings
            {
                RotationDuration = boxSettings.RotationDuration,
                RotationDelayTime = boxSettings.RotationDelayTime
            };

            _destroySettings = new DestroySettings
            {
                DestroyZCoordinates = boxSettings.DestroyZCoordinates,
                DestroyDelayTime = boxSettings.DestroyDelayTime
            };

            _slicedSettings = new SlicedSettings
            {
                DownRotationZ = boxSettings.DownRotationZ,
                UpRotationZ = boxSettings.UpRotationZ,
                RightRotationZ = boxSettings.RightRotationZ,
                LeftRotationZ = boxSettings.LeftRotationZ,
                DownDirection = boxSettings.DownDirection,
                UpDirection = boxSettings.UpDirection,
                RightDirection = boxSettings.RightDirection,
                LeftDirection = boxSettings.LeftDirection,
                AllowedAngle = boxSettings.AllowedAngle
            };
        }

        private void OnDestroy()
        {
            _boxViewCreated?.Dispose();

            _particleSpawnLeft = null;
            _particleSpawnRight = null;
            _playerPoint = null;
            _spawnPoints = null;
        }

        public async UniTask SpawnNote(NoteInfo note, CancellationToken ct)
        {
            try
            {
                // Validate spawn index
                int spawnIndex = note._lineIndex + note._lineLayer * 4;
                if (spawnIndex < 0 || spawnIndex >= _spawnPoints.Length)
                {
                    throw new ApplicationException($"Spawn index ({spawnIndex}) is out of bounds.");
                }

                // Pre-calculate spawn position
                Transform spawnPoint = _spawnPoints[spawnIndex];
                Vector3 spawnPosition = new(spawnPoint.position.x, MovementSettings.LowestY, spawnPoint.position.z);

                Quaternion randomRotation = await UniTask.RunOnThreadPool(() =>
                {
                    var random = new System.Random();
                    return Quaternion.Euler(
                        (float)random.NextDouble() * _maxDeviation * 2f - _maxDeviation,
                        (float)random.NextDouble() * _maxDeviation * 2f - _maxDeviation,
                        (float)random.NextDouble() * _maxDeviation * 2f - _maxDeviation
                    );
                }, cancellationToken: ct);

                // Create spawnSettings for BoxView
                var spawnSettings = new SpawnSettings
                {
                    Type = note._type,
                    OriginalY = spawnPoint.position.y,
                    CutDirection = note._cutDirection,
                    Ct = ct,
                    MovementSettings = MovementSettings,
                    RotationSettings = _rotationSettings,
                    DestroySettings = _destroySettings,
                    SlicedSettings = _slicedSettings
                };

                // Create box view and instance
                var boxView = _boxViewPool.Create(spawnSettings);
                _activeBoxViews.Add(boxView);

                // Apply position and rotation
                Transform boxTransform = boxView.gameObject.transform;
                boxTransform.SetPositionAndRotation(spawnPosition, randomRotation);

                // Start particle effect processing asynchronously
                UniTask particleTask = UniTask.CompletedTask; // Default to a no-op task
                if (note._type == 0 && _particleSpawnLeft != null)
                {
                    particleTask = TriggerParticleEffect(_particleSpawnLeft, boxTransform.position, ct);
                }
                else if (_particleSpawnRight != null)
                {
                    particleTask = TriggerParticleEffect(_particleSpawnRight, boxTransform.position, ct);
                }

                await particleTask;

                // Notify observers
                _boxViewCreated.OnNext(boxView);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to spawn note: ", ex);
            }
        }

        private async UniTask TriggerParticleEffect(GameObject particlePrefab, Vector3 position, CancellationToken ct)
        {
            Quaternion particleRotation = Quaternion.Euler(_particleRotation);

            await _particleEffectHandler.TriggerParticleEffectAsync(
                particlePrefab,
                position,
                particleRotation,
                _particleDelaytime,
                _defaultPoolCapacity,
                _maxParticlePoolSize,
                ct
            );
        }

        public void RemoveBoxView()
        {
            if (_activeBoxViews.Count > 0)
            {
                var boxView = _activeBoxViews[0];
                boxView.ReturnToPool();
                _activeBoxViews.RemoveAt(0);
            }
        }
    }
}
