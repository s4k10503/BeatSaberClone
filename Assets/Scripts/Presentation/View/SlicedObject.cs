using UnityEngine;
using Cysharp.Threading.Tasks;
using EzySlice;
using System.Threading;
using System;

namespace BeatSaberClone.Presentation
{
    public sealed class SlicedObject : ISlicedObject
    {
        private readonly float _cutForce = 2000f;
        private readonly float _deleteDelayTime = 0.3f;

        public async UniTask Sliced(
            GameObject targetObject,
            Vector3 slicePosition,
            Vector3 planeNormal,
            Material crossSectionMaterial,
            CancellationToken ct)
        {
            var hull = targetObject.Slice(slicePosition, planeNormal);

            if (hull != null)
            {
                var upperHull = hull.CreateUpperHull(targetObject, crossSectionMaterial);
                var lowerHull = hull.CreateLowerHull(targetObject, crossSectionMaterial);

                UnityEngine.Object.Destroy(targetObject);
                targetObject = null;

                await UniTask.WhenAll(
                    AddForceToSlicedObjectAsync(upperHull, ct),
                    AddForceToSlicedObjectAsync(lowerHull, ct)
                );
            }
        }

        private async UniTask AddForceToSlicedObjectAsync(GameObject obj, CancellationToken ct)
        {
            var rb = obj.AddComponent<Rigidbody>();
            var collider = obj.AddComponent<MeshCollider>();
            collider.convex = true;
            rb.AddExplosionForce(_cutForce, obj.transform.position, 1);

            await UniTask.Delay(TimeSpan.FromSeconds(_deleteDelayTime), cancellationToken: ct);
            UnityEngine.Object.Destroy(obj);
            obj = null;
        }
    }
}