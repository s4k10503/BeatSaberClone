using UnityEngine;
using System.Collections.Generic;

namespace BeatSaberClone.Presentation
{
    /// <summary>
    /// Adds animation capabilities to laser beams and fan lasers
    /// </summary>
    public class LaserAnimationController : MonoBehaviour
    {
        public enum AnimationType
        {
            None,
            TriAxisRotate,
            IndividualBeamRotate
        }

        public enum SpreadAnimationType
        {
            None,
            Sine,
            PingPong,
            Random
        }

        public enum RotationDirection
        {
            Clockwise,
            CounterClockwise
        }

        public AnimationType GetAnimationType => _animationType;
        public bool IsPlaying => _isPlaying;

        [Header("Animation Settings")]
        [SerializeField] private AnimationType _animationType = AnimationType.None;
        [SerializeField] private bool _playOnStart = true;

        [Header("Rotation Angle Constraints")]
        [SerializeField, Range(-180f, 180f)] private float _minXRotationAngle = -45f;
        [SerializeField, Range(-180f, 180f)] private float _maxXRotationAngle = 45f;
        [SerializeField, Range(-180f, 180f)] private float _minYRotationAngle = -45f;
        [SerializeField, Range(-180f, 180f)] private float _maxYRotationAngle = 45f;
        [SerializeField, Range(-180f, 180f)] private float _minZRotationAngle = -45f;
        [SerializeField, Range(-180f, 180f)] private float _maxZRotationAngle = 45f;
        [SerializeField] private bool _useAngleLimits = true;

        [Header("Shared Rotation Settings")]
        [SerializeField, Range(0f, 1f)] private float _xAxisInfluence = 0f;
        [SerializeField, Range(0f, 1f)] private float _yAxisInfluence = 0f;
        [SerializeField, Range(0f, 1f)] private float _zAxisInfluence = 0f;
        [SerializeField, Range(0f, 50f)] private float _rotationSpeed = 1f;

        [Header("Tri-Axis Rotation Settings")]
        [SerializeField] private bool _animateXAxis = true;
        [SerializeField] private bool _animateYAxis = true;
        [SerializeField] private bool _animateZAxis = true;
        [SerializeField] private RotationDirection _xAxisRotationDirection = RotationDirection.Clockwise;
        [SerializeField] private RotationDirection _yAxisRotationDirection = RotationDirection.Clockwise;
        [SerializeField] private RotationDirection _zAxisRotationDirection = RotationDirection.Clockwise;

        [Header("Individual Beam Rotation Settings")]
        [SerializeField] private bool _randomizeBeamRotations = true;
        [SerializeField, Range(0f, 180f)] private float _maxBeamRotationAngle = 45f;
        [SerializeField, Range(0.1f, 10f)] private float _beamPhaseVariance = 1f;

        [Header("Spread Animation Settings")]
        [SerializeField] private SpreadAnimationType _spreadAnimType = SpreadAnimationType.None;
        [SerializeField, Range(0f, 10f)] private float _spreadAnimSpeed = 1f;
        [SerializeField, Range(0f, 180f)] private float _minSpreadAngle = 20f;
        [SerializeField, Range(0f, 180f)] private float _maxSpreadAngle = 90f;
        [SerializeField] private bool _combineSpreadWithRotation = true;
        [SerializeField] private float _randomChangeInterval = 0.5f;

        [Header("Group Animation Settings")]
        [SerializeField] private bool _useGroupAnimation = false;

        [Header("References")]
        [SerializeField] private LaserBeam _targetLaserBeam;
        [SerializeField] private FanLaserController _targetFanLaser;

        private bool _isPlaying = false;
        private float _spreadTimer = 0f;
        private float _randomTimer = 0f;
        private float _currentRandomTarget = 0f;
        private float _lastRandomValue = 0f;
        private Vector3 _originalDirection;
        private Vector3 _originalFanDirection;
        private float _originalSpreadAngle;
        private Quaternion _originalRotation;
        private Quaternion _originalFanRotation;

        private void Start()
        {
            if (_playOnStart)
            {
                Play();
            }

            StoreOriginalDirections();
        }

