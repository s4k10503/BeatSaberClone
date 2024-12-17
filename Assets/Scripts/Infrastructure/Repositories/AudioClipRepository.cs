using UnityEngine;
using Zenject;
using BeatSaberClone.Domain;

namespace BeatSaberClone.Infrastructure
{
    public class AudioClipRepository : IAudioClipRepository
    {
        private readonly AudioClipList _audioClipList;

        [Inject]
        public AudioClipRepository(AudioClipList audioClipList)
        {
            _audioClipList = audioClipList;
        }

        public AudioClip GetSeClip(SoundEffect effect)
        {
            switch (effect)
            {
                case SoundEffect.Slice:
                    return _audioClipList.ClipSlice;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(effect), "Sound effect is not found.");
            }
        }

        public AudioClip GetTrackClip()
        {
            return _audioClipList.ClipDemoTrack;
        }
    }
}
