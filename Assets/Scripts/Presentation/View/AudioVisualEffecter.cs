using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace BeatSaberClone.Presentation
{
    public sealed class AudioVisualEffecter : MonoBehaviour, IAudioVisualEffecter
    {
        [SerializeField] private Material _targetMaterial;
        [SerializeField] private GameObject _parentObject1;
        [SerializeField] private GameObject _parentObject2;
        [SerializeField] private Light _controlledLight;

        private float _maxLightIntensity;
        private float _intensityScale;
        private Color _baseFogColor;
        private Color _targetFogColor;
        private float _scaleMultiplier;
        private float _lerpSpeed;

        private List<List<GameObject>> _cubeGroups = new();
        private Color _baseEmissionColor;
        private Color _currentEmissionColor;

        private bool _isDestroyed = false;

        [Inject]
        public void Construct(
            [Inject(Id = "MaxLightIntensity")] float maxLightIntensity,
            [Inject(Id = "IntensityScale")] float intensityScale,
            [Inject(Id = "ScaleMultiplier")] float scaleMultiplier,
            [Inject(Id = "VisualEffectLerpSpeed")] float lerpSpeed,
            [Inject(Id = "BaseFogColor")] Color baseFogColor,
            [Inject(Id = "TargetFogColor")] Color targetFogColor
        )
        {
            _maxLightIntensity = maxLightIntensity;
            _intensityScale = intensityScale;
            _scaleMultiplier = scaleMultiplier;
            _lerpSpeed = lerpSpeed;
            _baseFogColor = baseFogColor;
            _targetFogColor = targetFogColor;
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

            InitializeCubeGroups(_parentObject1);
            InitializeCubeGroups(_parentObject2);
        }

        private void OnDestroy()
        {
            if (_targetMaterial != null && _targetMaterial.HasProperty("_EmissionColor"))
            {
                _targetMaterial.SetColor("_EmissionColor", _baseEmissionColor);
            }

            _isDestroyed = true;
            _targetMaterial = null;
            _parentObject1 = null;
            _parentObject2 = null;
            _controlledLight = null;
        }

        private void InitializeCubeGroups(GameObject parentObject)
        {
            if (parentObject != null)
            {
                var parentTransform = parentObject.transform;
                List<GameObject> cubeGroup = new(parentTransform.childCount);

                for (int i = 0; i < parentTransform.childCount; i++)
                {
                    cubeGroup.Add(parentTransform.GetChild(i).gameObject);
                }

                _cubeGroups.Add(cubeGroup);
            }
        }

        public void UpdateEffect(float average, float[] spectrumData)
        {
            if (_isDestroyed) return;

            float intensity = Mathf.Clamp01(average * _intensityScale);
            UpdateLightIntensity(intensity);
            UpdateFogColor(intensity);
            UpdateMaterialEmission(intensity);
            UpdateObjectScales(spectrumData);
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

        private void UpdateObjectScales(float[] spectrumData)
        {
            if (spectrumData == null || spectrumData.Length == 0)
                return;

            for (int groupIndex = 0; groupIndex < _cubeGroups.Count; groupIndex++)
            {
                List<GameObject> cubeGroup = _cubeGroups[groupIndex];
                UpdateCubeGroupScales(cubeGroup, spectrumData);
            }
        }

        private void UpdateCubeGroupScales(List<GameObject> cubeGroup, float[] spectrumData)
        {
            int limit = Mathf.Min(cubeGroup.Count, spectrumData.Length);

            for (int i = 0; i < limit; i++)
            {
                UpdateCubeScale(cubeGroup[i], spectrumData[i]);
            }
        }

        private void UpdateCubeScale(GameObject cube, float spectrumValue)
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
    }
}
