using System.Collections.Generic;
using System;
using UnityEngine;

namespace BeatSaberClone.Presentation
{
    /// <summary>
    /// Controls a fan of laser beams spreading from a single point in a specified angle range
    /// </summary>
    public class FanLaserController : MonoBehaviour
    {
        public enum GroupAnimationMode
        {
            Uniform,    // All beams are the same animation
            Alternating, // Alternate left and right animation
            MirrorSync,  // Synchronized symmetrically
            MirrorOppose // Symmetrically inverse phase
        }

        public enum BeamGroup
        {
            All,
            Left,
            Right,
            Center
        }

        public List<LaserBeam> GetLaserBeams() => _laserBeams;
        public List<LaserBeam> GetLeftBeams() => _leftBeams;
        public List<LaserBeam> GetRightBeams() => _rightBeams;
        public LaserBeam GetCenterBeam() => _centerBeam;
        public float GetSpreadAngle() => _spreadAngle;
        public GroupAnimationMode GetAnimationMode() => _animationMode;
        public void SetAnimationMode(GroupAnimationMode mode) => _animationMode = mode;


        [Header("Fan Laser Settings")]
        [SerializeField] private int _beamCount = 5;
        [SerializeField] private float _spreadAngle = 45f;
        [SerializeField] private bool _enabled = false;
        [SerializeField] private GroupAnimationMode _animationMode = GroupAnimationMode.Uniform;

        [Header("Laser Beam Settings")]
        [SerializeField] private float _width = 0.1f;
        [SerializeField] private float _maxLength = 100.0f;
        [SerializeField] private Vector3 _centerDirection = Vector3.forward;
        [SerializeField] private Material _laserMaterial;

        // List of all laser beams
        private List<LaserBeam> _laserBeams = new();
        private List<LaserBeam> _leftBeams = new();
        private List<LaserBeam> _rightBeams = new();
        private LaserBeam _centerBeam;
        private List<Quaternion> _initialBeamRotations = new(); // Store initial rotations

        private void Start()
        {
            CreateLaserBeams();
        }

        private void OnDestroy()
        {
            ClearLaserBeams();
        }

        private void CreateLaserBeams()
        {
            ClearLaserBeams();

            if (_beamCount <= 0) return;

            // Calculate the center index
            int centerIndex = _beamCount / 2;
            bool hasCenterBeam = _beamCount % 2 == 1;

            // Create as many beams as specified
            for (int i = 0; i < _beamCount; i++)
            {
                // Create a beam object with appropriate name
                string beamName;
                if (hasCenterBeam && i == centerIndex)
                {
                    beamName = "CenterLaserBeam";
                }
                else if (i < centerIndex)
                {
                    beamName = $"LeftLaserBeam_{i}";
                }
                else
                {
                    beamName = $"RightLaserBeam_{i - (hasCenterBeam ? centerIndex + 1 : centerIndex)}";
                }

                GameObject beamObj = new(beamName);
                beamObj.transform.SetParent(transform);
                beamObj.transform.localPosition = Vector3.zero;

                // Added LaserBeam Component
                LaserBeam laserBeam = beamObj.AddComponent<LaserBeam>();

                // Get and set LineRenderer
                LineRenderer lineRenderer = beamObj.GetComponent<LineRenderer>();

                // LineRenderer basic settings
                SetupLineRenderer(lineRenderer);

                // Calculate the initial direction
                Vector3 direction = CalculateBeamDirection(i);

                // Configuring LaserBeam
                laserBeam.SetWidth(_width);
                laserBeam.SetMaxLength(_maxLength);
                laserBeam.SetEnabled(_enabled);
                // Set initial transform based on calculated direction
                laserBeam.transform.rotation = Quaternion.LookRotation(direction, CalculatePerpendicularAxis(direction));
                laserBeam.SetDirection(direction); // Still set direction for LaserBeam internal logic if needed

                // Add to list
                _laserBeams.Add(laserBeam);
                _initialBeamRotations.Add(laserBeam.transform.rotation); // Store initial rotation

                // Divide into left and right groups
                if (hasCenterBeam && i == centerIndex)
                {
                    _centerBeam = laserBeam;
                }
                else if (i < centerIndex)
                {
                    _leftBeams.Add(laserBeam);
                }
                else
                {
                    _rightBeams.Add(laserBeam);
                }
            }
        }

