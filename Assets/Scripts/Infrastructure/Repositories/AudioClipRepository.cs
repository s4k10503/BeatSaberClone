using UnityEngine;
using Zenject;
using BeatSaberClone.Domain;

namespace BeatSaberClone.Infrastructure
{
    public sealed class AudioClipRepository : IAudioClipRepository
    {
        private readonly AudioClipList _audioClipList;

        [Inject]
        public AudioClipRepository(AudioClipList audioClipList)
        {
            _audioClipList = audioClipList;
        }

        public AudioClip GetSeClip(SoundEffect effect)
        {
            return effect switch
            {
                SoundEffect.Slice => _audioClipList.ClipSlice,
                _ => throw new InfrastructureException($"Sound effect '{effect}' is not found."),
            };
        }

        public AudioClip GetTrackClip()
        {
            return _audioClipList.ClipDemoTrack;
        }
    }
}
