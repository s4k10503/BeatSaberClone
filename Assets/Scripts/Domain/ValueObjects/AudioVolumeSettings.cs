using System;

namespace BeatSaberClone.Domain
{
    [Serializable]
    public sealed class AudioVolumeSettings
    {
        public float TrackVolume { get; }
        public float EffectsVolume { get; }
        public DateTime LastModified { get; }

        // Main constructor requiring all three values.
        public AudioVolumeSettings(float trackVolume, float effectsVolume, DateTime lastModified)
        {
            TrackVolume = ClampVolume(trackVolume);
            EffectsVolume = ClampVolume(effectsVolume);
            LastModified = lastModified;
        }

        // Overloaded constructor for cases where a lastModified value isn’t provided.
        public AudioVolumeSettings(float trackVolume, float effectsVolume)
            : this(trackVolume, effectsVolume, DateTime.UtcNow)
        {
        }

        private static float ClampVolume(float volume)
        {
            return Math.Clamp(volume, 0f, 1f);
        }

        public AudioVolumeSettings WithTrackVolume(float newVolume)
        {
            return new AudioVolumeSettings(newVolume, EffectsVolume);
        }

        public AudioVolumeSettings WithEffectsVolume(float newVolume)
        {
            return new AudioVolumeSettings(TrackVolume, newVolume);
        }

        // Using the overloaded constructor for the default settings.
        public static AudioVolumeSettings Default => new AudioVolumeSettings(1.0f, 0.5f);
    }
}
