using UnityEngine;
using Zenject;

namespace BeatSaberClone.Presentation
{
    [CreateAssetMenu(fileName = "SlicerSettings", menuName = "BeatSaberClone/SlicerSettings")]
    public class SlicerSettings : ScriptableObjectInstaller
    {
        [Header("Cast Settings")]
        public float DetectionRadius = 0.1f;
        public int InterpolationSteps = 10;

        [Header("Trail Settings")]
        public int NumVerticesPerFrame = 12;
        public int TrailFrameLength = 3;

        [Header("Foce Settings")]
        public float CutForce = 2000f;

        public override void InstallBindings()
        {
            Container.BindInstance(this);
        }
    }
}
