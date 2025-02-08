using UnityEngine;
using Zenject;

namespace BeatSaberClone.Presentation
{
    [CreateAssetMenu(fileName = "ParticleEffectSettings", menuName = "BeatSaberClone/ParticleEffectSettings")]
    public class ParticleEffectSettings : ScriptableObjectInstaller
    {
        [Header("Particle Pool Settings")]
        public int DefaultPoolCapacity = 4;
        public int MaxPoolSize = 8;

        [Header("Particle Effect Timing")]
        public float ParticleDelayTime = 0.0f;

        public override void InstallBindings()
        {
            Container.BindInstance(this);
        }
    }
}
