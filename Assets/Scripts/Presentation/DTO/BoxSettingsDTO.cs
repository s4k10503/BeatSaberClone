using UnityEngine;

namespace BeatSaberClone.Presentation
{
    public struct MovementSettings
    {
        public float LerpSpeed;
        public float InitialMoveSpeed;
        public float FinalMoveSpeed;
        public float SlowDownDistance;
        public float LowestY;
    }

    public struct RotationSettings
    {
        public float RotationDuration;
        public float RotationDelayTime;
    }

    public struct DestroySettings
    {
        public float DestroyZCoordinates;
        public float DestroyDelayTime;
    }

    public struct SlicedSettings
    {
        // Rotation angle for each cutting direction (Z-axis)
        public float DownRotationZ;
        public float UpRotationZ;
        public float RightRotationZ;
        public float LeftRotationZ;

        // Expected direction for each cutting direction
        public Vector3 DownDirection;
        public Vector3 UpDirection;
        public Vector3 RightDirection;
        public Vector3 LeftDirection;

        // Slice angle tolerance
        public float AllowedAngle;
    }
}
