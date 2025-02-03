using System.Threading;
using BeatSaberClone.Domain;
using Cysharp.Threading.Tasks;
using UnityEngine.XR;
using Zenject;

namespace BeatSaberClone.UseCase
{
    public sealed class HapticFeedbackUseCase : IHapticFeedbackUseCase
    {
        private readonly float _hapticDuration;
        private readonly float _hapticIntensity;
        private readonly IHapticService _hapticService;

        [Inject]
        public HapticFeedbackUseCase(
            float hapticDuration,
            float hapticIntensity,
            IHapticService hapticService)
        {
            _hapticDuration = hapticDuration;
            _hapticIntensity = hapticIntensity;
            _hapticService = hapticService;
        }

        public void Dispose()
        {
            _hapticService.Dispose();
        }

        public async UniTask TriggerFeedback(XRNode xrNode, CancellationToken ct)
        {
            await _hapticService
                .SendHapticFeedback(xrNode, _hapticIntensity, _hapticDuration, ct);
        }
    }
}
