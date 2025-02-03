using UnityEngine;

namespace BeatSaberClone.UseCase
{
    public sealed class LinecastSliceDetector : ISliceDetector
    {
        private readonly LayerMask slicableLayer;
        private readonly Transform startSlicePoint;
        private readonly Transform endSlicePoint;

        public LinecastSliceDetector(Transform startSlicePoint, Transform endSlicePoint, LayerMask slicableLayer)
        {
            this.startSlicePoint = startSlicePoint;
            this.endSlicePoint = endSlicePoint;
            this.slicableLayer = slicableLayer;
        }

        public void Dispose()
        {
        }

        public bool CheckForSlice(out GameObject slicedObject)
        {
            bool hasHit = Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, slicableLayer);
            slicedObject = hasHit ? hit.transform.gameObject : null;
            return hasHit;
        }
    }
}