using UnityEngine;

namespace BeatSaberClone.Presentation
{
    [CreateAssetMenu(fileName = "BoxSettings", menuName = "BeatSaberClone/BoxSettings")]
    public class BoxSettings : ScriptableObject
    {
        [Header("Movement Settings")]

        // Box initial speed (fast phase movement speed)
        [SerializeField, Range(10f, 100f)]
        private float _initialMoveSpeed = 70f;
        public float InitialMoveSpeed => _initialMoveSpeed;

        // Final speed of box (slow phase movement speed)
        [SerializeField, Range(10f, 100f)]
        private float _finalMoveSpeed = 5f;
        public float FinalMoveSpeed => _finalMoveSpeed;

        // If the distance from the player falls below this value, the movement speed will be slow.
        [SerializeField, Tooltip("Slow down threshold starts at the distance from the player")]
        private float _slowDownDistance = 10f;
        public float SlowDownDistance => _slowDownDistance;

        [Header("Box Animation Settings")]
        [SerializeField, Range(0.01f, 1f)]
        private float _rotationDuration = 0.2f;
        public float RotationDuration => _rotationDuration;

        [SerializeField, Range(0f, 1f)]
        private float _rotationDelay = 0.1f;
        public float RotationDelay => _rotationDelay;

        // Settings for box position interpolation and destruction
        [Header("Other Settings")]
        public float LerpSpeed = 5f;
        public float DestroyZCoordinates = -5f;
        public float DefaultOriginalY = 0f;
    }
}
