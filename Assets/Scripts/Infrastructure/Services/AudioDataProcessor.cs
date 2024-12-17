/*
Quoted and modified from:
https://note.com/logic_magic/n/n47e91a1e65bb
*/

using System.Threading;
using BeatSaberClone.Domain;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace BeatSaberClone.Infrastructure
{
    public class AudioDataProcessor : IAudioDataProcessor
    {
        public float[] SpectrumData { get; private set; }

        private AudioSource _audioSource;
        private readonly FFTResolution _fftResolution;
        private readonly FFTWindow _fftWindow;

        [Inject]
        public AudioDataProcessor(
            [Inject(Id = "Track")] AudioSource audioSource,
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
            if (_audioSource.isPlaying && _audioSource.timeSamples < _audioSource.clip.samples)
            {
                _audioSource.GetSpectrumData(SpectrumData, 0, _fftWindow);
            }
            else
            {
                System.Array.Clear(SpectrumData, 0, SpectrumData.Length);
            }
        }

        public async UniTask<float> CalculateAverageSpectrumAsync(CancellationToken ct)
        {
            if (_audioSource == null) return 0f;

            return await UniTask.RunOnThreadPool(() =>
            {
                float sum = 0f;
                if (_audioSource != null)
                {
                    foreach (var value in SpectrumData)
                    {
                        sum += value;
                    }
                }
                return sum / SpectrumData.Length;
            }, cancellationToken: ct);
        }
    }
}
