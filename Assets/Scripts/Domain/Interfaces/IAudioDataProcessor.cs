using System;

namespace BeatSaberClone.Domain
{
    public interface IAudioDataProcessor : IDisposable
    {
        float[] SpectrumData { get; }
        float[] BandData { get; }
        void UpdateSpectrumData();
        float CalculateAverageSpectrum();
        float[] GetFrequencyBands();
        float GetBandEnergy(int bandIndex);
    }
}