        private void StoreOriginalDirections()
        {
            if (_targetLaserBeam != null)
            {
                _originalDirection = _targetLaserBeam.transform.forward;
                _originalRotation = _targetLaserBeam.transform.rotation;
            }

            if (_targetFanLaser != null)
            {
                _originalFanDirection = _targetFanLaser.transform.forward;
                _originalFanRotation = _targetFanLaser.transform.rotation;
                _originalSpreadAngle = _targetFanLaser.GetSpreadAngle();
            }
        }

        private void Update()
        {
            if (!_isPlaying) return;

            // 主要なアニメーション処理を実行
            switch (_animationType)
            {
                case AnimationType.TriAxisRotate:
                    AnimateTriAxisRotation();
                    break;
                case AnimationType.IndividualBeamRotate:
                    AnimateIndividualBeams();
                    break;
            }

            // スプレッド角度のアニメーションを組み合わせる（どちらのアニメーションタイプでも共通）
            if (_combineSpreadWithRotation && _targetFanLaser != null)
            {
                AnimateSpreadAngle();
            }
        }

        private void AnimateSpreadAngle()
        {
            if (_targetFanLaser == null || _spreadAnimType == SpreadAnimationType.None) return;

            _spreadTimer += Time.deltaTime * _spreadAnimSpeed;
            float t = CalculateSpreadParameter();

            // 角度の計算と適用
            float targetAngle = Mathf.Lerp(_minSpreadAngle, _maxSpreadAngle, t);
            _targetFanLaser.SetSpreadAngle(targetAngle);
        }

        private float CalculateSpreadParameter()
        {
            switch (_spreadAnimType)
            {
                case SpreadAnimationType.Sine:
                    // サイン波の変化（滑らかに広がったり狭まったり）
                    return (Mathf.Sin(_spreadTimer * Mathf.PI * 2) + 1f) * 0.5f;

                case SpreadAnimationType.PingPong:
                    // ピンポン（等速で広がって等速で戻る）
                    return Mathf.PingPong(_spreadTimer, 1f);

                case SpreadAnimationType.Random:
                    // ランダムな変化
                    _randomTimer += Time.deltaTime;
                    if (_randomTimer >= _randomChangeInterval)
                    {
                        _lastRandomValue = _currentRandomTarget;
                        _currentRandomTarget = Random.value;
                        _randomTimer = 0;
                    }

                    // 前の値と次の値の間をスムーズに補間
                    return Mathf.Lerp(_lastRandomValue, _currentRandomTarget, _randomTimer / _randomChangeInterval);

                default:
                    return 0;
            }
        }

        private void AnimateTriAxisRotation()
        {
            // 回転角度の計算
            Vector3 angles = CalculateTriAxisAngles();

            // 各軸の回転を組み合わせたQuaternionを作成
            Quaternion combinedRotation = CreateRotationFromAngles(angles);

            ApplyRotationToTargets(combinedRotation);
        }

        private Vector3 CalculateTriAxisAngles()
        {
            float timeFactor = Time.time * _rotationSpeed;
            float xAngle, yAngle, zAngle;

            if (_useAngleLimits)
            {
                xAngle = _animateXAxis ? ModulateAngleWithinLimits(timeFactor * 0.9f, _minXRotationAngle, _maxXRotationAngle) * _xAxisInfluence : 0f;
                yAngle = _animateYAxis ? ModulateAngleWithinLimits(timeFactor * 1.1f, _minYRotationAngle, _maxYRotationAngle) * _yAxisInfluence : 0f;
                zAngle = _animateZAxis ? ModulateAngleWithinLimits(timeFactor * 1.3f, _minZRotationAngle, _maxZRotationAngle) * _zAxisInfluence : 0f;
            }
            else
            {
                xAngle = _animateXAxis ? (timeFactor * _xAxisInfluence * 10f) % 360f : 0f;
                yAngle = _animateYAxis ? (timeFactor * _yAxisInfluence * 15f) % 360f : 0f;
                zAngle = _animateZAxis ? (timeFactor * _zAxisInfluence * 20f) % 360f : 0f;
            }

            if (_xAxisRotationDirection == RotationDirection.CounterClockwise) xAngle = -xAngle;
            if (_yAxisRotationDirection == RotationDirection.CounterClockwise) yAngle = -yAngle;
            if (_zAxisRotationDirection == RotationDirection.CounterClockwise) zAngle = -zAngle;

            return new Vector3(xAngle, yAngle, zAngle);
        }

