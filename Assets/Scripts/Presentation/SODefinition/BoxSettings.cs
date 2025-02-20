using UnityEngine;
using Zenject;

namespace BeatSaberClone.Presentation
{
    [CreateAssetMenu(fileName = "BoxSettings", menuName = "BeatSaberClone/BoxSettings")]
    public class BoxSettings : ScriptableObjectInstaller
    {
        [Header("Movement Settings")]

        [SerializeField, Range(10f, 100f), Tooltip("Box initial speed (fast phase movement speed)")]
        private float _initialMoveSpeed = 100f;
        public float InitialMoveSpeed => _initialMoveSpeed;

        [SerializeField, Range(10f, 100f)]
        private float _finalMoveSpeed = 10f;
        public float FinalMoveSpeed => _finalMoveSpeed;

        [SerializeField, Tooltip("Slow down threshold starts at the distance from the player")]
        private float _slowDownDistance = 15f;
        public float SlowDownDistance => _slowDownDistance;

        [Tooltip("Lerp factor for movement interpolation")]
        public float LerpSpeed = 5f;

        [Tooltip("Default Y position for box spawning")]
        public float LowestY = 0f;


        [Header("Rotation Settings")]
        [SerializeField, Range(0.01f, 1f)] private float _rotationDuration = 0.2f;
        public float RotationDuration => _rotationDuration;

        [SerializeField, Range(0f, 1f)] private float _rotationDelayTime = 0.1f;
        public float RotationDelayTime => _rotationDelayTime;

        [SerializeField, Tooltip("The maximum value of the random rotation angle for each axis (actually ranges from -Max to +Max)")]
        private float _maxRotationDeviation = 15f;
        public float MaxRotationDeviation => _maxRotationDeviation;

        [Header("Particle Effect Settings")]
        [SerializeField, Tooltip("Rotating Particle Effects (Euler angle)")]
        private Vector3 _particleEffectRotation = new(-90f, 0f, 0f);
        public Vector3 ParticleEffectRotation => _particleEffectRotation;

        [Header("Sliced Settings")]

        // Target Rotation Z Angles
        [SerializeField] private float _downRotationZ = 180f;
        public float DownRotationZ => _downRotationZ;

        [SerializeField] private float _upRotationZ = 0f;
        public float UpRotationZ => _upRotationZ;

        [SerializeField] private float _rightRotationZ = 90f;
        public float RightRotationZ => _rightRotationZ;
        [SerializeField] private float _leftRotationZ = 270f;
        public float LeftRotationZ => _leftRotationZ;

        [Header("Expected Slice Directions")]
        [SerializeField] private Vector3 _downDirection = Vector3.down;
        public Vector3 DownDirection => _downDirection;

        [SerializeField] private Vector3 _upDirection = Vector3.up;
        public Vector3 UpDirection => _upDirection;

        [SerializeField] private Vector3 _rightDirection = Vector3.right;
        public Vector3 RightDirection => _rightDirection;

        [SerializeField] private Vector3 _leftDirection = Vector3.left;
        public Vector3 LeftDirection => _leftDirection;

        [SerializeField, Range(0f, 45f)] private float _allowedAngle = 15f;
        public float AllowedAngle => _allowedAngle;

        [Header("Destroy Settings")]
        public float DestroyZCoordinates = -5f;
        public float DestroyDelayTime = 0.3f;

        public override void InstallBindings()
        {
            Container.BindInstance(this);
        }
    }
}
