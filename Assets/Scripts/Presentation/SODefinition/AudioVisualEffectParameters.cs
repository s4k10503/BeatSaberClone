using UnityEngine;

namespace BeatSaberClone.Presentation
{
    [CreateAssetMenu(fileName = "AudioVisualEffectParameters", menuName = "BeatSaberClone/AudioVisualEffectParameters", order = 1)]
    public class AudioVisualEffectParameters : ScriptableObject
    {
        public Color BaseFogColor = Color.gray;
        public Color TargetFogColor = Color.white;
        public float IntensityScale = 100.0f;
        public float ScaleMultiplier = 100.0f;
        public float LerpSpeed = 1.0f;
    }
}
