using System.Threading;
using BeatSaberClone.Domain;
using Cysharp.Threading.Tasks;
using Zenject;

namespace BeatSaberClone.UseCase
{
    public class AudioDataProcessingUseCase : IAudioDataProcessingUseCase
    {
        private readonly IAudioDataProcessor _audioDataProcessor;
        private readonly ILoggerService _logger;

        [Inject]
        public AudioDataProcessingUseCase(
            IAudioDataProcessor audioDataProcessor,
            ILoggerService logger)
        {
            _audioDataProcessor = audioDataProcessor;
            _logger = logger;
        }

        public void Dispose()
        {
            //_logger.Log("Dispose AudioDataProcessingUseCase");
            _logger.Dispose();
            _audioDataProcessor.Dispose();
        }

        public void UpdateSpectrumData()
        {
            _audioDataProcessor.UpdateSpectrumData();
        }

        public async UniTask<float> GetAverageSpectrumAsync(CancellationToken ct)
        {
            return await _audioDataProcessor.CalculateAverageSpectrumAsync(ct);
        }

        public float[] GetSpectrumData()
        {
            return _audioDataProcessor.SpectrumData;
        }
    }
}