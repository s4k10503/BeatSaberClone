using UnityEngine;

namespace BeatSaberClone.Infrastructure
{
    [CreateAssetMenu(fileName = "HapticSettings", menuName = "BeatSaberClone/HapticSettings")]
    public class HapticSettings : ScriptableObject
    {
        // Speed thresholds for detecting the start and end of swing
        [Header("Haptic Settings")]
        public float HapticDuration = 0.1f;
        public float HapticIntensity = 0.5f;
    }
}
