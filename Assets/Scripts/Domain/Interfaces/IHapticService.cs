using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.XR;

namespace BeatSaberClone.Domain
{
    public interface IHapticService : IDisposable
    {
        UniTask SendHapticFeedback(XRNode xrNode, float intensity, float duration, CancellationToken ct);
    }
}
