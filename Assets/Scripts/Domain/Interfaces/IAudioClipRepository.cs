using UnityEngine;

namespace BeatSaberClone.Domain
{
    public interface IAudioClipRepository
    {
        AudioClip GetSeClip(SoundEffect effect);
        AudioClip GetTrackClip();
    }
}
