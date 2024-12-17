using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

namespace BeatSaberClone.Presentation
{
    public class MovementAnimationController : IMovementAnimationController
    {
        private bool _isMoving;
        private bool _isRotating;
        private float _moveSpeed;
        private float _lerpSpeed;
        private float _originalY;
        private float _rotationSpeed;
        private Quaternion _targetRotation;


        public void InitializeMovement(float moveSpeed, float originalY, float initialXPosition, float lerpSpeed)
        {
            _moveSpeed = moveSpeed;
            _originalY = originalY;
            _lerpSpeed = lerpSpeed;
            _isMoving = true;
        }

        public async UniTask StartRotationAsync(Transform transform, Quaternion targetRotation, float duration, float delaySeconds, CancellationToken ct)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken: ct);

            _targetRotation = targetRotation;

            // Calculate the angle difference between the current rotation and the target rotation
            float angleDifference = Quaternion.Angle(transform.rotation, _targetRotation);

            // Calculate the rotation speed (angle/second)
            _rotationSpeed = angleDifference / duration;
            _isRotating = true;
        }

        public void UpdateMovementAndRotation(Transform transform)
        {
            if (_isMoving)
            {
                Move(transform);
            }

            if (_isRotating)
            {
                Rotate(transform);
            }
        }

        private void Move(Transform transform)
        {
            // Move in the axial direction
            transform.position -= Vector3.forward * _moveSpeed * Time.deltaTime;

            // Supreme y coordinates and return to their original position
            if (!Mathf.Approximately(transform.position.y, _originalY))
            {
                var newY = Mathf.Lerp(transform.position.y, _originalY, Time.deltaTime * _lerpSpeed);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
        }

        private void Rotate(Transform transform)
        {
            // Calculate the angle difference between the current rotation and the target rotation
            float angleDifference = Quaternion.Angle(transform.rotation, _targetRotation);

            if (angleDifference > 0.1f)
            {
                // Close to the target based on the rotation speed
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    _targetRotation,
                    _rotationSpeed * Time.deltaTime
                );
            }
            else
            {
                // reach the target rotation
                transform.rotation = _targetRotation;
                _isRotating = false;
            }
        }
    }
}
