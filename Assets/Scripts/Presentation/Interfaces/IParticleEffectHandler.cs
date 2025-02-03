using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BeatSaberClone.Presentation
{
    public interface IParticleEffectHandler
    {
        UniTask TriggerParticleEffect(
            GameObject particlePrefab,
            Vector3 position,
            Quaternion rotation,
            float delayTime,
            int capacity,
            int maxSize,
            CancellationToken ct);
    }
}
