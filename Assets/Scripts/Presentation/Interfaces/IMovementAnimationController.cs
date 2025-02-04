using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BeatSaberClone.Presentation
{
    public interface IMovementAnimationController
    {
        void SetParameters(float moveSpeed, float originalY, float lerpSpeed);
        UniTask InitializeRotationAsync(
            Transform transform,
            Quaternion targetRotation,
            float duration,
            float delaySeconds,
            CancellationToken ct);
        void ApplyMovementAndRotation(Transform transform, float moveSpeed);
        void StopRotation();
    }
}
