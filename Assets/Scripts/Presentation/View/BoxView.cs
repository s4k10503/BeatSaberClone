using System;
using UnityEngine;
using UniRx;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

namespace BeatSaberClone.Presentation
{
    public sealed class BoxView : MonoBehaviour
    {
        private GameObject _cachedGameObject;

        public bool IsSliced { get; private set; }

        private IMovementAnimationController _movementAnimationController;
        private ISlicedObject _slicedObject;

        private float _lerpSpeed;
        private float _destroyZCoordinates;
        private bool _isMoving;
        private int _type;
        private float _moveSpeed; // Current movement speed (starting at the initial speed)
        private float _originalY;
        private float _finalMoveSpeed;    // Moving speed after throwing down
        private float _slowDownDistance; // Slow -down timing, the threshold of Z coordinates, etc.
        private bool _hasSlowedDown;      // Is it already slow down?
        private float _rotationDuration;
        private float _rotationDelay;
        private Quaternion _targetRotation;

        private readonly Subject<Unit> _onComboReset = new();
        public IObservable<Unit> OnComboReset => _onComboReset;

        private CancellationToken _ct;

        private enum CutDirection
        {
            Down = 0,
            Up = 1,
            Right = 2,
            Left = 3
        }

        [Inject]
        public void Construct(
            IMovementAnimationController movementAnimationController,
            ISlicedObject slicedObject,
            [Inject(Id = "BoxLerpSpeed")] float lerpSpeed,
            [Inject(Id = "BoxDestroyZCoordinates")] float destroyZCoordinates,
            [Inject(Id = "BoxFinalMoveSpeed")] float finalMoveSpeed,
            [Inject(Id = "BoxSlowDownDistance")] float slowDownDistance,
            [Inject(Id = "BoxRotationDuration")] float rotationDuration,
            [Inject(Id = "BoxRotationDelay")] float rotationDelay)
        {
            _movementAnimationController = movementAnimationController;
            _slicedObject = slicedObject;
            _lerpSpeed = lerpSpeed;
            _destroyZCoordinates = destroyZCoordinates;
            _finalMoveSpeed = finalMoveSpeed;
            _slowDownDistance = slowDownDistance;
            _hasSlowedDown = false;
            _rotationDuration = rotationDuration;
            _rotationDelay = rotationDelay;

            _cachedGameObject = gameObject;
        }

        public sealed class Factory : PlaceholderFactory<BoxView> { }

        private void FixedUpdate()
        {
            if (!_isMoving || _cachedGameObject == null) return;

            // If the specified threshold is not yet slowed down, change the movement speed
            if (!_hasSlowedDown && transform.position.z <= _slowDownDistance)
            {
                _moveSpeed = _finalMoveSpeed;
                _hasSlowedDown = true;

                if (transform.position.z > _destroyZCoordinates)
                {
                    TriggerAnimation(_ct).Forget();
                }
            }

            if (transform.position.z > _destroyZCoordinates)
            {
                _movementAnimationController.ApplyMovementAndRotation(transform, _moveSpeed);
            }
            else
            {
                _movementAnimationController.StopRotation();
                _onComboReset.OnNext(Unit.Default);

                Destroy(_cachedGameObject);
                _cachedGameObject = null;
            }
        }

        private void OnDestroy()
        {
            _onComboReset.Dispose();
            _movementAnimationController = null;
            _slicedObject = null;
        }

        private async UniTask TriggerAnimation(CancellationToken ct)
        {
            if (_cachedGameObject == null || transform == null) return;

            // Supplies from current rotation to target rotation
            try
            {
                _movementAnimationController
                    .SetParameters(_moveSpeed, _originalY, _lerpSpeed);
                await _movementAnimationController
                    .InitializeRotationAsync(transform, _targetRotation, _rotationDuration, _rotationDelay, ct);
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
            CancellationToken ct)
        {
            if (IsSliced) return;

            IsSliced = true;

            await _slicedObject.Sliced(_cachedGameObject, slicePosition, planeNormal, crossSectionMaterial, ct);
        }

        public float CheckSliceDirection(Vector3 velocity, int cutDirection)
        {
            // Get the expected cut direction
            Vector3 expectedDirection = GetExpectedCutDirection((CutDirection)cutDirection);

            // Calculate the actual cutting direction and the expected direction
            float angle = Vector3.Angle(velocity.normalized, expectedDirection);

            // Angle allowable range (eg ± 15 degrees)
            var allowedAngle = 15f;

            // Correction recommendation: Logic about score magnification is not a View responsibility
            // It is conceivable to return a difference from the target angle
            if (angle <= allowedAngle)
            {
                return 1f;
            }
            else
            {
                _onComboReset.OnNext(Unit.Default);
                return 0f;
            }
        }

        // Set the box type, initial speed, and early Y coordinates
        public void SetParameters(int type, float moveSpeed, float originalY, int cutDirection, CancellationToken ct)
        {
            _type = type;
            _moveSpeed = moveSpeed;
            _originalY = originalY;
            _isMoving = true;
            IsSliced = false;
            _hasSlowedDown = false;
            _ct = ct;

            float _targetRotationZ = GetTargetRotationZ((CutDirection)cutDirection);
            _targetRotation = Quaternion.Euler(0, 0, _targetRotationZ);
        }


        private float GetTargetRotationZ(CutDirection cutDirection)
        {
            return cutDirection switch
            {
                CutDirection.Down => 180f,
                CutDirection.Up => 0f,
                CutDirection.Right => 90f,
                CutDirection.Left => 270f,
                _ => 0f
            };
        }

        private Vector3 GetExpectedCutDirection(CutDirection cutDirection)
        {
            return cutDirection switch
            {
                CutDirection.Down => Vector3.down,
                CutDirection.Up => Vector3.up,
                CutDirection.Right => Vector3.right,
                CutDirection.Left => Vector3.left,
                _ => Vector3.zero
            };
        }
    }
}
