using UnityEngine;

namespace BeatSaberClone.Presentation
{
    public sealed class SphereCastSliceDetector : ISliceDetector
    {
        private readonly LayerMask _slicableLayer;
        private readonly Transform _startSlicePoint;
        private readonly Transform _endSlicePoint;
        private readonly float _detectionRadius;
        private readonly int _interpolationSteps;

        public SphereCastSliceDetector(
            Transform startSlicePoint,
            Transform endSlicePoint,
            LayerMask slicableLayer,
            float detectionRadius,
            int interpolationSteps)
        {
            _startSlicePoint = startSlicePoint;
            _endSlicePoint = endSlicePoint;
            _slicableLayer = slicableLayer;
            _detectionRadius = detectionRadius;
            _interpolationSteps = Mathf.Max(1, interpolationSteps);
        }

        public void Dispose()
        {
        }

        public bool CheckForSlice(out GameObject slicedObject)
        {
            slicedObject = null;
            bool hasHit = false;

            // Interpolate and divide into multiple segments to make a judgment
            for (int i = 0; i < _interpolationSteps; i++)
            {
                float t0 = (float)i / _interpolationSteps;
                float t1 = (float)(i + 1) / _interpolationSteps;
                Vector3 segmentStart = Vector3.Lerp(_startSlicePoint.position, _endSlicePoint.position, t0);
                Vector3 segmentEnd = Vector3.Lerp(_startSlicePoint.position, _endSlicePoint.position, t1);

                Vector3 direction = (segmentEnd - segmentStart).normalized;
                float distance = Vector3.Distance(segmentStart, segmentEnd);

                if (Physics.SphereCast(
                    segmentStart,
                    _detectionRadius,
                    direction,
                    out RaycastHit hit,
                    distance,
                    _slicableLayer))
                {
                    slicedObject = hit.transform.gameObject;
                    hasHit = true;
                    break;
                }
            }
            return hasHit;
        }
    }
}
