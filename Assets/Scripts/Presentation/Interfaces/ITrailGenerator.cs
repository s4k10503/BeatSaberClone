using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BeatSaberClone.Presentation
{
    public interface ITrailGenerator : IDisposable
    {
        UniTask InitializeAsync(
            Transform tipTransform,
            Transform baseTransform,
            GameObject meshParent,
            int numVerticesPerFrame,
            int trailFrameLength,
            CancellationToken ct);

        UniTask UpdateTrailAsync(CancellationToken ct);
    }
}
