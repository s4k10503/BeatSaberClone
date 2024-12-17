using System;
using System.Threading;
using BeatSaberClone.Domain;
using Cysharp.Threading.Tasks;
using UnityEngine.XR;

namespace BeatSaberClone.Infrastructure
{
    public class HapticService : IHapticService
    {
        public async UniTask SendHapticFeedback(XRNode xrNode, float intensity, float duration, CancellationToken ct)
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode(xrNode);
            device.SendHapticImpulse(0, intensity, duration);
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: ct);
        }

        public void Dispose()
        {

        }
    }
}
