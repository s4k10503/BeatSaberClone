using UnityEngine;
using System;

namespace BeatSaberClone.Presentation
{
    public sealed class MovementAnimationController : IMovementAnimationController
    {
        private bool _isRotating;
        private float _moveSpeed;
        private float _lerpSpeed;
        private float _originalY;
        private float _rotationSpeed;
        private Quaternion _targetRotation;

        public void SetParameters(float moveSpeed, float originalY, float lerpSpeed)
        {
            _moveSpeed = moveSpeed;
            _originalY = originalY;
            _lerpSpeed = lerpSpeed;
        }

        public void InitializeRotation(
            Transform transform,
            Quaternion targetRotation,
            float duration)
        {
            if (transform == null || transform.gameObject == null) return;

            try
            {
                _targetRotation = targetRotation;

                // Calculate the angle difference between the current rotation and the target rotation
                float angleDifference = Quaternion.Angle(transform.rotation, _targetRotation);

                // Calculate the rotation speed (angle/second)
                _rotationSpeed = angleDifference / duration;
                _isRotating = true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("InitializeRotation failed: ", ex);
            }
        }

        public void ApplyMovementAndRotation(Transform transform, float moveSpeed, bool hasSlowedDown)
        {
            _moveSpeed = moveSpeed;

            MoveForward(transform, hasSlowedDown);

            if (_isRotating)
            {
                Rotate(transform);
            }
        }

        public void StopRotation()
        {
            _isRotating = false;
        }

        private void MoveForward(Transform transform, bool hasSlowedDown)
        {
            if (transform == null || transform.gameObject == null) return;

            // Cash to local variables
            float deltaTime = Time.deltaTime;
            Vector3 position = transform.position;

            // Move in the axial direction
            position -= _moveSpeed * deltaTime * Vector3.forward;

            // Supreme y coordinates and return to their original position
            if (hasSlowedDown && !Mathf.Approximately(position.y, _originalY))
            {
                position.y = Mathf.Lerp(position.y, _originalY, deltaTime * _lerpSpeed);
            }

            transform.position = position;
        }

        private void Rotate(Transform transform)
        {
            if (transform == null || transform.gameObject == null)
            {
                _isRotating = false;
                return;
            }

            // Cash to local variables
            float deltaTime = Time.deltaTime;
            Quaternion currentRotation = transform.rotation;

            // Calculate the angle difference between the current rotation and the target rotation
            float angleDifference = Quaternion.Angle(currentRotation, _targetRotation);

            if (angleDifference > 0.1f)
            {
                // Close to the target based on the rotation speed
                transform.rotation = Quaternion.RotateTowards(
                    currentRotation,
                    _targetRotation,
                    _rotationSpeed * deltaTime
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