        private void SetupLineRenderer(LineRenderer lineRenderer)
        {
            if (lineRenderer == null) return;

            // Basic settings
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = _width;
            lineRenderer.endWidth = _width;
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.forward * _maxLength);

            // Apply material
            if (_laserMaterial != null)
            {
                lineRenderer.material = _laserMaterial;
            }
            else
            {
                Debug.LogError("FanLaserController: No laser material assigned!");
            }
        }

        private void ClearLaserBeams()
        {
            // Delete all existing beams
            foreach (var beam in _laserBeams)
            {
                if (beam != null && beam.gameObject != null)
                {
                    Destroy(beam.gameObject);
                }
            }

            _laserBeams.Clear();
            _leftBeams.Clear();
            _rightBeams.Clear();
            _centerBeam = null;
            _initialBeamRotations.Clear(); // Clear initial rotations
        }

        /// <summary>
        /// 扇状の分布に基づいてビームの方向を計算する
        /// </summary>
        private Vector3 CalculateBeamDirection(int beamIndex)
        {
            if (beamIndex < 0 || beamIndex >= _beamCount)
                return _centerDirection.normalized;

            // 単一ビームの場合は中心方向を返す
            if (_beamCount == 1)
                return _centerDirection.normalized;

            // 角度ステップと回転軸の計算
            float angleStep = _spreadAngle / (_beamCount - 1);
            Vector3 center = _centerDirection.normalized;
            Vector3 perpendicular = CalculatePerpendicularAxis(center);

            // 開始角度（中心から左に半分の広がり）
            float startAngle = -_spreadAngle / 2f;

            // このビームの角度
            float angle = startAngle + (beamIndex * angleStep);

            // 中心をこの角度で回転
            Quaternion rotation = Quaternion.AngleAxis(angle, perpendicular);
            return rotation * center;
        }

        /// <summary>
        /// 中心方向に垂直な軸を計算
        /// </summary>
        private Vector3 CalculatePerpendicularAxis(Vector3 direction)
        {
            Vector3 normDirection = direction.normalized;
            Vector3 up = Vector3.up;

            // 中心方向が上または下に近い場合、代わりに右方向を使用
            if (Vector3.Dot(normDirection, up) > 0.99f || Vector3.Dot(normDirection, up) < -0.99f)
            {
                up = Vector3.right;
            }

            return Vector3.Cross(normDirection, up).normalized;
        }

        private void UpdateBeamDirections()
        {
            if (_beamCount <= 0 || _laserBeams.Count == 0 || _laserBeams.Count != _initialBeamRotations.Count) return;

            // 各ビームの方向と初期回転を再計算・更新
            for (int i = 0; i < _laserBeams.Count && i < _beamCount; i++)
            {
                if (_laserBeams[i] != null)
                {
                    Vector3 direction = CalculateBeamDirection(i);
                    Quaternion initialRotation = Quaternion.LookRotation(direction, CalculatePerpendicularAxis(direction));
                    _laserBeams[i].transform.rotation = initialRotation; // Reset to calculated initial rotation
                    _laserBeams[i].SetDirection(direction); // Update direction if LaserBeam uses it
                    _initialBeamRotations[i] = initialRotation; // Update stored initial rotation
                }
            }
        }

        /// <summary>
        /// 指定したグループのビームのリストを取得
        /// </summary>
        private List<LaserBeam> GetBeamGroup(BeamGroup group)
        {
            switch (group)
            {
                case BeamGroup.All:
                    return _laserBeams;
                case BeamGroup.Left:
                    return _leftBeams;
                case BeamGroup.Right:
                    return _rightBeams;
                case BeamGroup.Center:
                    return _centerBeam != null ? new List<LaserBeam> { _centerBeam } : new List<LaserBeam>();
                default:
                    return new List<LaserBeam>();
            }
        }

        /// <summary>
        /// 指定したグループのビームに共通の設定を適用
        /// </summary>
        private void ApplyToGroup<T>(BeamGroup group, Action<LaserBeam, T> action, T value)
        {
            foreach (var beam in GetBeamGroup(group))
            {
                if (beam != null)
                {
                    action(beam, value);
                }
            }
        }

