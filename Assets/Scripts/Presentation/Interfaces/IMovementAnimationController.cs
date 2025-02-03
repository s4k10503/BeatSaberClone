using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BeatSaberClone.Presentation
{
    public interface IMovementAnimationController
    {
        void InitializeMovement(float moveSpeed, float originalY, float initialXPosition, float lerpSpeed);
        UniTask StartRotationAsync(Transform transform, Quaternion targetRotation, float duration, float delaySeconds, CancellationToken ct);
        void UpdateMovementAndRotation(Transform transform, float moveSpeed);
    }
}
