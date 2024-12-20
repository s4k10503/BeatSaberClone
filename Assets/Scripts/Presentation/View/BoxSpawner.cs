using System;
using System.Linq;
using UnityEngine;
using UniRx;
using Zenject;
using System.Threading;
using Cysharp.Threading.Tasks;
using BeatSaberClone.Domain;

namespace BeatSaberClone.Presentation
{
    public sealed class BoxSpawner : MonoBehaviour, IBoxSpawner
    {
        [Header("Game Objects")]
        [SerializeField] private GameObject _particleSpawnLeft;
        [SerializeField] private GameObject _particleSpawnRight;
        [SerializeField] private Transform _playerPoint;
        [SerializeField] private Transform[] _spawnPoints;

        private CustomBoxViewFactory _boxViewFactory;
        private IParticleEffectHandler _particleEffectHandler;

        public Transform PlayerPoint => _playerPoint;
        public Transform[] SpawnPoints => _spawnPoints;
        private float _lowestY;

        private Subject<BoxView> _boxViewCreated = new Subject<BoxView>();
        public IObservable<BoxView> BoxViewCreated => _boxViewCreated.AsObservable();

        private Subject<string> _onErrorOccurred = new Subject<string>();
        public IObservable<string> OnErrorOccurred => _onErrorOccurred.AsObservable();

        [Inject]
        public void Construct(
            IParticleEffectHandler particleEffectHandler,
            CustomBoxViewFactory boxViewFactory)
        {
            _particleEffectHandler = particleEffectHandler;
            _boxViewFactory = boxViewFactory;

            // Determine the Y coordinate of the bottom point
            _lowestY = _spawnPoints.Min(sp => sp.position.y);
        }

        private void OnDestroy()
        {
            _boxViewCreated?.Dispose();
            _onErrorOccurred?.Dispose();

            _particleSpawnLeft = null;
            _particleSpawnRight = null;
            _playerPoint = null;
            _spawnPoints = null;
        }

        public async UniTask SpawnNote(NoteInfo note, float moveSpeed, CancellationToken ct)
        {
            try
            {
                // Validate spawn index
                int spawnIndex = note._lineIndex + note._lineLayer * 4;
                if (spawnIndex < 0 || spawnIndex >= _spawnPoints.Length)
                {
                    LogError($"Spawn index ({spawnIndex}) is out of bounds.");
                    return;
                }

                // Pre-calculate spawn position
                Transform spawnPoint = _spawnPoints[spawnIndex];
                Vector3 spawnPosition = new Vector3(spawnPoint.position.x, _lowestY, spawnPoint.position.z);

                Quaternion randomRotation = await UniTask.RunOnThreadPool(() =>
                {
                    var random = new System.Random();
                    return Quaternion.Euler(
                        (float)random.NextDouble() * 30f - 15f, // X axis (-15 to 15)
                        (float)random.NextDouble() * 30f - 15f, // Y axis (-15 to 15)
                        (float)random.NextDouble() * 30f - 15f  // Z axis (-15 to 15)
                    );
                }, cancellationToken: ct);

                // Create box view and instance
                var boxView = _boxViewFactory.Create(note._type, moveSpeed, spawnPoint.position.y);
                GameObject boxInstance = boxView.gameObject;

                // Apply position and rotation
                Transform boxTransform = boxInstance.transform;
                boxTransform.SetPositionAndRotation(spawnPosition, randomRotation);

                // Initialize BoxView asynchronously
                var animationTask = boxView.SetAnimation(note._cutDirection, ct);

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

                // Wait for all tasks to complete
                await UniTask.WhenAll(animationTask, particleTask);

                // Notify observers
                _boxViewCreated.OnNext(boxView);
            }
            catch (Exception e)
            {
                LogError($"Failed to spawn note: {e.Message}");
            }
        }

        private async UniTask TriggerParticleEffect(GameObject particlePrefab, Vector3 position, CancellationToken ct)
        {
            Quaternion particleRotation = Quaternion.Euler(-90, 0, 0);
            float particleLifetime = 1.0f;

            await _particleEffectHandler.TriggerParticleEffect(
                particlePrefab, position, particleRotation, particleLifetime, ct
            );
        }

        private void LogError(string message)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            _onErrorOccurred.OnNext(message);
#endif
        }
    }
}
