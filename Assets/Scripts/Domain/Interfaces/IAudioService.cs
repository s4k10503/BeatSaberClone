using UniRx;

namespace BeatSaberClone.Domain
{
    public interface IAudioService
    {
        IReadOnlyReactiveProperty<AudioVolumeSettings> Settings { get; }
        bool TrackIsPlaying { get; }
        void PlayTrack(AudioAsset clip);
        void PlayEffect(AudioAsset clip);
        void PauseTrack();
        void ResumeTrack();
        void UpdateSettings(float trackVolume, float effectsVolume);
    }
}