        /// <summary>
        /// ビームグループの回転を設定
        /// </summary>
        private void SetGroupRotation(BeamGroup group, Quaternion groupRotation)
        {
            List<LaserBeam> beams = GetBeamGroup(group);
            foreach (var beam in beams)
            {
                if (beam != null)
                {
                    int beamIndex = _laserBeams.IndexOf(beam);
                    if (beamIndex >= 0 && beamIndex < _initialBeamRotations.Count)
                    {
                        // Apply group rotation relative to the beam's initial rotation
                        beam.transform.rotation = groupRotation * _initialBeamRotations[beamIndex];
                    }
                }
            }
        }

        #region Public API

        /// <summary>
        /// Sets the enabled state of all laser beams in the fan
        /// </summary>
        public void SetEnabled(bool isEnabled)
        {
            _enabled = isEnabled;
            ApplyToGroup(BeamGroup.All, (beam, enabled) => beam.SetEnabled(enabled), isEnabled);
        }

        /// <summary>
        /// Sets the width of all laser beams in the fan
        /// </summary>
        public void SetWidth(float newWidth)
        {
            _width = Mathf.Max(0.01f, newWidth);
            ApplyToGroup(BeamGroup.All, (beam, width) => beam.SetWidth(width), _width);
        }

        /// <summary>
        /// Sets the maximum length of all laser beams in the fan
        /// </summary>
        public void SetMaxLength(float newMaxLength)
        {
            _maxLength = Mathf.Max(0.1f, newMaxLength);
            ApplyToGroup(BeamGroup.All, (beam, length) => beam.SetMaxLength(length), _maxLength);
        }

        /// <summary>
        /// Sets the number of beams in the fan and recreates the pattern
        /// </summary>
        public void SetBeamCount(int count)
        {
            _beamCount = Mathf.Max(1, count);
            CreateLaserBeams();
        }

        /// <summary>
        /// Sets the spread angle of the fan and updates the pattern
        /// </summary>
        public void SetSpreadAngle(float angle)
        {
            _spreadAngle = Mathf.Clamp(angle, 0.1f, 360f);
            UpdateBeamDirections();
        }

        // アニメーション関連のメソッド
        public void AnimateLeftGroup(Quaternion rotation) => SetGroupRotation(BeamGroup.Left, rotation);
        public void AnimateRightGroup(Quaternion rotation) => SetGroupRotation(BeamGroup.Right, rotation);
        public void AnimateCenterBeam(Quaternion rotation) => SetGroupRotation(BeamGroup.Center, rotation);
        // Optional: Method to animate all beams uniformly with a group rotation
        public void AnimateAllBeamsUniform(Quaternion rotation) => SetGroupRotation(BeamGroup.All, rotation);

        /// <summary>
        /// 各ビームに個別の回転を適用
        /// </summary>
        /// <param name="rotations">各ビームに適用する回転のリスト。リストの長さがビーム数と一致する必要があります。</param>
        public void ApplyIndividualRotations(List<Quaternion> rotations)
        {
            if (rotations == null || rotations.Count == 0 || _laserBeams.Count == 0 || _laserBeams.Count != _initialBeamRotations.Count) return;

            // 各ビームに回転を適用
            for (int i = 0; i < _laserBeams.Count && i < rotations.Count; i++)
            {
                if (_laserBeams[i] != null)
                {
                    // Apply individual rotation relative to the beam's initial rotation
                    _laserBeams[i].transform.rotation = rotations[i] * _initialBeamRotations[i];
                    // Optional: Update LaserBeam's internal direction if needed
                    // _laserBeams[i].SetDirection(_laserBeams[i].transform.forward);
                }
            }
        }

