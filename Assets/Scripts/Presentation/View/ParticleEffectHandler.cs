using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;
using System.Threading;
using System;
using System.Collections.Generic;

namespace BeatSaberClone.Presentation
{
    public class ParticleEffectHandler : MonoBehaviour, IParticleEffectHandler
    {
        private Dictionary<GameObject, ObjectPool<GameObject>> _particlePools
            = new Dictionary<GameObject, ObjectPool<GameObject>>();

        private void OnDestroy()
        {
            _particlePools?.Clear();
            _particlePools = null;
        }

        public async UniTask TriggerParticleEffect(
            GameObject particlePrefab,
            Vector3 position,
            Quaternion rotation,
            float delayTime,
            CancellationToken ct)
        {
            // Check if a pool for this prefab exists; if not, create one.
            if (!_particlePools.TryGetValue(particlePrefab, out var pool))
            {
                pool = new ObjectPool<GameObject>(
                    createFunc: () => Instantiate(particlePrefab),
                    actionOnGet: effect => effect.SetActive(true),
                    actionOnRelease: effect => effect.SetActive(false),
                    actionOnDestroy: Destroy,
                    collectionCheck: false,
                    defaultCapacity: 4,
                    maxSize: 8
                );
                _particlePools[particlePrefab] = pool;
            }

            var effectInstance = pool.Get();
            effectInstance.transform.SetPositionAndRotation(position, rotation);

            if (effectInstance.TryGetComponent<ParticleSystem>(out var particleSystem))
            {
                particleSystem.Play();
                try
                {
                    await UniTask.Delay((int)((particleSystem.main.duration + delayTime) * 1000), cancellationToken: ct);
                }
                catch (OperationCanceledException)
                {
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                finally
                {
                    if (!ct.IsCancellationRequested)
                    {
                        pool.Release(effectInstance);
                    }
                }
            }
        }
    }
}