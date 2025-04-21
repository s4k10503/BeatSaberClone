using System;
using UniRx;
using BeatSaberClone.Domain;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Zenject;

namespace BeatSaberClone.UseCase
{
    public sealed class AudioUseCase : IAudioUseCase, IDisposable
    {
        private Dictionary<SoundEffect, AudioAsset> _soundEffects;
        private readonly IAudioClipRepository _audioClipRepository;
        private readonly IAudioVolumeRepository _audioVolumeRepository;
        private readonly IAudioService _audioService;
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public AudioUseCase(
            IAudioVolumeRepository audioVolumeRepository,
            IAudioClipRepository audioClipRepository,
            IAudioService audioService)
        {
            _soundEffects = new Dictionary<SoundEffect, AudioAsset>();
            _audioVolumeRepository = audioVolumeRepository;
            _audioClipRepository = audioClipRepository;
            _audioService = audioService;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            await LoadAudioSettingsAsync(ct);
        }

        private async UniTask LoadAudioSettingsAsync(CancellationToken ct)
        {
            (float volumeTrack, float volumeSe) = await _audioVolumeRepository.LoadAudioSettingsAsync(ct);

            _audioService.UpdateSettings(volumeTrack, volumeSe);
            _soundEffects[SoundEffect.Slice] = _audioClipRepository.GetSeAsset(SoundEffect.Slice);
        }

        public void SetTrackVolume(float volume)
        {
            var currentSettings = _audioService.Settings.Value;
            _audioService.UpdateSettings(volume, currentSettings.EffectsVolume);
        }

        public void SetSeVolume(float volume)
        {
            var currentSettings = _audioService.Settings.Value;
            _audioService.UpdateSettings(currentSettings.TrackVolume, volume);
        }

        public async UniTask SaveVolume(CancellationToken ct)
        {
            var settings = _audioService.Settings.Value;
            await _audioVolumeRepository.SaveAudioSettingsAsync(settings.TrackVolume, settings.EffectsVolume, ct);
        }

        public void PlayTrack()
        {
            _audioService.PlayTrack(_audioClipRepository.GetTrackAsset());
        }

        public void PauseTrack()
        {
            _audioService.PauseTrack();
        }

        public void ResumeTrack()
        {
            _audioService.ResumeTrack();
        }

        public async UniTask PlaySoundEffect(SoundEffect effect, CancellationToken ct)
        {
            if (_soundEffects.TryGetValue(effect, out AudioAsset asset))
            {
                _audioService.PlayEffect(asset);
                await UniTask.Delay(TimeSpan.FromSeconds(asset.Length), cancellationToken: ct);
            }
        }

        public float GetTotalDuration()
        {
            return _audioClipRepository.GetTrackAsset().Length;
        }

        public bool GetTrackIsPlaying()
        {
            return _audioService.TrackIsPlaying;
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _soundEffects?.Clear();
            _soundEffects = null;
        }
    }
}
