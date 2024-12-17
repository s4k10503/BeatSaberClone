using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BeatSaberClone.Presentation
{
    public interface ITrailGenerator : IDisposable
    {
        UniTask Initialize(
            Transform tipTransform,
            Transform baseTransform,
            GameObject meshParent,
            int numVerticesPerFrame,
            int trailFrameLength,
            CancellationToken ct);

        void UpdateTrail();
    }
}
