using System.Threading;

namespace BeatSaberClone.Presentation
{
    public struct SpawnSettings
    {
        public int Type;
        public float OriginalY;
        public int CutDirection;
        public CancellationToken Ct;

        public MovementSettings MovementSettings;
        public RotationSettings RotationSettings;
        public DestroySettings DestroySettings;
        public SlicedSettings SlicedSettings;
    }
}
