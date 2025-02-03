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
        private static readonly string s_soundSettingsFilePath = Path.Combine(Application.persistentDataPath, "SoundSettings.json");

        [Inject]
        public JsonSoundVolumeRepository()
        {
        }

        public void Dispose()
        {
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
            catch (Exception ex)
            {
                throw new InfrastructureException("Failed to save sound settings to JSON: ", ex);
            }
        }

        public async UniTask<(float VolumeBGM, float VolumeSE)> LoadSoundSettingsAsync(CancellationToken ct)
        {
            try
            {
                if (!File.Exists(s_soundSettingsFilePath))
                {
                    throw new InfrastructureException("Sound settings file not found.");
                }

                string json = await File.ReadAllTextAsync(s_soundSettingsFilePath, ct);
                VolumeSettings volumeSettings = JsonUtility.FromJson<VolumeSettings>(json);

                if (volumeSettings == null)
                {
                    throw new InfrastructureException("Sound settings file is invalid.");
                }
                return (volumeSettings.VolumeBGM, volumeSettings.VolumeSE);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Failed to load sound settings from JSON: ", ex);
            }
        }
    }
}