        private float ModulateAngleWithinLimits(float time, float minAngle, float maxAngle)
        {
            // Use Sin wave to oscillate between min and max angles
            return Mathf.Lerp(minAngle, maxAngle, (Mathf.Sin(time) + 1f) * 0.5f);
        }

        private Quaternion CreateRotationFromAngles(Vector3 angles)
        {
            Quaternion xRotation = Quaternion.AngleAxis(angles.x, Vector3.right);
            Quaternion yRotation = Quaternion.AngleAxis(angles.y, Vector3.up);
            Quaternion zRotation = Quaternion.AngleAxis(angles.z, Vector3.forward);

            return xRotation * zRotation * yRotation;
        }

        private void ApplyRotationToTargets(Quaternion rotation)
        {
            // 通常のレーザービームに適用
            if (_targetLaserBeam != null)
            {
                _targetLaserBeam.transform.rotation = rotation * _originalRotation;
            }

            // ファンレーザーに適用
            if (_targetFanLaser != null)
            {
                if (_useGroupAnimation && _targetFanLaser.GetAnimationMode() != FanLaserController.GroupAnimationMode.Uniform)
                {
                    ApplyGroupRotation(rotation);
                }
                else
                {
                    // Uniformモード: FanLaserControllerのルートの回転を設定
                    _targetFanLaser.transform.rotation = rotation * _originalFanRotation;
                    // Uniformモードでも、各ビームの初期回転に対する相対回転を適用したい場合は、
                    // FanLaserControllerに新しいメソッド（例: AnimateAllBeamsUniform）を追加して呼び出す
                    // _targetFanLaser.AnimateAllBeamsUniform(rotation);
                }
            }
        }

        private void ApplyGroupRotation(Quaternion rotation)
        {
            // Get base rotation relative to initial fan rotation
            Quaternion relativeGroupRotation = rotation * _originalFanRotation;

            switch (_targetFanLaser.GetAnimationMode())
            {
                case FanLaserController.GroupAnimationMode.Alternating:
                    // Calculate inverted rotation based on the main rotation
                    Quaternion invertedRotation = Quaternion.Inverse(rotation) * _originalFanRotation; // Invert relative to original
                    // Apply rotations to groups
                    _targetFanLaser.AnimateLeftGroup(relativeGroupRotation);
                    _targetFanLaser.AnimateRightGroup(invertedRotation);

                    // Center beam rotation (needs a Quaternion, let's use a partial Y rotation for now)
                    if (_targetFanLaser.GetCenterBeam() != null)
                    {
                        // Apply half Y rotation relative to original fan rotation
                        Quaternion centerRotation = Quaternion.AngleAxis(rotation.eulerAngles.y * 0.5f, Vector3.up) * _originalFanRotation;
                        _targetFanLaser.AnimateCenterBeam(centerRotation);
                    }
                    break;

                case FanLaserController.GroupAnimationMode.MirrorSync:
                    // Apply the same rotation to all beams relative to their initial rotations
                    // Option 1: Set root transform (might be simpler if FanLaserController doesn't adjust beams internally)
                    _targetFanLaser.transform.rotation = rotation * _originalFanRotation;
                    // Option 2: Call a method to rotate all beams relative to initial (requires AnimateAllBeamsUniform)
                    // _targetFanLaser.AnimateAllBeamsUniform(relativeGroupRotation);
                    break;

                case FanLaserController.GroupAnimationMode.MirrorOppose:
                    ApplyMirrorOpposeRotation(rotation); // Pass the main rotation
                    break;
            }
        }

        private void ApplyMirrorOpposeRotation(Quaternion rotation) // Accepts Quaternion
        {
            // Calculate rotations relative to original fan rotation
            Quaternion baseRotation = rotation * _originalFanRotation;

            // Left group (normal rotation)
            Quaternion leftRotation = baseRotation;

            // Right group (half rotation speed - applied by adjusting the angle)
            Vector3 angles = rotation.eulerAngles;
            Quaternion rightRotation = Quaternion.Euler(angles.x * 0.5f, angles.y * 0.5f, angles.z * 0.5f) * _originalFanRotation;

            _targetFanLaser.AnimateLeftGroup(leftRotation);
            _targetFanLaser.AnimateRightGroup(rightRotation);

            // Center beam (normal rotation)
            if (_targetFanLaser.GetCenterBeam() != null)
            {
                _targetFanLaser.AnimateCenterBeam(baseRotation);
            }
        }

