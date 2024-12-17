using System;
using System.IO;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using BeatSaberClone.Domain;
using Zenject;

namespace BeatSaberClone.Infrastructure
{
    public sealed class JsonSoundVolumeRepository : ISoundVolumeRepository, IDisposable
    {
        private readonly ILoggerService _logger;
        private static readonly string s_soundSettingsFilePath = Path.Combine(Application.persistentDataPath, "SoundSettings.json");

        [Inject]
        public JsonSoundVolumeRepository(ILoggerService logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            _logger.Dispose();
        }

        public async UniTask SaveSoundSettingsAsync(float volumeBGM, float volumeSE, CancellationToken ct)
        {
            try
            {
                VolumeSettings volumeSettings = new VolumeSettings
                {
                    VolumeBGM = volumeBGM,
                    VolumeSE = volumeSE
                };
                string json = JsonUtility.ToJson(volumeSettings);
                await File.WriteAllTextAsync(s_soundSettingsFilePath, json, ct);
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to save sound settings to JSON: {e.Message}");
            }
        }

        public async UniTask<(float VolumeBGM, float VolumeSE)> LoadSoundSettingsAsync(CancellationToken ct)
        {
            try
            {
                if (File.Exists(s_soundSettingsFilePath))
                {
                    string json = await File.ReadAllTextAsync(s_soundSettingsFilePath, ct);
                    VolumeSettings volumeSettings = JsonUtility.FromJson<VolumeSettings>(json);
                    return (volumeSettings.VolumeBGM, volumeSettings.VolumeSE);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to load sound settings from JSON: {e.Message}");
            }

            return (1.0f, 0.25f);
        }
    }
}
