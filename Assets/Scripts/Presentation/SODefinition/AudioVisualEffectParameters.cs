using UnityEngine;
using Zenject;

namespace BeatSaberClone.Presentation
{
    [CreateAssetMenu(fileName = "AudioVisualEffectParameters", menuName = "BeatSaberClone/AudioVisualEffectParameters")]
    public sealed class AudioVisualEffectParameters : ScriptableObjectInstaller
    {
        [Header("Common Settings")]
        public float IntensityScale = 100.0f;

        [Header("Fog Settings")]
        public Color BaseFogColor = Color.gray;
        public Color TargetFogColor = Color.white;

        [Header("Light Effect Settings")]
        public float MaxLightIntensity = 3.0f;

        [Header("Scale Effect Settings")]

        public float ScaleMultiplier = 100.0f;
        public float LerpSpeed = 1.0f;

        [Header("Rotation Effect Settings")]
        public float RotationAngleMultiplier = 360f;
        public float RotationThreshold = 0.9f;
        public float DurationPerChild = 0.5f;
        public float DelayBetweenChildren = 0.1f;

        public override void InstallBindings()
        {
            Container.BindInstance(this);
        }
    }
}
