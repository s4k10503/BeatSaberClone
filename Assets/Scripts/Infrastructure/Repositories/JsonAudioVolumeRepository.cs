using System;
using System.IO;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using BeatSaberClone.Domain;

namespace BeatSaberClone.Infrastructure
{
    public sealed class JsonAudioVolumeRepository : IAudioVolumeRepository
    {
        private readonly string _audioSettingsFilePath;

        private AudioVolumeSettings _cachedSettings;
        private DateTime _lastCacheUpdate;
        private const int CACHE_VALIDITY_MINUTES = 5;

        public JsonAudioVolumeRepository()
        {
            _audioSettingsFilePath = Path.Combine(Application.persistentDataPath, "SoundSettings.json");
        }

        public JsonAudioVolumeRepository(string filePath)
        {
            _audioSettingsFilePath = filePath;
        }

        public async UniTask SaveAudioSettingsAsync(float volumeBGM, float volumeSE, CancellationToken ct)
        {
            ValidateVolumeValues(volumeBGM, volumeSE);

            try
            {
                var settings = new AudioVolumeSettings(volumeBGM, volumeSE);

                string json = JsonUtility.ToJson(settings, true);
                await File.WriteAllTextAsync(_audioSettingsFilePath, json, ct);

                _cachedSettings = settings;
                _lastCacheUpdate = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Failed to save sound settings to JSON", ex);
            }
        }

        public async UniTask<(float VolumeBGM, float VolumeSE)> LoadAudioSettingsAsync(CancellationToken ct)
        {
            // If cache is enabled, return from cache
            if (IsCacheValid())
            {
                return (_cachedSettings.TrackVolume, _cachedSettings.EffectsVolume);
            }

            try
            {
                if (!File.Exists(_audioSettingsFilePath))
                {
                    // Save and return default values
                    await SaveAudioSettingsAsync(0.5f, 0.5f, ct);
                    return (0.5f, 0.5f);
                }

                string json = await File.ReadAllTextAsync(_audioSettingsFilePath, ct);
                var settings = JsonUtility.FromJson<AudioVolumeSettings>(json);

                if (settings == null)
                {
                    throw new InfrastructureException("Failed to deserialize sound settings");
                }

                ValidateVolumeValues(settings.TrackVolume, settings.EffectsVolume);

                _cachedSettings = settings;
                _lastCacheUpdate = DateTime.UtcNow;

                return (settings.TrackVolume, settings.EffectsVolume);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Failed to load sound settings from JSON", ex);
            }
        }

        private bool IsCacheValid()
        {
            return _cachedSettings != null && (DateTime.UtcNow - _lastCacheUpdate).TotalMinutes < CACHE_VALIDITY_MINUTES;
        }

        private void ValidateVolumeValues(float volumeBGM, float volumeSE)
        {
            if (volumeBGM < 0 || volumeBGM > 1)
                throw new InfrastructureException($"Invalid BGM volume value: {volumeBGM}. Must be between 0 and 1.");
            if (volumeSE < 0 || volumeSE > 1)
                throw new InfrastructureException($"Invalid SE volume value: {volumeSE}. Must be between 0 and 1.");
        }

        public void Dispose()
        {
            _cachedSettings = null;
        }
    }
}
