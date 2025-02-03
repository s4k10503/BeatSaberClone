using UnityEngine;

namespace BeatSaberClone.Presentation
{
    [CreateAssetMenu(fileName = "BoxSettings", menuName = "BeatSaberClone/BoxSettings")]
    public class BoxSettings : ScriptableObject
    {
        [Header("Movement Settings")]

        // Box initial speed (fast phase movement speed)
        [SerializeField, Range(0f, 100f)]
        private float _initialMoveSpeed = 70f;
        public float InitialMoveSpeed => _initialMoveSpeed;

        // Final speed of box (slow phase movement speed)
        [SerializeField, Range(0f, 20f)]
        private float _finalMoveSpeed = 5f;
        public float FinalMoveSpeed => _finalMoveSpeed;

        // If the distance from the player falls below this value, the movement speed will be slow.
        [SerializeField, Tooltip("Slow down threshold starts at the distance from the player")]
        private float _slowDownDistance = 10f;
        public float SlowDownDistance => _slowDownDistance;

        // Settings for box position interpolation and destruction
        public float LerpSpeed = 5f;
        public float DestroyZCoordinates = -5f;
        public float DefaultOriginalY = 0f;
    }
}
