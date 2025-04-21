using System.Threading;
using Cysharp.Threading.Tasks;

namespace BeatSaberClone.Domain
{
    public interface IAudioVolumeRepository
    {
        UniTask SaveAudioSettingsAsync(float volumeBGM, float volumeSE, CancellationToken ct);
        UniTask<(float VolumeBGM, float VolumeSE)> LoadAudioSettingsAsync(CancellationToken ct);
    }
}
