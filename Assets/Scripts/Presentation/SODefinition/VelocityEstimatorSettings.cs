using UnityEngine;

namespace BeatSaberClone.Presentation
{
    [CreateAssetMenu(fileName = "VelocityEstimatorSettings", menuName = "BeatSaberClone/VelocityEstimatorSettings")]
    public class VelocityEstimatorSettings : ScriptableObject
    {
        [Tooltip("How many frames to average over for computing velocity")]
        public int VelocityAverageFrames = 5;
        [Tooltip("How many frames to average over for computing angular velocity")]
        public int AngularVelocityAverageFrames = 11;
        public bool EstimateOnAwake = false;
    }
}
