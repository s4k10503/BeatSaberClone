using System;
using UniRx;
using BeatSaberClone.Domain;
using Zenject;

namespace BeatSaberClone.Infrastructure
{
    public sealed class AudioService : IAudioService
    {
        private readonly ReactiveProperty<AudioVolumeSettings> _settings;
        private readonly IAudioSource _trackSource;
        private readonly IAudioSource _effectsSource;

        public IReadOnlyReactiveProperty<AudioVolumeSettings> Settings => _settings;

        public bool TrackIsPlaying => _trackSource.IsPlaying;

        public AudioService(
            [Inject(Id = "TrackSource")] IAudioSource trackSource,
            [Inject(Id = "SeSource")] IAudioSource effectsSource)
        {
            _trackSource = trackSource ?? throw new ArgumentNullException(nameof(trackSource));
            _effectsSource = effectsSource ?? throw new ArgumentNullException(nameof(effectsSource));
            _settings = new ReactiveProperty<AudioVolumeSettings>(
                new AudioVolumeSettings(_trackSource.Volume, _effectsSource.Volume));
        }

        public void PlayTrack(AudioAsset asset)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            // Set up AudioClip from AudioAsset via the interface and play
            _trackSource.SetClip(asset);
            _trackSource.Play();
        }

        public void PlayEffect(AudioAsset asset)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            _effectsSource.PlayOneShot(asset);
        }

        public void PauseTrack()
        {
            _trackSource.Pause();
        }

        public void ResumeTrack()
        {
            _trackSource.UnPause();
        }

        public void UpdateSettings(float trackVolume, float effectsVolume)
        {
            if (trackVolume < 0f || trackVolume > 1f)
                throw new ArgumentException("Track volume must be between 0 and 1", nameof(trackVolume));
            if (effectsVolume < 0f || effectsVolume > 1f)
                throw new ArgumentException("Effects volume must be between 0 and 1", nameof(effectsVolume));

            _trackSource.Volume = trackVolume;
            _effectsSource.Volume = effectsVolume;
            _settings.Value = new AudioVolumeSettings(trackVolume, effectsVolume);
        }
    }
}
