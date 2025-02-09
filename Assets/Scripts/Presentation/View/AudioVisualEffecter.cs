using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace BeatSaberClone.Presentation
{
    public sealed class AudioVisualEffecter : MonoBehaviour, IAudioVisualEffecter
    {
        [Header("Material Settings")]
        [SerializeField] private Material _luminousMaterial;
        [SerializeField] private Material _smokeMaterial;

        [Header("Level Settings")]
        [SerializeField] private Light _directionalLight;
        [SerializeField] private GameObject _parentPillarObjectL;
        [SerializeField] private GameObject _parentPillarObjectR;
        [SerializeField] private GameObject _parentRingObject;

        private AudioVisualEffectParameters _audioVisualEffectParameters;

        private Color _baseColor;
        private Color _flashColor;
        private float _intensityScale;

        private float _scaleMultiplier;
        private float _lerpSpeed;
        private float _rotationAngleMultiplier;
        private float _rotationThreshold;
        private float _durationPerChild;
        private float _delayBetweenChildren;

        private List<List<GameObject>> _pillarGroups = new();
        private List<GameObject> _ringGroup = new();

        private Color _baseLightColor;

        // Structures for integrating fields of luminescent information
        private struct EmissionInfo
        {
            public Color OriginalColor; // Early Emission Color (for restore with OnDestroy)
            public float Intensity;     // Initial Intensity (Maximum Ingredients)
            public Color Normalized;    // Normalized color (divided by Intensity)
        }

        private EmissionInfo _luminousEmission;
        private EmissionInfo _smokeEmission;

        private bool _previousIntensityWasOne = false;
        private bool _isFlashActive = false;
        private bool _isRotating = false;

        [Inject]
        public void Construct(AudioVisualEffectParameters audioVisualEffectParameters)
        {
            try
            {
                _audioVisualEffectParameters = audioVisualEffectParameters;

                _baseColor = _audioVisualEffectParameters.BaseColor;
                _flashColor = _audioVisualEffectParameters.FlashColor;

                _intensityScale = _audioVisualEffectParameters.IntensityScale;
                _scaleMultiplier = _audioVisualEffectParameters.ScaleMultiplier;
                _lerpSpeed = _audioVisualEffectParameters.LerpSpeed;

                _rotationAngleMultiplier = _audioVisualEffectParameters.RotationAngleMultiplier;
                _rotationThreshold = _audioVisualEffectParameters.RotationThreshold;
                _durationPerChild = _audioVisualEffectParameters.DurationPerChild;
                _delayBetweenChildren = _audioVisualEffectParameters.DelayBetweenChildren;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in Construct: " + ex);
            }
        }

        public void Initialize()
        {
            try
            {
                RenderSettings.fog = true;

                InitializeMaterial(_luminousMaterial, isLuminous: true);
                InitializeMaterial(_smokeMaterial, isLuminous: false);

                if (_directionalLight != null)
                    _baseLightColor = _directionalLight.color;

                if (_parentPillarObjectL != null)
                    _pillarGroups.Add(GetChildGameObjects(_parentPillarObjectL));
                if (_parentPillarObjectR != null)
                    _pillarGroups.Add(GetChildGameObjects(_parentPillarObjectR));
                if (_parentRingObject != null)
                    _ringGroup = GetChildGameObjects(_parentRingObject);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in Initialize: " + ex);
            }
        }

        // Helper to retrieve all child objects of the specified parent object as a list
        private List<GameObject> GetChildGameObjects(GameObject parent)
        {
            List<GameObject> children = new(parent.transform.childCount);
            try
            {
                for (int i = 0; i < parent.transform.childCount; i++)
                {
                    children.Add(parent.transform.GetChild(i).gameObject);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in GetChildGameObjects: " + ex);
            }

            return children;
        }

        private void OnDestroy()
        {
            try
            {
                RestoreMaterialEmission(_luminousMaterial, _luminousEmission.OriginalColor);
                RestoreMaterialEmission(_smokeMaterial, _smokeEmission.OriginalColor);

                _luminousMaterial = null;
                _smokeMaterial = null;
                _parentPillarObjectL = null;
                _parentPillarObjectR = null;
                _directionalLight = null;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in OnDestroy: " + ex);
            }
        }

        #region Initialization Helpers

        // Obtains the material's Emission information and stores it in a structure
        private void InitializeMaterial(Material material, bool isLuminous)
        {
            if (material == null)
                return;

            try
            {
                if (material.HasProperty("_EmissionColor"))
                {
                    material.EnableKeyword("_EMISSION");
                    Color baseColor = material.GetColor("_EmissionColor");
                    float intensity = baseColor.maxColorComponent;
                    Color normalized = intensity > 0 ? baseColor / intensity : baseColor;

                    EmissionInfo emissionInfo = new()
                    {
                        OriginalColor = baseColor,
                        Intensity = intensity,
                        Normalized = normalized
                    };

                    if (isLuminous)
                    {
                        _luminousEmission = emissionInfo;
                    }
                    else
                    {
                        _smokeEmission = emissionInfo;
                    }
                }
                else if (isLuminous)
                {
                    throw new ApplicationException("There is no '_EmissionColor' property in the luminous material. Check the settings for Shader Graph.");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in InitializeMaterial: " + ex);
            }
        }

        // Returns the emission color of the material to its initial value.
        private void RestoreMaterialEmission(Material material, Color baseColor)
        {
            try
            {
                if (material != null && material.HasProperty("_EmissionColor"))
                {
                    material.SetColor("_EmissionColor", baseColor);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in RestoreMaterialEmission: " + ex);
            }
        }

        #endregion

        public void UpdateEffect(float average, float[] spectrumData)
        {
            try
            {
                float intensity = Mathf.Clamp01(average * _intensityScale);

                // Note: Temporary processing.You should create a score and switch the timing.
                // Detects and toggles the moment when intensity reaches 1
                if (Mathf.Approximately(intensity, 1f) && !_previousIntensityWasOne)
                {
                    ToggleColors();
                    _previousIntensityWasOne = true;
                }
                else if (!Mathf.Approximately(intensity, 1f))
                {
                    _previousIntensityWasOne = false;
                }

                UpdatePillarScales(spectrumData);
                UpdateRingRotation(intensity);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in UpdateEffect: " + ex);
            }
        }

        #region Toggle & Emission Helpers

        // Switching between flash and normal states
        private void ToggleColors()
        {
            try
            {
                bool newFlashState = !_isFlashActive;
                SetGlobalColors(newFlashState);

                // Emission updates for each material
                SetEmissionForMaterial(_luminousMaterial, _luminousEmission.Normalized, _luminousEmission.Intensity, newFlashState);
                SetEmissionForMaterial(_smokeMaterial, _smokeEmission.Normalized, _smokeEmission.Intensity, newFlashState);

                _isFlashActive = newFlashState;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in ToggleColors: " + ex);
            }
        }

        // Updated RenderSettings.fog and DirectionalLight colors
        private void SetGlobalColors(bool flash)
        {
            try
            {
                if (flash)
                {
                    RenderSettings.fogColor = _flashColor;
                    if (_directionalLight != null)
                        _directionalLight.color = _flashColor;
                }
                else
                {
                    RenderSettings.fogColor = _baseColor;
                    if (_directionalLight != null)
                        _directionalLight.color = _baseLightColor;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in SetGlobalColors: " + ex);
            }
        }

        // Updated material's Emission color (changed between Flash and normal state)
        private void SetEmissionForMaterial(Material material, Color baseNormalized, float baseIntensity, bool flash)
        {
            try
            {
                if (material != null && material.HasProperty("_EmissionColor"))
                {
                    Color emission = flash ? NormalizeColor(_flashColor) * baseIntensity : baseNormalized * baseIntensity;
                    material.SetColor("_EmissionColor", emission);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in SetEmissionForMaterial: " + ex);
            }
        }

        // Normalize the specified color (divided by maximum component)
        private Color NormalizeColor(Color color)
        {
            try
            {
                float maxComponent = color.maxColorComponent;
                return maxComponent > 0 ? color / maxComponent : color;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in NormalizeColor: " + ex);
            }
        }

        #endregion

        #region Pillar & Ring Update

        private void UpdatePillarScales(float[] spectrumData)
        {
            try
            {
                if (spectrumData == null || spectrumData.Length == 0)
                    return;

                foreach (List<GameObject> cubeGroup in _pillarGroups)
                {
                    UpdateCubeGroupScales(cubeGroup, spectrumData);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in UpdatePillarScales: " + ex);
            }
        }

        private void UpdateCubeGroupScales(List<GameObject> cubeGroup, float[] spectrumData)
        {
            try
            {
                int limit = Mathf.Min(cubeGroup.Count, spectrumData.Length);
                for (int i = 0; i < limit; i++)
                {
                    UpdatePillarScale(cubeGroup[i], spectrumData[i]);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in UpdateCubeGroupScales: " + ex);
            }
        }

        // Updated Y scale for each pillar with Lerp
        private void UpdatePillarScale(GameObject cube, float spectrumValue)
        {
            try
            {
                float targetScaleY = spectrumValue * _scaleMultiplier;
                float currentScaleY = cube.transform.localScale.y;

                if (Mathf.Abs(currentScaleY - targetScaleY) > 0.01f)
                {
                    float newScaleY = Mathf.Lerp(currentScaleY, targetScaleY, _lerpSpeed * Time.deltaTime);
                    Vector3 currentScale = cube.transform.localScale;
                    cube.transform.localScale = new Vector3(currentScale.x, newScaleY, currentScale.z);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in UpdatePillarScale: " + ex);
            }
        }

        // Run a ring rotation animation
        private void UpdateRingRotation(float intensity)
        {
            try
            {
                // Note: It is a temporary process
                // It seems that it should be executed at the time of the score, like in Notes.
                if (intensity < _rotationThreshold || _isRotating || _ringGroup.Count == 0)
                    return;

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
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in UpdateRingRotation: " + ex);
            }
        }

        #endregion
    }
}