        private void AnimateIndividualBeams()
        {
            if (_targetFanLaser == null) return;

            List<LaserBeam> beams = _targetFanLaser.GetLaserBeams();
            if (beams == null || beams.Count == 0) return;

            var rotationData = CalculateIndividualBeamRotations(beams);

            ApplyIndividualBeamRotations(beams, rotationData);
        }

        private (List<float> xRotations, List<float> yRotations, List<float> zRotations) CalculateIndividualBeamRotations(List<LaserBeam> beams)
        {
            List<float> xRotations = new();
            List<float> yRotations = new();
            List<float> zRotations = new();

            for (int i = 0; i < beams.Count; i++)
            {
                float phaseOffset = _randomizeBeamRotations
                    ? Mathf.PerlinNoise(i * 0.3f, 0.5f) * _beamPhaseVariance
                    : i * (1f / beams.Count) * _beamPhaseVariance;

                float time = Time.time * _rotationSpeed + phaseOffset;

                float xAngle, yAngle, zAngle;

                if (_useAngleLimits)
                {
                    xAngle = CalculateAxisRotationWithLimits(time * 0.9f, _minXRotationAngle, _maxXRotationAngle, _xAxisInfluence);
                    yAngle = CalculateAxisRotationWithLimits(time * 1.1f, _minYRotationAngle, _maxYRotationAngle, _yAxisInfluence);
                    zAngle = CalculateAxisRotationWithLimits(time * 1.3f, _minZRotationAngle, _maxZRotationAngle, _zAxisInfluence);
                }
                else
                {
                    xAngle = _xAxisInfluence > 0 ? Mathf.Sin(time * 0.9f) * _maxBeamRotationAngle * _xAxisInfluence : 0f;
                    yAngle = _yAxisInfluence > 0 ? Mathf.Sin(time * 1.1f) * _maxBeamRotationAngle * _yAxisInfluence : 0f;
                    zAngle = _zAxisInfluence > 0 ? Mathf.Sin(time * 1.3f) * _maxBeamRotationAngle * _zAxisInfluence : 0f;
                }

                xRotations.Add(xAngle);
                yRotations.Add(yAngle);
                zRotations.Add(zAngle);
            }

            return (xRotations, yRotations, zRotations);
        }

        private float CalculateAxisRotationWithLimits(float time, float minAngle, float maxAngle, float influence)
        {
            if (influence <= 0) return 0f;

            return Mathf.Lerp(minAngle, maxAngle, (Mathf.Sin(time) + 1f) * 0.5f) * influence;
        }

        private void ApplyIndividualBeamRotations(List<LaserBeam> beams, (List<float> xRotations, List<float> yRotations, List<float> zRotations) rotationData)
        {
            if (_useGroupAnimation && _targetFanLaser.GetAnimationMode() != FanLaserController.GroupAnimationMode.Uniform)
            {
                switch (_targetFanLaser.GetAnimationMode())
                {
                    case FanLaserController.GroupAnimationMode.Alternating:
                        ApplyAlternatingGroupRotations(rotationData); // Calls RotateLeft/RightGroupBeams
                        break;
                    case FanLaserController.GroupAnimationMode.MirrorSync:
                        _targetFanLaser.ApplyIndividualAxisRotations(rotationData.xRotations, rotationData.yRotations, rotationData.zRotations);
                        break;
                    case FanLaserController.GroupAnimationMode.MirrorOppose:
                        ApplyMirrorOpposeIndividualRotations(beams); // Calls ApplyIndividualAxisRotations
                        break;
                }
            }
            else
            {
                _targetFanLaser.ApplyIndividualAxisRotations(rotationData.xRotations, rotationData.yRotations, rotationData.zRotations);
            }
        }

