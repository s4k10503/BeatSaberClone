using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace BeatSaberClone.UseCase
{
    public interface IAudioDataProcessingUseCase : IDisposable
    {
        void UpdateSpectrumData();
        UniTask<float> GetAverageSpectrumAsync(CancellationToken ct);
        UniTask<float[]> GetSpectrumDataAsync(CancellationToken ct);
    }
}
