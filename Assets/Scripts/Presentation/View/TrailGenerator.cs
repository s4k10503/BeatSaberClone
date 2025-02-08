using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace BeatSaberClone.Presentation
{
    public sealed class TrailGenerator : ITrailGenerator
    {
        private Transform _tipTransform;
        private Transform _baseTransform;
        private GameObject _meshParent;
        private int _numVerticesPerFrame;
        private int _trailFrameLength;

        private Mesh _mesh;
        private Vector3[] _vertices;
        private int[] _triangles;
        private int _frameCount;
        private Vector3 _previousTipPosition;
        private Vector3 _previousBasePosition;

        public async UniTask InitializeAsync(
            Transform tipTransform,
            Transform baseTransform,
            GameObject meshParent,
            int numVerticesPerFrame,
            int trailFrameLength,
            CancellationToken ct)
        {
            _tipTransform = tipTransform;
            _baseTransform = baseTransform;
            _meshParent = meshParent;
            _numVerticesPerFrame = numVerticesPerFrame;
            _trailFrameLength = trailFrameLength;

            await UniTask.WhenAll(
                InitializeTrailMeshAsync(),
                InitializeTrailMaterialAsync(),
                InitializeTrailDataAsync()
            );
        }

        public void Dispose()
        {
            if (_mesh != null)
            {
                Object.Destroy(_mesh);
                _mesh = null;
            }

            _vertices = null;
            _triangles = null;
        }

        private async UniTask InitializeTrailMeshAsync()
        {
            await UniTask.SwitchToMainThread();
            _mesh = new Mesh();
            _meshParent.GetComponent<MeshFilter>().mesh = _mesh;
        }

        private async UniTask InitializeTrailMaterialAsync()
        {
            await UniTask.SwitchToMainThread();
            var trailMaterial = new Material(_meshParent.GetComponent<MeshRenderer>().sharedMaterial);
            _meshParent.GetComponent<MeshRenderer>().sharedMaterial = trailMaterial;
        }

        private async UniTask InitializeTrailDataAsync()
        {
            await UniTask.RunOnThreadPool(() =>
            {
                int totalVertices = _trailFrameLength * _numVerticesPerFrame;
                _vertices = new Vector3[totalVertices];
                _triangles = new int[totalVertices];
            });

            await UniTask.SwitchToMainThread();
            _previousTipPosition = _tipTransform.position;
            _previousBasePosition = _baseTransform.position;
        }

        public async UniTask UpdateTrailAsync(CancellationToken ct)
        {
            await UniTask.Yield();

            if (_frameCount >= _trailFrameLength * _numVerticesPerFrame)
            {
                _frameCount = 0;
                _mesh.Clear();
            }

            UpdateVertices();
            UpdateTriangles();
            ApplyMeshChanges();
            CacheCurrentPositions();
            _frameCount += _numVerticesPerFrame;
        }

        private void UpdateVertices()
        {
            _vertices[_frameCount] = _baseTransform.position;
            _vertices[_frameCount + 1] = _tipTransform.position;
            _vertices[_frameCount + 2] = _previousTipPosition;
            _vertices[_frameCount + 3] = _baseTransform.position;
            _vertices[_frameCount + 4] = _previousTipPosition;
            _vertices[_frameCount + 5] = _tipTransform.position;

            _vertices[_frameCount + 6] = _previousTipPosition;
            _vertices[_frameCount + 7] = _baseTransform.position;
            _vertices[_frameCount + 8] = _previousBasePosition;
            _vertices[_frameCount + 9] = _previousTipPosition;
            _vertices[_frameCount + 10] = _previousBasePosition;
            _vertices[_frameCount + 11] = _baseTransform.position;
        }

        private void UpdateTriangles()
        {
            for (int i = 0; i < _numVerticesPerFrame; i++)
            {
                _triangles[_frameCount + i] = _frameCount + i;
            }
        }

        private void ApplyMeshChanges()
        {
            _mesh.Clear();
            _mesh.vertices = _vertices;
            _mesh.triangles = _triangles;
            _mesh.RecalculateBounds();
        }

        private void CacheCurrentPositions()
        {
            _previousTipPosition = _tipTransform.position;
            _previousBasePosition = _baseTransform.position;
        }
    }
}
