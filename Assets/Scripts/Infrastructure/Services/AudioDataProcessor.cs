/*
Quoted and modified from:
https://note.com/logic_magic/n/n47e91a1e65bb
*/

using BeatSaberClone.Domain;
using UnityEngine;
using Zenject;
using System;
using System.Linq;

namespace BeatSaberClone.Infrastructure
{
    public sealed class AudioDataProcessor : IAudioDataProcessor
    {
        public float[] SpectrumData { get; private set; }
        public float[] BandData { get; private set; }

        private readonly IAudioSource _audioSource;
        private readonly FFTResolution _fftResolution;
        private readonly FFTWindowType _fftWindow;
        private readonly int _bandCount = 8;
        private readonly float _multiplier = 10f;
        private readonly float _threshold = 0.01f;

        [Inject]
        public AudioDataProcessor(
            [Inject(Id = "TrackSource")] IAudioSource audioSource,
            FFTResolution fftResolution,
            FFTWindowType fftWindow)
        {
            _audioSource = audioSource ?? throw new ArgumentNullException(nameof(audioSource));
            _fftResolution = fftResolution;
            _fftWindow = fftWindow;

            SpectrumData = new float[(int)_fftResolution];
            BandData = new float[_bandCount];
        }

        public void UpdateSpectrumData()
        {
            ValidateAudioSource();

            if (_audioSource.IsPlaying && _audioSource.TimeSamples < _audioSource.Samples)
            {
                _audioSource.GetSpectrumData(SpectrumData, 0, _fftWindow);
                UpdateFrequencyBands();
            }
            else
            {
                ClearData();
            }
        }

        private void UpdateFrequencyBands()
        {
            int sampleCount = (int)_fftResolution;
            int[] bandSampleCounts = CalculateBandSampleCounts(sampleCount);
            int currentSample = 0;

            for (int i = 0; i < _bandCount; i++)
            {
                float average = 0;
                for (int j = 0; j < bandSampleCounts[i]; j++)
                {
                    if (currentSample < sampleCount)
                    {
                        average += SpectrumData[currentSample] * _multiplier;
                        currentSample++;
                    }
                }
                average /= bandSampleCounts[i];
                BandData[i] = average > _threshold ? average : 0;
            }
        }

        private int[] CalculateBandSampleCounts(int totalSamples)
        {
            int[] sampleCounts = new int[_bandCount];
            int remainingSamples = totalSamples;
            float multiplier = 2f;

            for (int i = 0; i < _bandCount; i++)
            {
                int sampleCount = (int)(remainingSamples / multiplier);
                sampleCounts[i] = Math.Max(1, sampleCount);
                remainingSamples -= sampleCount;
                if (remainingSamples <= 0) break;
            }

            return sampleCounts;
        }

        public float CalculateAverageSpectrum()
        {
            ValidateData();
            return SpectrumData.Average();
        }

        public float[] GetFrequencyBands()
        {
            ValidateData();
            return BandData;
        }

        public float GetBandEnergy(int bandIndex)
        {
            if (bandIndex < 0 || bandIndex >= _bandCount)
                throw new InfrastructureException($"Band index {bandIndex} is out of range [0, {_bandCount})");

            ValidateData();
            return BandData[bandIndex];
        }

        private void ValidateAudioSource()
        {
            if (_audioSource == null)
                throw new InfrastructureException("IAudioSource is null.");
            if (_audioSource.Samples == 0)
                throw new InfrastructureException("IAudioSource has no samples.");
        }

        private void ValidateData()
        {
            if (SpectrumData == null || SpectrumData.Length == 0)
                throw new InfrastructureException("SpectrumData is not properly initialized.");
            if (BandData == null || BandData.Length == 0)
                throw new InfrastructureException("BandData is not properly initialized.");
        }

        private void ClearData()
        {
            Array.Clear(SpectrumData, 0, SpectrumData.Length);
            Array.Clear(BandData, 0, BandData.Length);
        }

        public void Dispose()
        {
            Array.Clear(SpectrumData, 0, SpectrumData.Length);
            Array.Clear(BandData, 0, BandData.Length);
            SpectrumData = null;
            BandData = null;
        }
    }
}
