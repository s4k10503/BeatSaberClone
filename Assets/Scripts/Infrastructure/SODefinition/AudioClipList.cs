using UnityEngine;

namespace BeatSaberClone.Infrastructure
{
    [CreateAssetMenu(fileName = "AudioClipList", menuName = "BeatSaberClone/AudioClipList")]
    public sealed class AudioClipList : ScriptableObject
    {
        [Header("Track Clips")]
        [Tooltip("Demo truck clip.The repository corresponds to the identifier 'default_track'.")]
        public AudioClip ClipDemoTrack;

        [Header("Effect Clips")]
        [Tooltip("Slice clips for sound effects.The repository corresponds to the identifier 'Slice'.")]
        public AudioClip ClipSlice;

        public AudioClip GetTrack(string trackId)
        {
            if (string.IsNullOrEmpty(trackId))
                return null;

            if (trackId == "default_track")
                return ClipDemoTrack;

            return null;
        }

        public AudioClip GetEffect(string effectId)
        {
            if (string.IsNullOrEmpty(effectId))
                return null;

            if (effectId == "Slice")
                return ClipSlice;

            return null;
        }
    }
}
