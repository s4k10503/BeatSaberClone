using BeatSaberClone.Domain;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.XR;

namespace BeatSaberClone.Infrastructure
{
    public sealed class HapticService : IHapticService
    {
        public async UniTask SendHapticFeedback(XRNode xrNode, float intensity, float duration, CancellationToken ct)
        {
            if (intensity < 0)
            {
                throw new InfrastructureException("The strength of the vibration cannot be negative.");
            }
            if (duration <= 0)
            {
                throw new InfrastructureException("The duration of the vibration must be greater than 0.");
            }

            InputDevice device = InputDevices.GetDeviceAtXRNode(xrNode);

            try
            {
                bool result = device.SendHapticImpulse(0, intensity, duration);
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: ct);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("An error occurred while sending a haaptic feedback.", ex);
            }
        }

        public void Dispose()
        {
        }
    }
}
