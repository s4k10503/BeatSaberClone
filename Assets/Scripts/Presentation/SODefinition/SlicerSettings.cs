using UnityEngine;

namespace BeatSaberClone.Presentation
{
    [CreateAssetMenu(fileName = "SlicerSettings", menuName = "BeatSaberClone/SlicerSettings")]
    public class SlicerSettings : ScriptableObject
    {
        [Header("Cast Settings")]
        public float DetectionRadius = 0.1f;
        public int InterpolationSteps = 10;

        [Header("Trail Settings")]
        public int NumVerticesPerFrame = 12;
        public int TrailFrameLength = 3;
    }
}
