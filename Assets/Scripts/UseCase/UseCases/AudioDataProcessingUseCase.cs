using System.Threading;
using BeatSaberClone.Domain;
using Cysharp.Threading.Tasks;
using Zenject;

namespace BeatSaberClone.UseCase
{
    public sealed class AudioDataProcessingUseCase : IAudioDataProcessingUseCase
    {
        private readonly IAudioDataProcessor _audioDataProcessor;

        [Inject]
        public AudioDataProcessingUseCase(
            IAudioDataProcessor audioDataProcessor)
        {
            _audioDataProcessor = audioDataProcessor;
        }

        public void Dispose()
        {
            _audioDataProcessor.Dispose();
        }

        public void UpdateSpectrumData()
        {
            _audioDataProcessor.UpdateSpectrumData();
        }

        public async UniTask<float> GetAverageSpectrumAsync(CancellationToken ct)
        {
            return await UniTask.RunOnThreadPool(() =>
            {
                return _audioDataProcessor.CalculateAverageSpectrum();
            }, cancellationToken: ct);
        }

        public async UniTask<float[]> GetSpectrumDataAsync(CancellationToken ct)
        {
            return await UniTask.RunOnThreadPool(() =>
            {
                return _audioDataProcessor.SpectrumData;
            }, cancellationToken: ct);
        }
    }
}