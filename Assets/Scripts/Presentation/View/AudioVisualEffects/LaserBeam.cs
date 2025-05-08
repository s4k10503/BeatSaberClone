using UnityEngine;

namespace BeatSaberClone.Presentation
{
    /// <summary>
    /// Controls individual laser beam rendering and parameters
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class LaserBeam : MonoBehaviour
    {
        [Header("Laser Parameters")]
        [SerializeField] private bool _enabled = true;
        [SerializeField] private float _width = 0.1f;
        [SerializeField] private float _maxLength = 100.0f;
        [SerializeField] private Vector3 _direction = Vector3.forward;
        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();

            // Setup line renderer basic properties
            _lineRenderer.positionCount = 2;
            _lineRenderer.useWorldSpace = false;

            // Width initial setting
            _lineRenderer.startWidth = _width;
            _lineRenderer.endWidth = _width;

            // Initial color is set to white
            _lineRenderer.startColor = Color.white;
            _lineRenderer.endColor = Color.white;
        }

        private void Update()
        {
            if (_enabled)
            {
                UpdateLaserBeam();
            }
            else
            {
                _lineRenderer.enabled = false;
            }
        }

        private void UpdateLaserBeam()
        {
            // Set start and end positions
            Vector3 startPos = Vector3.zero;
            Vector3 endPos = _direction.normalized * _maxLength;

            // Perform raycast to check for obstacles
            if (Physics.Raycast(transform.position, transform.TransformDirection(_direction), out RaycastHit hit, _maxLength))
            {
                // If something was hit, set the end position to the hit point (in local space)
                endPos = transform.InverseTransformPoint(hit.point);
            }

            // Update line renderer positions
            _lineRenderer.SetPosition(0, startPos);
            _lineRenderer.SetPosition(1, endPos);
        }

        /// <summary>
        /// Enables or disables the laser beam
        /// </summary>
        public void SetEnabled(bool isEnabled)
        {
            _enabled = isEnabled;
        }

        /// <summary>
        /// Sets the laser beam _width
        /// </summary>
        public void SetWidth(float newWidth)
        {
            _width = Mathf.Max(0.01f, newWidth);
            if (_lineRenderer != null)
            {
                _lineRenderer.startWidth = _width;
                _lineRenderer.endWidth = _width;
            }
        }

        /// <summary>
        /// Sets the laser beam maximum length
        /// </summary>
        public void SetMaxLength(float newMaxLength)
        {
            _maxLength = Mathf.Max(0.1f, newMaxLength);
        }

        /// <summary>
        /// Sets the laser beam _direction (local space)
        /// </summary>
        public void SetDirection(Vector3 newDirection)
        {
            if (newDirection != Vector3.zero)
            {
                _direction = newDirection;
            }
        }

        /// <summary>
        /// LineRendererへの参照を取得
        /// </summary>
        public LineRenderer GetLineRenderer()
        {
            return _lineRenderer;
        }
    }
}
