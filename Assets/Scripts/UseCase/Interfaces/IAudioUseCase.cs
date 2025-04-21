using System.Threading;
using Cysharp.Threading.Tasks;
using BeatSaberClone.Domain;

namespace BeatSaberClone.UseCase
{
    public interface IAudioUseCase
    {
        UniTask InitializeAsync(CancellationToken ct);
        void SetTrackVolume(float volumeTrack);
        void SetSeVolume(float volumeSe);
        UniTask SaveVolume(CancellationToken ct);
        UniTask PlaySoundEffect(SoundEffect effect, CancellationToken ct);
        void PlayTrack();
        float GetTotalDuration();
        bool GetTrackIsPlaying();
    }
}
