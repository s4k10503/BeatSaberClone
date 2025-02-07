using UnityEngine;

namespace BeatSaberClone.Presentation
{
    public interface IMovementAnimationController
    {
        void SetParameters(float moveSpeed, float originalY, float lerpSpeed);
        void InitializeRotation(Transform transform, Quaternion targetRotation, float duration);
        void ApplyMovementAndRotation(Transform transform, float moveSpeed);
        void StopRotation();
    }
}
