using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace BeatSaberClone.Presentation
{
    public interface ISlicedObject
    {
        UniTask Sliced(
            Vector3 slicePosition,
            Vector3 planeNormal,
            Material crossSectionMaterial,
            float cutForce,
            CancellationToken ct);
    }
}
