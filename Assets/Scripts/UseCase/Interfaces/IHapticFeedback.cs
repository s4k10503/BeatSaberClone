using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.XR;

namespace BeatSaberClone.UseCase
{
    public interface IHapticFeedbackUseCase : IDisposable
    {
        UniTask TriggerFeedback(XRNode xrNode, CancellationToken ct);
    }
}
