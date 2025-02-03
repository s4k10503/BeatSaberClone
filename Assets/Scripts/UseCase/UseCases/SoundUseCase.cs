using System;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using Zenject;
using BeatSaberClone.Domain;

namespace BeatSaberClone.UseCase
{
    public sealed class SoundUseCase : ISoundUseCase, IDisposable
    {
        private AudioSource _audioSourceTrack;
        private AudioSource _audioSourceSe;
        private Dictionary<SoundEffect, AudioClip> _soundEffects;

        private readonly IAudioClipRepository _audioClipRepository;
        private readonly ISoundVolumeRepository _soundVolumeRepository;

        // ReactiveProperties
        public IReadOnlyReactiveProperty<float> VolumeTrack
            => _audioSourceTrack
                .ObserveEveryValueChanged(x => x.volume)
                .ToReactiveProperty();
        public IReadOnlyReactiveProperty<float> VolumeSe
            => _audioSourceSe
                .ObserveEveryValueChanged(x => x.volume)
                .ToReactiveProperty();

        [Inject]
        public SoundUseCase(
            [Inject(Id = "TrackSource")] AudioSource audioSourceTrack,
            [Inject(Id = "SeSource")] AudioSource audioSourceSe,
            ISoundVolumeRepository soundVolumeRepository,
            IAudioClipRepository soundEffectsRepository)
        {
            _soundEffects = new Dictionary<SoundEffect, AudioClip>();
            _audioSourceTrack = audioSourceTrack;
            _audioSourceSe = audioSourceSe;
            _soundVolumeRepository = soundVolumeRepository;
            _audioClipRepository = soundEffectsRepository;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            await LoadSoundSettingsAsync(ct);
        }

        public void Dispose()
        {
            _audioSourceSe = null;
            _audioSourceTrack = null;
            _soundEffects = null;
        }

        private async UniTask LoadSoundSettingsAsync(CancellationToken ct)
        {
            (float volumeTrack, float volumeSe) = await _soundVolumeRepository.LoadSoundSettingsAsync(ct);
            _audioSourceTrack.volume = volumeTrack;
            _audioSourceSe.volume = volumeSe;

            _audioSourceTrack.clip = _audioClipRepository.GetTrackClip();
            _soundEffects[SoundEffect.Slice] = _audioClipRepository.GetSeClip(SoundEffect.Slice);
        }

        public void SetTrackVolume(float volumeTrack)
        {
            _audioSourceTrack.volume = volumeTrack;
        }

        public void SetSeVolume(float volumeSe)
        {
            _audioSourceSe.volume = volumeSe;
        }

        public void PlayTrack()
        {
            _audioSourceTrack.Play();
        }

        public async UniTask SaveVolume(CancellationToken ct)
            => await _soundVolumeRepository
                .SaveSoundSettingsAsync(_audioSourceTrack.volume, _audioSourceSe.volume, ct);

        public async UniTask PlaySoundEffect(SoundEffect effect, CancellationToken ct)
        {
            if (_soundEffects.TryGetValue(effect, out AudioClip clip))
            {
                _audioSourceSe.clip = clip;
                _audioSourceSe.Play();
                await UniTask.Delay(TimeSpan.FromSeconds(clip.length), cancellationToken: ct);
            }
        }

        public float GetTotalDuration()
        {
            return _audioSourceTrack.clip.length;
        }

        public bool GetTrackIsPlaying()
        {
            return _audioSourceTrack.isPlaying;
        }
    }
}