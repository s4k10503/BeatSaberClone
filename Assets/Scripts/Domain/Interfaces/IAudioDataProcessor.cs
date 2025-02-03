using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace BeatSaberClone.Domain
{
    public interface IAudioDataProcessor : IDisposable
    {
        float[] SpectrumData { get; }
        void UpdateSpectrumData();
        float CalculateAverageSpectrum();
    }
}
