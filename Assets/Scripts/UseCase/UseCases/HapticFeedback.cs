using System.Threading;
using BeatSaberClone.Domain;
using Cysharp.Threading.Tasks;
using UnityEngine.XR;
using Zenject;

namespace BeatSaberClone.UseCase
{
    public class HapticFeedback : IHapticFeedback
    {
        private float _hapticDuration;
        private float _hapticIntensity;
        private readonly IHapticService _hapticService;
        private readonly ILoggerService _logger;

        [Inject]
        public HapticFeedback(
            float hapticDuration,
            float hapticIntensity,
            IHapticService hapticService,
            ILoggerService logger)
        {
            _hapticDuration = hapticDuration;
            _hapticIntensity = hapticIntensity;
            _hapticService = hapticService;
            _logger = logger;
        }

        public void Dispose()
        {
            //_logger.Log("Dispose HapticFeedback");
            _logger.Dispose();
            _hapticService.Dispose();
        }

        public async UniTask TriggerFeedback(XRNode xrNode, CancellationToken ct)
        {
            await _hapticService
                .SendHapticFeedback(xrNode, _hapticIntensity, _hapticDuration, ct);
        }
    }
}
