using UnityEngine;
using UniRx;
using BeatSaberClone.Domain;
using BeatSaberClone.UseCase;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Zenject;

namespace BeatSaberClone.Presentation
{
    public sealed class ObjectSlicer : MonoBehaviour, IObjectSlicer
    {
        [Header("Slicer Settings")]
        [SerializeField] private SlicerSide _slicerSide;
        [SerializeField] private LayerMask _slicableLayer;
        [SerializeField] private Material _crossSectionMaterial;
        [SerializeField] private GameObject _particleEffect;
        [SerializeField] private VelocityEstimator _velocityEstimator;

        private readonly ReactiveProperty<GameObject> _hitObject = new ReactiveProperty<GameObject>();

        public SlicerSide Side => _slicerSide;
        public IReadOnlyReactiveProperty<GameObject> HitObject => _hitObject;
        public Material CrossSectionMaterial => _crossSectionMaterial;
        public Vector3 PlaneNormal { get; private set; }
        public Transform TipTransform { get; private set; }
        public Vector3 SliceDirection { get; private set; }
        public Vector3 Velocity { get; private set; }

        [Header("Trail Settings")]
        [SerializeField] private GameObject _tip;
        [SerializeField] private GameObject _base;
        [SerializeField] private GameObject _meshParent;
        [SerializeField] private TrailSettings _trailSettings;
        private int _numVerticesPerFrame;
        private int _trailFrameLength;

        private ISliceDetector _sliceDetector;
        private IParticleEffectHandler _particleEffectHandler;
        private ITrailGenerator _trailGenerator;

        [Header("Particle Effect Settings")]
        [SerializeField] private ParticleEffectSettings _particleEffectSettings;

        private float _particleDelaytime;
        private int _defaultPoolCapacity;
        private int _maxPoolSize;

        [Inject]
        public void Construct(
            IParticleEffectHandler particleEffectHandler,
            ITrailGenerator trailGenerator)
        {
            TipTransform = _tip.transform;
            _sliceDetector = new LinecastSliceDetector(_base.transform, _tip.transform, _slicableLayer);

            _trailGenerator = trailGenerator;
            _numVerticesPerFrame = _trailSettings.NumVerticesPerFrame;
            _trailFrameLength = _trailSettings.TrailFrameLength;

            _particleEffectHandler = particleEffectHandler;
            _particleDelaytime = _particleEffectSettings.ParticleDelayTime;
            _defaultPoolCapacity = _particleEffectSettings.DefaultPoolCapacity;
            _maxPoolSize = _particleEffectSettings.MaxPoolSize;
        }

        public async UniTask Initialize(CancellationToken ct)
        {
            // Fix: Comment out because the drawing position of the trail is strange
            // await _trailGenerator.Initialize(_tip.transform, _base.transform, _meshParent, _numVerticesPerFrame, _trailFrameLength, ct);

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: ct);
            gameObject.SetActive(true);
        }

        public void UpdateTrail()
        {
            // _trailGenerator.UpdateTrail();
        }

        private void OnDestroy()
        {
            _hitObject.Dispose();
            _trailGenerator.Dispose();
            _particleEffectHandler = null;
            _trailGenerator = null;
            _sliceDetector = null;
            _velocityEstimator = null;
            _crossSectionMaterial = null;
            _particleEffect = null;
        }

        public void SliceDetection(CancellationToken ct)
        {
            if (_sliceDetector?.CheckForSlice(out GameObject slicedObject) == true)
            {
                ProcessSlice(slicedObject, ct);
            }
        }

        private void ProcessSlice(GameObject slicedObject, CancellationToken ct)
        {
            CalculateSliceData();
            _hitObject.Value = slicedObject;
            _particleEffectHandler?.TriggerParticleEffect(
                _particleEffect,
                transform.position,
                Quaternion.identity,
                _particleDelaytime,
                _defaultPoolCapacity,
                _maxPoolSize,
                ct);
        }

        private void CalculateSliceData()
        {
            SliceDirection = _tip.transform.position - _base.transform.position;
            Velocity = _velocityEstimator.GetVelocityEstimate();
            PlaneNormal = Vector3.Cross(SliceDirection, Velocity).normalized;
        }
    }
}
