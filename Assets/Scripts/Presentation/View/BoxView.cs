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
        [SerializeField] private float _lerpSpeed = 5f;
        [SerializeField] private float _destroyZCoordinates = -5f;
        public bool IsSliced { get; private set; }
        private bool _isMoving;

        private IMovementAnimationController _movementAnimationController;
        private ISlicedObject _slicedObject;
        private int _type;
        private float _moveSpeed;
        private float _originalY;

        private readonly Subject<Unit> _onComboReset = new Subject<Unit>();
        public IObservable<Unit> OnComboReset => _onComboReset;

        private Subject<string> _onErrorOccurred = new Subject<string>();
        public IObservable<string> OnErrorOccurred => _onErrorOccurred.AsObservable();

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
            ISlicedObject slicedObject)
        {
            _movementAnimationController = movementAnimationController;
            _slicedObject = slicedObject;
        }

        public class Factory : PlaceholderFactory<BoxView> { }

        private void FixedUpdate()
        {
            if (!_isMoving) return;

            if (transform.position.z > _destroyZCoordinates)
            {
                _movementAnimationController.UpdateMovementAndRotation(transform);
            }
            else
            {
                _onComboReset.OnNext(Unit.Default);
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            _onComboReset.Dispose();
            _movementAnimationController = null;
            _slicedObject = null;
        }

        public async UniTask SetAnimation(int cutDirection, CancellationToken ct)
        {
            float targetRotationZ = GetTargetRotationZ((CutDirection)cutDirection);
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetRotationZ);

            _movementAnimationController
                .InitializeMovement(_moveSpeed, _originalY, transform.position.x, _lerpSpeed);

            // interpolation from current rotation to target rotation
            await _movementAnimationController
                .StartRotationAsync(transform, targetRotation, 0.2f, 0.1f, ct);
        }

        public async UniTask Sliced(
            Vector3 slicePosition,
            Vector3 planeNormal,
            Material crossSectionMaterial,
            CancellationToken ct)
        {
            if (IsSliced) return;

            IsSliced = true;

            await _slicedObject.Sliced(gameObject, slicePosition, planeNormal, crossSectionMaterial, ct);
        }

        public float CheckSliceDirection(Vector3 velocity, int cutDirection)
        {
            // Acquire a cutting direction (expected direction)
            Vector3 expectedDirection = GetExpectedCutDirection((CutDirection)cutDirection);

            // Calculate the current cutting direction and the expected cutting direction
            float angle = Vector3.Angle(velocity.normalized, expectedDirection);

            // Absent angle range (for example, ± 30 degrees)
            var allowedAngle = 15f;

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

        public void SetParameters(int type, float moveSpeed, float originalY)
        {
            _type = type;
            _moveSpeed = moveSpeed;
            _originalY = originalY;
            _isMoving = true;
            IsSliced = false;
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

        private void LogError(string message)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            _onErrorOccurred.OnNext(message);
#endif
        }
    }
}
