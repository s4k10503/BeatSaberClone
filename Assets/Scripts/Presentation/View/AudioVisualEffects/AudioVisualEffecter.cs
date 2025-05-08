using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace BeatSaberClone.Presentation
{
    public sealed class AudioVisualEffecter : MonoBehaviour, IAudioVisualEffecter
    {
        [Header("Platform Specific Settings")]
        [SerializeField] private GameObject _platformSpecificPrefab;

        [Header("Material Settings")]
        [SerializeField] private Material _luminousMaterial;
        [SerializeField] private Material _smokeMaterial;

        [Header("Light Settings")]
        [SerializeField] private Light _directionalLight;
        [SerializeField] private Light _pointlLight;

        [Header("Pillar Settings")]
        [SerializeField] private GameObject _parentPillarObjectL;
        [SerializeField] private GameObject _parentPillarObjectR;

        [Header("Ring Settings")]
        [SerializeField] private GameObject _parentRingObject;

        [Header("Laser System Settings")]
        [SerializeField] private List<LaserBeam> _laserBeams = new();
        [SerializeField] private List<FanLaserController> _fanLasers = new();
        [SerializeField] private List<LaserAnimationController> _animationControllers = new();
        [SerializeField, Range(0f, 1f)] private float _laserIntensityMultiplier = 1f;
        [SerializeField] private bool _syncLaserIntensity = true;
        [SerializeField] private bool _syncLaserColor = false;

        private Color _materialBaseColor;
        private Color _materialFlashColor;
        private Color _lightBaseColor;
        private Color _lightFlashColor;
        private Color _fogBaseColor;
        private Color _fogFlashColor;

        private AudioVisualEffectParameters _audioVisualEffectParameters;

        private float _intensityScale;
        private float _scaleMultiplier;
        private float _lerpSpeed;
        private float _rotationAngleMultiplier;
        private float _rotationThreshold;
        private float _durationPerChild;
        private float _delayBetweenChildren;

        private List<List<GameObject>> _pillarGroups = new();
        private List<GameObject> _ringGroup = new();

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

                _materialBaseColor = _audioVisualEffectParameters.MaterialBaseColor;
                _materialFlashColor = _audioVisualEffectParameters.MaterialFlashColor;
                _lightBaseColor = _audioVisualEffectParameters.LightBaseColor;
                _lightFlashColor = _audioVisualEffectParameters.LightFlashColor;
                _fogBaseColor = _audioVisualEffectParameters.FogBaseColor;
                _fogFlashColor = _audioVisualEffectParameters.FogFlashColor;

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
                if (_parentPillarObjectL != null)
                    _pillarGroups.Add(GetChildGameObjects(_parentPillarObjectL));

                if (_parentPillarObjectR != null)
                    _pillarGroups.Add(GetChildGameObjects(_parentPillarObjectR));

                if (_parentRingObject != null)
                    _ringGroup = GetChildGameObjects(_parentRingObject);

                RenderSettings.fog = true;
                SetGlobalColors(false);
                InitializeMaterial(_luminousMaterial, isLuminous: true);
                InitializeMaterial(_smokeMaterial, isLuminous: false);
                ApplyLaserMaterialToAll();

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
                if (_platformSpecificPrefab != null)
                {
                    Instantiate(_platformSpecificPrefab);
                }
#endif
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
                SetGlobalColors(false);
                RenderSettings.fogColor = _fogBaseColor;
                RestoreMaterialEmission(_luminousMaterial, _luminousEmission.OriginalColor);
                RestoreMaterialEmission(_smokeMaterial, _smokeEmission.OriginalColor);

                _luminousMaterial = null;
                _smokeMaterial = null;
                _parentPillarObjectL = null;
                _parentPillarObjectR = null;
                _directionalLight = null;
                _pointlLight = null;
                _parentRingObject = null;

                // レーザーシステムの参照をクリア
                _laserBeams.Clear();
                _fanLasers.Clear();
                _animationControllers.Clear();
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
                UpdateLaserSystem();
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
                    RenderSettings.fogColor = _fogFlashColor;
                    if (_directionalLight != null && _pointlLight != null)
                    {
                        _pointlLight.color = _lightFlashColor;
                        _directionalLight.color = _lightFlashColor;
                    }
                }
                else
                {
                    RenderSettings.fogColor = _fogBaseColor;
                    if (_directionalLight != null && _pointlLight != null)
                    {
                        _pointlLight.color = _lightBaseColor;
                        _directionalLight.color = _lightBaseColor;
                    }
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
                    Color emission = flash ?
                        NormalizeColor(_materialFlashColor) * baseIntensity :
                        NormalizeColor(_materialBaseColor) * baseIntensity;
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

        #region Laser System Update

        private void UpdateLaserSystem()
        {
            try
            {
                if (_syncLaserIntensity || _syncLaserColor)
                {
                    float laserIntensity = Mathf.Clamp01(_laserIntensityMultiplier);
                    UpdateLaserBeams(laserIntensity);
                    UpdateFanLasers(laserIntensity);
                }

                // Ensure animation controllers are actually playing
                foreach (var animController in _animationControllers)
                {
                    if (animController == null) continue;

                    // If the controller has an animation type set but isn't playing, start it
                    if (animController.GetAnimationType != BeatSaberClone.Presentation.LaserAnimationController.AnimationType.None &&
                        !animController.IsPlaying)
                    {
                        animController.Play();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in UpdateLaserSystem: " + ex);
            }
        }

        private void UpdateLaserBeams(float intensity)
        {
            try
            {
                if (_laserBeams.Count == 0) return;

                // レーザーマテリアルに発光色を設定（ベース色または点滅色）
                if (_luminousMaterial != null)
                {
                    // マテリアルのエミッション色を設定
                    if (_luminousMaterial.HasProperty("_EmissionColor"))
                    {
                        // 現在の状態（通常またはフラッシュ）に基づいて色を選択
                        Color baseColor = _isFlashActive ? _materialFlashColor : _materialBaseColor;

                        // 強度を適用（音楽同期が有効な場合）
                        float appliedIntensity = _syncLaserIntensity ? intensity * _laserIntensityMultiplier : 1.0f;

                        // 正規化された色に強度を掛けてエミッション色を設定
                        Color emission = NormalizeColor(baseColor) * _luminousEmission.Intensity * appliedIntensity;
                        _luminousMaterial.SetColor("_EmissionColor", emission);
                        _luminousMaterial.EnableKeyword("_EMISSION");
                    }
                }

                // 各LineRendererの色を設定
                foreach (var laser in _laserBeams)
                {
                    if (laser == null) continue;

                    // LineRendererを直接取得
                    LineRenderer lineRenderer = laser.GetLineRenderer();
                    if (lineRenderer == null) continue;

                    // 状態に合わせて色を設定（マテリアルは共有済み）
                    Color color = _isFlashActive ? _materialFlashColor : _materialBaseColor;
                    lineRenderer.startColor = color;
                    lineRenderer.endColor = color;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in UpdateLaserBeams: " + ex);
            }
        }

        private void UpdateFanLasers(float intensity)
        {
            try
            {
                foreach (var fanLaser in _fanLasers)
                {
                    if (fanLaser == null) continue;

                    // ファンの全ビームを取得して個別に更新
                    List<LaserBeam> beams = fanLaser.GetLaserBeams();
                    foreach (var beam in beams)
                    {
                        if (beam == null) continue;

                        // LineRendererを直接取得
                        LineRenderer lineRenderer = beam.GetLineRenderer();
                        if (lineRenderer == null) continue;

                        // 状態に合わせて色を設定（マテリアルは共有済み）
                        Color color = _isFlashActive ? _materialFlashColor : _materialBaseColor;
                        lineRenderer.startColor = color;
                        lineRenderer.endColor = color;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in UpdateFanLasers: " + ex);
            }
        }

        #endregion


        // 単一のレーザービームにマテリアルを適用
        private void ApplyMaterialToLaserBeam(LaserBeam laser)
        {
            if (laser == null || _luminousMaterial == null) return;

            try
            {
                LineRenderer lineRenderer = laser.GetLineRenderer();
                if (lineRenderer != null)
                {
                    lineRenderer.sharedMaterial = _luminousMaterial;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to apply material to laser: {ex.Message}");
            }
        }

        // ファンレーザーの全ビームにマテリアルを適用
        private void ApplyMaterialToFanLaser(FanLaserController fanLaser)
        {
            if (fanLaser == null) return;

            List<LaserBeam> beams = fanLaser.GetLaserBeams();
            foreach (var beam in beams)
            {
                if (beam != null)
                {
                    ApplyMaterialToLaserBeam(beam);
                }
            }
        }

        // すべてのレーザーに共有マテリアルを適用
        private void ApplyLaserMaterialToAll()
        {
            // 単一レーザーに適用
            foreach (var laser in _laserBeams)
            {
                ApplyMaterialToLaserBeam(laser);
            }

            // ファンレーザーに適用
            foreach (var fanLaser in _fanLasers)
            {
                ApplyMaterialToFanLaser(fanLaser);
            }
        }
    }
}
