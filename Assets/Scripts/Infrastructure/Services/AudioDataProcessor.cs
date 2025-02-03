/*
Quoted and modified from:
https://note.com/logic_magic/n/n47e91a1e65bb
*/

using BeatSaberClone.Domain;
using UnityEngine;
using Zenject;

namespace BeatSaberClone.Infrastructure
{
    public sealed class AudioDataProcessor : IAudioDataProcessor
    {
        public float[] SpectrumData { get; private set; }

        private AudioSource _audioSource;
        private readonly FFTResolution _fftResolution;
        private readonly FFTWindow _fftWindow;

        [Inject]
        public AudioDataProcessor(
            [Inject(Id = "TrackSource")] AudioSource audioSource,
            FFTResolution fftResolution,
            FFTWindow fftWindow)
        {
            _audioSource = audioSource;
            _fftResolution = fftResolution;
            _fftWindow = fftWindow;

            SpectrumData = new float[(int)_fftResolution];
        }

        public void Dispose()
        {
            _audioSource = null;
            SpectrumData = null;
        }

        public void UpdateSpectrumData()
        {
            if (_audioSource == null)
            {
                throw new InfrastructureException("Audiosource is null.");
            }
            if (_audioSource.clip == null)
            {
                throw new InfrastructureException("Audiosource clip is null.");
            }
            if (_audioSource.isPlaying && _audioSource.timeSamples < _audioSource.clip.samples)
            {
                _audioSource.GetSpectrumData(SpectrumData, 0, _fftWindow);
            }
            else
            {
                System.Array.Clear(SpectrumData, 0, SpectrumData.Length);
            }
        }

        public float CalculateAverageSpectrum()
        {
            if (_audioSource == null)
            {
                throw new InfrastructureException("Audiosource is null.");
            }
            if (SpectrumData == null || SpectrumData.Length == 0)
            {
                throw new InfrastructureException("SpectrumData is not properly initialized.");
            }

            float sum = 0f;
            foreach (var value in SpectrumData)
            {
                sum += value;
            }
            return sum / SpectrumData.Length;
        }
    }
}