        /// <summary>
        /// 各ビームに個別のXYZ回転を適用
        /// </summary>
        /// <param name="xRotations">X軸の回転角度（度）のリスト</param>
        /// <param name="yRotations">Y軸の回転角度（度）のリスト</param>
        /// <param name="zRotations">Z軸の回転角度（度）のリスト</param>
        public void ApplyIndividualAxisRotations(List<float> xRotations, List<float> yRotations, List<float> zRotations)
        {
            if (_laserBeams.Count == 0) return;
            if (xRotations == null) xRotations = new List<float>(new float[_laserBeams.Count]);
            if (yRotations == null) yRotations = new List<float>(new float[_laserBeams.Count]);
            if (zRotations == null) zRotations = new List<float>(new float[_laserBeams.Count]);

            // 各ビームの回転を計算して適用
            List<Quaternion> rotations = new List<Quaternion>();

            for (int i = 0; i < _laserBeams.Count; i++)
            {
                float xAngle = i < xRotations.Count ? xRotations[i] : 0f;
                float yAngle = i < yRotations.Count ? yRotations[i] : 0f;
                float zAngle = i < zRotations.Count ? zRotations[i] : 0f;

                Quaternion rotation = Quaternion.Euler(xAngle, yAngle, zAngle);
                rotations.Add(rotation);
            }

            ApplyIndividualRotations(rotations);
        }

        /// <summary>
        /// 特定のビームに対してオフセット回転を適用
        /// </summary>
        /// <param name="beamIndex">対象ビームのインデックス</param>
        /// <param name="xAngle">X軸回転角度（度）</param>
        /// <param name="yAngle">Y軸回転角度（度）</param>
        /// <param name="zAngle">Z軸回転角度（度）</param>
        public void RotateIndividualBeam(int beamIndex, float xAngle, float yAngle, float zAngle)
        {
            if (beamIndex < 0 || beamIndex >= _laserBeams.Count || beamIndex >= _initialBeamRotations.Count) return;

            LaserBeam beam = _laserBeams[beamIndex];
            if (beam == null) return;

            // Calculate the individual rotation
            Quaternion individualRotation = Quaternion.Euler(xAngle, yAngle, zAngle);

            // Apply individual rotation relative to the beam's initial rotation
            beam.transform.rotation = individualRotation * _initialBeamRotations[beamIndex];

            // Optional: Update LaserBeam's internal direction if needed
            // beam.SetDirection(beam.transform.forward);
        }

        /// <summary>
        /// 指定したグループのビームに個別のXYZ回転を適用
        /// </summary>
        private void RotateGroupBeams(BeamGroup group, List<float> xAngles, List<float> yAngles, List<float> zAngles)
        {
            List<LaserBeam> beams = GetBeamGroup(group);
            if (beams.Count == 0) return;

            for (int i = 0; i < beams.Count; i++)
            {
                if (beams[i] == null) continue;

                int beamIndex = _laserBeams.IndexOf(beams[i]);
                if (beamIndex < 0) continue;

                float xAngle = i < xAngles.Count ? xAngles[i] : 0f;
                float yAngle = i < yAngles.Count ? yAngles[i] : 0f;
                float zAngle = i < zAngles.Count ? zAngles[i] : 0f;

                RotateIndividualBeam(beamIndex, xAngle, yAngle, zAngle);
            }
        }

        /// <summary>
        /// 左グループのビームに個別のXYZ回転を適用
        /// </summary>
        public void RotateLeftGroupBeams(List<float> xAngles, List<float> yAngles, List<float> zAngles)
        {
            RotateGroupBeams(BeamGroup.Left, xAngles, yAngles, zAngles);
        }

        /// <summary>
        /// 右グループのビームに個別のXYZ回転を適用
        /// </summary>
        public void RotateRightGroupBeams(List<float> xAngles, List<float> yAngles, List<float> zAngles)
        {
            RotateGroupBeams(BeamGroup.Right, xAngles, yAngles, zAngles);
        }

        /// <summary>
        /// Sets the center direction of the fan (DEPRECATED: Use transform.rotation instead)
        /// </summary>
        [Obsolete("SetCenterDirection is deprecated. Manipulate transform.rotation directly or use specific animation methods.")]
        public void SetCenterDirection(Vector3 newDirection)
        {
            Debug.LogWarning("SetCenterDirection is deprecated. Rotation might not behave as expected.");
            if (newDirection != Vector3.zero)
            {
                _centerDirection = newDirection;
                // UpdateBeamDirections might conflict with direct rotation control.
                // Consider removing this call if manual rotation is primary.
                UpdateBeamDirections();
            }
        }

        #endregion
    }
}
