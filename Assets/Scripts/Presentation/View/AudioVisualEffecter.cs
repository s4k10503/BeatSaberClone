using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace BeatSaberClone.Presentation
{
    public sealed class AudioVisualEffecter : MonoBehaviour, IAudioVisualEffecter
    {
        [SerializeField] private Material _targetMaterial;
        [SerializeField] private GameObject _parentPillarObject1;
        [SerializeField] private GameObject _parentPillarObject2;
        [SerializeField] private GameObject _parentRingObject;
        [SerializeField] private Light _controlledLight;

        private AudioVisualEffectParameters _audioVisualEffectParameters;

        private float _intensityScale;

        private float _maxLightIntensity;

        private Color _baseFogColor;
        private Color _targetFogColor;
        private float _scaleMultiplier;
        private float _lerpSpeed;

        private float _rotationAngleMultiplier;
        private float _rotationThreshold;
        private float _durationPerChild;
        private float _delayBetweenChildren;

        private List<List<GameObject>> _pillarGroups = new();
        private List<GameObject> _ringGroup = new();
        private Color _baseEmissionColor;
        private Color _currentEmissionColor;

        private bool _isRotating = false;
        private bool _isDestroyed = false;

        [Inject]
        public void Construct(AudioVisualEffectParameters audioVisualEffectParameters)
        {
            _audioVisualEffectParameters = audioVisualEffectParameters;

            _intensityScale = _audioVisualEffectParameters.IntensityScale;
            _maxLightIntensity = _audioVisualEffectParameters.MaxLightIntensity;
            _baseFogColor = _audioVisualEffectParameters.BaseFogColor;
            _targetFogColor = _audioVisualEffectParameters.TargetFogColor;
            _scaleMultiplier = _audioVisualEffectParameters.ScaleMultiplier;
            _lerpSpeed = _audioVisualEffectParameters.LerpSpeed;
            _rotationAngleMultiplier = _audioVisualEffectParameters.RotationAngleMultiplier;
            _rotationThreshold = _audioVisualEffectParameters.RotationThreshold;
            _durationPerChild = _audioVisualEffectParameters.DurationPerChild;
            _delayBetweenChildren = _audioVisualEffectParameters.DelayBetweenChildren;
        }

        public void Initialize()
        {
            if (_isDestroyed) return;

            RenderSettings.fog = true;

            // Check if the material has an emission property
            if (_targetMaterial.HasProperty("_EmissionColor"))
            {
                // Enable Emotion Keywords
                _targetMaterial.EnableKeyword("_EMISSION");
                _baseEmissionColor = _targetMaterial.GetColor("_EmissionColor");
            }
            else
            {
                throw new ApplicationException("There is no'_emissionColor 'property in the material.Check the settings for Shader Graph.");
            }

            InitializePillarGroups(_parentPillarObject1);
            InitializePillarGroups(_parentPillarObject2);
            InitializeRingGroup(_parentRingObject);
        }

        private void OnDestroy()
        {
            if (_targetMaterial != null && _targetMaterial.HasProperty("_EmissionColor"))
            {
                _targetMaterial.SetColor("_EmissionColor", _baseEmissionColor);
            }

            _isDestroyed = true;
            _targetMaterial = null;
            _parentPillarObject1 = null;
            _parentPillarObject2 = null;
            _controlledLight = null;
        }

        private void InitializePillarGroups(GameObject parentObject)
        {
            if (parentObject != null)
            {
                var parentTransform = parentObject.transform;
                List<GameObject> cubeGroup = new(parentTransform.childCount);

                for (int i = 0; i < parentTransform.childCount; i++)
                {
                    cubeGroup.Add(parentTransform.GetChild(i).gameObject);
                }

                _pillarGroups.Add(cubeGroup);
            }
        }

        private void InitializeRingGroup(GameObject parentObject)
        {
            if (parentObject != null)
            {
                var parentTransform = parentObject.transform;
                _ringGroup = new List<GameObject>(parentTransform.childCount);
                for (int i = 0; i < parentTransform.childCount; i++)
                {
                    _ringGroup.Add(parentTransform.GetChild(i).gameObject);
                }
            }
        }

        public void UpdateEffect(float average, float[] spectrumData)
        {
            if (_isDestroyed) return;

            float intensity = Mathf.Clamp01(average * _intensityScale);
            UpdateLightIntensity(intensity);
            UpdateFogColor(intensity);
            UpdateMaterialEmission(intensity);
            UpdatePillarScales(spectrumData);
            UpdateRingRotation(intensity);
        }

        private void UpdateLightIntensity(float intensity)
        {
            _controlledLight.intensity = intensity * _maxLightIntensity;
        }

        private void UpdateFogColor(float intensity)
        {
            Color fogColor = Color.Lerp(_baseFogColor, _targetFogColor, intensity);
            RenderSettings.fogColor = fogColor;
        }

        private void UpdateMaterialEmission(float intensity)
        {
            if (_targetMaterial.HasProperty("_EmissionColor"))
            {
                Color newEmissionColor = _baseEmissionColor * intensity;
                if (_currentEmissionColor != newEmissionColor)
                {
                    _targetMaterial.SetColor("_EmissionColor", newEmissionColor);
                    _currentEmissionColor = newEmissionColor;
                }
            }
        }

        private void UpdatePillarScales(float[] spectrumData)
        {
            if (spectrumData == null || spectrumData.Length == 0)
                return;

            for (int groupIndex = 0; groupIndex < _pillarGroups.Count; groupIndex++)
            {
                List<GameObject> cubeGroup = _pillarGroups[groupIndex];
                UpdateCubeGroupScales(cubeGroup, spectrumData);
            }
        }

        private void UpdateCubeGroupScales(List<GameObject> cubeGroup, float[] spectrumData)
        {
            int limit = Mathf.Min(cubeGroup.Count, spectrumData.Length);

            for (int i = 0; i < limit; i++)
            {
                UpdatePillarScale(cubeGroup[i], spectrumData[i]);
            }
        }

        private void UpdatePillarScale(GameObject cube, float spectrumValue)
        {
            float intensity = spectrumValue * _scaleMultiplier;
            float currentScaleY = cube.transform.localScale.y;

            if (Mathf.Abs(currentScaleY - intensity) > 0.01f)
            {
                float newScaleY = Mathf.Lerp(currentScaleY, intensity, _lerpSpeed * Time.deltaTime);
                cube.transform.localScale = new Vector3(
                    cube.transform.localScale.x,
                    newScaleY,
                    cube.transform.localScale.z);
            }
        }

        private void UpdateRingRotation(float intensity)
        {
            // Note: It is a temporary process
            // It seems that it should be executed at the time of the score, like in Notes.
            if (intensity >= _rotationThreshold && !_isRotating && _ringGroup.Count > 0)
            {
                _isRotating = true;

                // Create a DOTween Sequence and rotate each child object in the ring group in turn
                Sequence rotationSequence = DOTween.Sequence();

                for (int i = 0; i < _ringGroup.Count; i++)
                {
                    GameObject child = _ringGroup[i];

                    // Set the Tween of each object to start at the timing of i * delayBetweenChildren in the sequence
                    rotationSequence.Insert(
                        i * _delayBetweenChildren,
                        child.transform.DOLocalRotate(
                            new Vector3(0f, 0f, _rotationAngleMultiplier),
                            _durationPerChild,
                            RotateMode.LocalAxisAdd
                            ).SetEase(Ease.OutQuad)
                    );
                }

                rotationSequence.OnComplete(() => { _isRotating = false; });
                rotationSequence.Play();
            }
        }
    }
}