        private void ApplyAlternatingGroupRotations((List<float> xRotations, List<float> yRotations, List<float> zRotations) rotationData)
        {
            // 左右で異なるパターンの回転
            List<float> leftXRotations = new();
            List<float> leftYRotations = new();
            List<float> leftZRotations = new();

            List<float> rightXRotations = new();
            List<float> rightYRotations = new();
            List<float> rightZRotations = new();

            // 左ビームグループの数だけ抽出
            int leftCount = _targetFanLaser.GetLeftBeams().Count;
            for (int i = 0; i < leftCount && i < rotationData.xRotations.Count; i++)
            {
                leftXRotations.Add(rotationData.xRotations[i]);
                leftYRotations.Add(rotationData.yRotations[i]);
                leftZRotations.Add(rotationData.zRotations[i]);
            }

            // 右ビームグループは逆の回転
            int rightCount = _targetFanLaser.GetRightBeams().Count;
            for (int i = 0; i < rightCount && (leftCount + i) < rotationData.xRotations.Count; i++)
            {
                // 右側は左側の逆相
                rightXRotations.Add(-rotationData.xRotations[leftCount + i]);
                rightYRotations.Add(-rotationData.yRotations[leftCount + i]);
                rightZRotations.Add(-rotationData.zRotations[leftCount + i]);
            }

            // 左右のグループに別々に適用
            _targetFanLaser.RotateLeftGroupBeams(leftXRotations, leftYRotations, leftZRotations);
            _targetFanLaser.RotateRightGroupBeams(rightXRotations, rightYRotations, rightZRotations);

            // 中央ビームは異なる回転を適用
            LaserBeam centerBeam = _targetFanLaser.GetCenterBeam();
            if (centerBeam != null)
            {
                float time = Time.time * _rotationSpeed;
                float centerXAngle = Mathf.Cos(time) * _maxBeamRotationAngle * 0.5f * _xAxisInfluence;
                float centerYAngle = Mathf.Sin(time * 1.5f) * _maxBeamRotationAngle * 0.5f * _yAxisInfluence;
                float centerZAngle = 0f; // Z軸は固定

                int centerIndex = _targetFanLaser.GetLaserBeams().IndexOf(centerBeam);
                if (centerIndex >= 0)
                {
                    _targetFanLaser.RotateIndividualBeam(centerIndex, centerXAngle, centerYAngle, centerZAngle);
                }
            }
        }

        private void ApplyMirrorOpposeIndividualRotations(List<LaserBeam> beams)
        {
            // 左右で位相をずらした回転
            List<float> phaseShiftedXRotations = new List<float>();
            List<float> phaseShiftedYRotations = new List<float>();
            List<float> phaseShiftedZRotations = new List<float>();

            // 中央からの距離に応じて位相をずらす
            int midPoint = beams.Count / 2;
            for (int i = 0; i < beams.Count; i++)
            {
                float distanceFromCenter = Mathf.Abs(i - midPoint) / (float)midPoint;
                float phaseShift = distanceFromCenter * Mathf.PI; // 最大180度の位相ずれ

                float time = Time.time * _rotationSpeed + phaseShift;

                float xAngle = Mathf.Sin(time * 0.9f) * _maxBeamRotationAngle * _xAxisInfluence;
                float yAngle = Mathf.Sin(time * 1.1f) * _maxBeamRotationAngle * _yAxisInfluence;
                float zAngle = Mathf.Sin(time * 1.3f) * _maxBeamRotationAngle * _zAxisInfluence;

                phaseShiftedXRotations.Add(xAngle);
                phaseShiftedYRotations.Add(yAngle);
                phaseShiftedZRotations.Add(zAngle);
            }

            _targetFanLaser.ApplyIndividualAxisRotations(
                phaseShiftedXRotations,
                phaseShiftedYRotations,
                phaseShiftedZRotations);
        }

        #region Public API

        public void Play()
        {
            _isPlaying = true;
            _spreadTimer = 0f;
            _randomTimer = 0f;
            _currentRandomTarget = Random.value;
            _lastRandomValue = _currentRandomTarget;

            // 元の設定を記憶
            if (_targetFanLaser != null)
            {
                _originalSpreadAngle = _targetFanLaser.GetSpreadAngle();
            }
        }

        public void Stop()
        {
            _isPlaying = false;

            // 元の設定に戻す
            if (_targetFanLaser != null)
            {
                _targetFanLaser.SetSpreadAngle(_originalSpreadAngle);
            }
        }

        #endregion
    }
}
