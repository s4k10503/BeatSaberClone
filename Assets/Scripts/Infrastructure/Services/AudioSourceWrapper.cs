using UnityEngine;
using BeatSaberClone.Domain;
using System;

namespace BeatSaberClone.Infrastructure
{
    public class AudioSourceWrapper : IAudioSource
    {
        private readonly AudioSource _audioSource;

        public AudioSourceWrapper(AudioSource audioSource)
        {
            _audioSource = audioSource;
        }

        public float Volume
        {
            get => _audioSource.volume;
            set => _audioSource.volume = value;
        }

        public bool IsPlaying => _audioSource.isPlaying;

        public int TimeSamples => _audioSource.timeSamples;

        public int Samples => _audioSource.clip?.samples ?? 0;

        public void SetClip(AudioAsset asset)
        {
            AudioClip clip = asset.NativeAsset as AudioClip;
            if (clip == null)
                throw new InvalidOperationException("The internal asset is not a valid AudioClip.");
            _audioSource.clip = clip;
        }

        public void Play()
        {
            _audioSource.Play();
        }

        public void PlayOneShot(AudioAsset asset)
        {
            AudioClip clip = asset.NativeAsset as AudioClip;
            if (clip == null)
                throw new InvalidOperationException("The internal asset is not a valid AudioClip.");
            _audioSource.PlayOneShot(clip);
        }

        public void Pause()
        {
            _audioSource.Pause();
        }

        public void UnPause()
        {
            _audioSource.UnPause();
        }

        public void GetSpectrumData(float[] samples, int channel, FFTWindowType window)
        {
            FFTWindow unityWindow = window switch
            {
                FFTWindowType.Rectangular => FFTWindow.Rectangular,
                FFTWindowType.Triangle => FFTWindow.Triangle,
                FFTWindowType.Hamming => FFTWindow.Hamming,
                FFTWindowType.Hanning => FFTWindow.Hanning,
                FFTWindowType.Blackman => FFTWindow.Blackman,
                FFTWindowType.BlackmanHarris => FFTWindow.BlackmanHarris,
                _ => throw new ArgumentException($"Unknown FFT window type: {window}", nameof(window))
            };
            _audioSource.GetSpectrumData(samples, channel, unityWindow);
        }
    }
}
