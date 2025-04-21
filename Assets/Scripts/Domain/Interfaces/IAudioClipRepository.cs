using System;

namespace BeatSaberClone.Domain
{
    public interface IAudioClipRepository : IDisposable
    {
        AudioAsset GetSeAsset(SoundEffect effect);
        AudioAsset GetTrackAsset();
        void ClearCache();
    }
}
