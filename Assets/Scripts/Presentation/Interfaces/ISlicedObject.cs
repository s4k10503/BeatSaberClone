using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace BeatSaberClone.Presentation
{
    public interface ISlicedObject
    {
        UniTask Sliced(
            GameObject targetObject,
            Vector3 slicePosition,
            Vector3 planeNormal,
            Material crossSectionMaterial,
            CancellationToken ct);
    }
}
