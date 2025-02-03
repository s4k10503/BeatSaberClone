using UnityEngine;

namespace BeatSaberClone.Presentation
{
    [CreateAssetMenu(fileName = "AudioVisualEffectParameters", menuName = "BeatSaberClone/AudioVisualEffectParameters")]
    public sealed class AudioVisualEffectParameters : ScriptableObject
    {
        [Header("Fog Settings")]
        public Color BaseFogColor = Color.gray;
        public Color TargetFogColor = Color.white;

        [Header("Effect Intensity")]
        public float IntensityScale = 100.0f;
        public float ScaleMultiplier = 100.0f;
        public float LerpSpeed = 1.0f;
    }
}
