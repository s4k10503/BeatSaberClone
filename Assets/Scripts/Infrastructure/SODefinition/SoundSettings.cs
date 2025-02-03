using UnityEngine;

namespace BeatSaberClone.Infrastructure
{
    [CreateAssetMenu(fileName = "AudioClipList", menuName = "BeatSaberClone/AudioClipList")]
    public sealed class AudioClipList : ScriptableObject
    {
        public AudioClip ClipDemoTrack;
        public AudioClip ClipSlice;
    }
}
