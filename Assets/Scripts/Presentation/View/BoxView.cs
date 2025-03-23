using System;
using UnityEngine;
using EzySlice;
using UniRx;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

namespace BeatSaberClone.Presentation
{
    // Implement iPoolable and receive parameters at Spawn
    public sealed class BoxView : MonoBehaviour, ISlicedObject
    {
        public bool IsSliced { get; private set; }
        private GameObject _cachedGameObject;
        private IMemoryPool _pool;
        private IMovementAnimationController _movementAnimationController;

        private float _currentMoveSpeed;
        private bool _isMoving;
        private bool _hasSlowedDown;
        private Quaternion _targetRotation;

        private SpawnSettings _spawnSettings;
        private MovementSettings _movementSettings;
        private RotationSettings _rotationSettings;
        private DestroySettings _destroySettings;
        private SlicedSettings _slicedSettings;

        private readonly Subject<Unit> _onComboReset = new();
        public IObservable<Unit> OnComboReset => _onComboReset.AsObservable();

        [Inject]
        public void Construct(IMovementAnimationController movementAnimationController)
        {
            _hasSlowedDown = false;
            _movementAnimationController = movementAnimationController;
            _cachedGameObject = gameObject;
        }

        private void OnDestroy()
        {
            _onComboReset.Dispose();
            _movementAnimationController = null;
        }

        public sealed class BoxPool : MonoMemoryPool<SpawnSettings, BoxView>
        {
            protected override void Reinitialize(SpawnSettings spawnSettings, BoxView item)
            {
                item.OnSpawned(this, spawnSettings);
            }

            protected override void OnDespawned(BoxView item)
            {
                item.OnDespawned();
            }
        }

        private void OnSpawned(IMemoryPool pool, SpawnSettings spawnSettings)
        {
            _pool = pool;

            _spawnSettings = spawnSettings;
            _movementSettings = _spawnSettings.MovementSettings;
            _rotationSettings = _spawnSettings.RotationSettings;
            _destroySettings = _spawnSettings.DestroySettings;
            _slicedSettings = _spawnSettings.SlicedSettings;

            _currentMoveSpeed = _movementSettings.InitialMoveSpeed;
            _isMoving = true;
            IsSliced = false;
            _hasSlowedDown = false;

            float _targetRotationZ = GetTargetRotationZ(_spawnSettings.CutDirection);
            _targetRotation = Quaternion.Euler(0, 0, _targetRotationZ);

            gameObject.SetActive(true);
        }

        private void OnDespawned()
        {
            _isMoving = false;
            _hasSlowedDown = false;
            gameObject.SetActive(false);
        }

        public void ReturnToPool()
        {
            _pool?.Despawn(this);
        }

        private void FixedUpdate()
        {
            if (!_isMoving || _cachedGameObject == null) return;

            TrySlowDown();
            ProcessMovement();
        }

        private void TrySlowDown()
        {
            if (!_hasSlowedDown && transform.position.z <= _movementSettings.SlowDownDistance)
            {
                _currentMoveSpeed = _movementSettings.FinalMoveSpeed;
                _hasSlowedDown = true;

                // If you haven't reached the discarded position yet, start rotating animation.
                if (transform.position.z > _destroySettings.DestroyZCoordinates)
                {
                    TriggerAnimationAsync().Forget();
                }
            }
        }

        private void ProcessMovement()
        {
            if (transform.position.z > _destroySettings.DestroyZCoordinates)
            {
                _movementAnimationController.ApplyMovementAndRotation(transform, _currentMoveSpeed, _hasSlowedDown);
            }
            else
            {
                _movementAnimationController.StopRotation();
                _onComboReset.OnNext(Unit.Default);
                ReturnToPool();
            }
        }

        private async UniTask TriggerAnimationAsync()
        {
            if (_cachedGameObject == null || transform == null) return;

            // Supplies from current rotation to target rotation
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_rotationSettings.RotationDelayTime), cancellationToken: _spawnSettings.Ct);
                _movementAnimationController.SetParameters(_currentMoveSpeed, _spawnSettings.OriginalY, _movementSettings.LerpSpeed);
                _movementAnimationController.InitializeRotation(transform, _targetRotation, _rotationSettings.RotationDuration);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("TriggerAnimation failed: ", ex);
            }
        }

        public async UniTask Sliced(
            Vector3 slicePosition,
            Vector3 planeNormal,
            Material crossSectionMaterial,
            float cutForce,
            CancellationToken ct)
        {
            if (IsSliced) return;

            IsSliced = true;

            var hull = _cachedGameObject.Slice(slicePosition, planeNormal);
            if (hull != null)
            {
                // I want to pool the object.
                // But I've discarded it because there are too many variations in the cut surface.
                var upperHull = hull.CreateUpperHull(_cachedGameObject, crossSectionMaterial);
                var lowerHull = hull.CreateLowerHull(_cachedGameObject, crossSectionMaterial);

                ReturnToPool();

                await UniTask.WhenAll(
                    AddForceToSlicedObjectAsync(upperHull, cutForce, ct),
                    AddForceToSlicedObjectAsync(lowerHull, cutForce, ct)
                );
            }
        }

        private async UniTask AddForceToSlicedObjectAsync(
            GameObject obj,
            float cutForce,
            CancellationToken ct)
        {
            var rb = obj.AddComponent<Rigidbody>();
            var collider = obj.AddComponent<MeshCollider>();
            collider.convex = true;
            rb.AddExplosionForce(cutForce, obj.transform.position, 1);

            await UniTask.Delay(TimeSpan.FromSeconds(_destroySettings.DestroyDelayTime), cancellationToken: ct);

            // The same reason as the note that the method was called
            Destroy(obj);
            obj = null;
        }

        public float CheckSliceDirection(Vector3 velocity, int slicerId)
        {
            if (_spawnSettings.Type != slicerId)
            {
                _onComboReset.OnNext(Unit.Default);
                return 0f;
            }

            // Get the expected cut direction
            Vector3 expectedDirection = GetExpectedCutDirection(_spawnSettings.CutDirection);

            // Get the actual cut direction
            Vector3 normalizedDirection = velocity.normalized;

            // Calculate the angle between the expected direction and the actual direction
            float angle = Vector3.Angle(normalizedDirection, expectedDirection);

            // Correction recommendation: Logic about score magnification is not a View responsibility
            // It is conceivable to return a difference from the target angle
            if (angle <= _slicedSettings.AllowedAngle)
            {
                return 1f;
            }
            else
            {
                _onComboReset.OnNext(Unit.Default);
                return 0f;
            }
        }

        // cutDirection: 0 = Down to Up, 1 = Up to Down, 2 = Right to Left, 3 = Left to Right
        private float GetTargetRotationZ(int cutDirection)
        {
            return cutDirection switch
            {
                0 => _slicedSettings.DownRotationZ,
                1 => _slicedSettings.DefaultRotationZ,
                2 => _slicedSettings.RightRotationZ,
                3 => _slicedSettings.LeftRotationZ,
                _ => 0f,
            };
        }

        // cutDirection: 0 = Down to Up, 1 = Up to Down, 2 = Right to Left, 3 = Left to Right
        private Vector3 GetExpectedCutDirection(int cutDirection)
        {
            return cutDirection switch
            {
                0 => _slicedSettings.UpDirection,
                1 => _slicedSettings.DownDirection,
                2 => _slicedSettings.LeftDirection,
                3 => _slicedSettings.RightDirection,
                _ => Vector3.zero,
            };
        }
    }
}
