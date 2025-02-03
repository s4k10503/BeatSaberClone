using UnityEngine;

namespace BeatSaberClone.Presentation
{
    [CreateAssetMenu(fileName = "TrailSettings", menuName = "BeatSaberClone/TrailSettings")]
    public class TrailSettings : ScriptableObject
    {
        [Header("Trail Settings")]
        public int NumVerticesPerFrame = 12;
        public int TrailFrameLength = 3;
    }
}
