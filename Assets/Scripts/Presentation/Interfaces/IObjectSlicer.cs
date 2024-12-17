using UnityEngine;
using UniRx;
using BeatSaberClone.Domain;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace BeatSaberClone.Presentation
{
    public interface IObjectSlicer
    {
        SlicerSide Side { get; }
        IReadOnlyReactiveProperty<GameObject> HitObject { get; }
        Transform TipTransform { get; }
        Material CrossSectionMaterial { get; }
        Vector3 PlaneNormal { get; }
        Vector3 SliceDirection { get; }
        Vector3 Velocity { get; }

        UniTask Initialize(CancellationToken ct);
        void SliceDetection(CancellationToken ct);
        void UpdateTrail();
    }
}
