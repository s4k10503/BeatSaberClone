using System.Collections.Generic;
using BeatSaberClone.Domain;
using Zenject;

namespace BeatSaberClone.Infrastructure
{
    public sealed class AudioClipRepository : IAudioClipRepository
    {
        private readonly Dictionary<string, AudioAsset> _assetCache;
        private readonly AudioClipList _audioClipList;

        [Inject]
        public AudioClipRepository(
            AudioClipList audioClipList)
        {
            _audioClipList = audioClipList;
            _assetCache = new Dictionary<string, AudioAsset>();
        }

        public AudioAsset GetTrackAsset()
        {
            string trackId = "default_track";
            if (_assetCache.TryGetValue(trackId, out var asset))
            {
                return asset;
            }

            var clip = _audioClipList.GetTrack(trackId);
            if (clip == null)
                throw new InfrastructureException($"Track not found: {trackId}");

            var newAsset = new AudioAsset(trackId, clip.length, clip);
            _assetCache[trackId] = newAsset;
            return newAsset;
        }

        public AudioAsset GetSeAsset(SoundEffect effect)
        {
            string effectId = effect.ToString();
            if (_assetCache.TryGetValue(effectId, out var asset))
            {
                return asset;
            }

            var clip = _audioClipList.GetEffect(effectId);
            if (clip == null)
                throw new InfrastructureException($"Effect not found: {effectId}");

            var newAsset = new AudioAsset(effectId, clip.length, clip);
            _assetCache[effectId] = newAsset;
            return newAsset;
        }

        public void ClearCache()
        {
            _assetCache.Clear();
        }

        public void Dispose()
        {
            ClearCache();
        }
    }
}
