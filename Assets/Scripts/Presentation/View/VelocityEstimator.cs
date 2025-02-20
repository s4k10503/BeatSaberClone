/*
Quoted and modified from:
https://github.com/ValveSoftware/steamvr_unity_plugin/blob/master/Assets/SteamVR/InteractionSystem/Core/Scripts/VelocityEstimator.cs

BSD-3-Clause license
Copyright (c) Valve Corporation
*/


using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace BeatSaberClone.Presentation
{
    public sealed class VelocityEstimator : MonoBehaviour
    {
        [SerializeField] private VelocityEstimatorSettings _velocityEstimatorSettings;
        private int _velocityAverageFrames;
        private int _angularVelocityAverageFrames;

        private int sampleCount;
        private Vector3[] velocitySamples;
        private Vector3[] angularVelocitySamples;
        private CancellationTokenSource cancellation;

        void Awake()
        {
            InitializeSamples();
            if (_velocityEstimatorSettings.EstimateOnAwake)
            {
                BeginEstimatingVelocity();
            }
        }

        void OnDestroy()
        {
            // Stop the task when the component is destroyed
            StopEstimatingVelocity();
        }

        public void BeginEstimatingVelocity()
        {
            // Stop existing estimation
            StopEstimatingVelocity();

            cancellation = new CancellationTokenSource();

            // Start Task
            EstimateVelocityAsync(cancellation.Token).Forget();
        }

        public void StopEstimatingVelocity()
        {
            // Cancel if task is running
            cancellation?.Cancel();

            // Discard CancellationTokenSource
            cancellation?.Dispose();

            cancellation = null;
        }

        public Vector3 GetVelocityEstimate()
        {
            return ComputeAverage(velocitySamples, sampleCount);
        }

        public Vector3 GetAngularVelocityEstimate()
        {
            return ComputeAverage(angularVelocitySamples, sampleCount);
        }

        public Vector3 GetAccelerationEstimate()
        {
            Vector3 averageAcceleration = Vector3.zero;
            int validSampleCount = Mathf.Max(0, sampleCount - 2);

            for (int i = 0; i < validSampleCount; i++)
            {
                Vector3 acceleration = (velocitySamples[(i + 1) % velocitySamples.Length] - velocitySamples[i % velocitySamples.Length]) / Time.deltaTime;
                averageAcceleration += acceleration;
            }

            if (validSampleCount > 0)
            {
                averageAcceleration /= validSampleCount;
            }

            return averageAcceleration;
        }

        private void InitializeSamples()
        {
            _velocityAverageFrames = _velocityEstimatorSettings.VelocityAverageFrames;
            _angularVelocityAverageFrames = _velocityEstimatorSettings.AngularVelocityAverageFrames;

            velocitySamples = new Vector3[_velocityAverageFrames];
            angularVelocitySamples = new Vector3[_angularVelocityAverageFrames];
        }

        private Vector3 ComputeAverage(Vector3[] samples, int count)
        {
            Vector3 sum = Vector3.zero;
            int sampleCount = Mathf.Min(count, samples.Length);

            for (int i = 0; i < sampleCount; i++)
            {
                sum += samples[i];
            }

            if (sampleCount > 0)
            {
                sum /= sampleCount;
            }

            return sum;
        }

        private async UniTaskVoid EstimateVelocityAsync(CancellationToken token)
        {
            transform.GetPositionAndRotation(out Vector3 previousPosition, out Quaternion previousRotation);
            while (!token.IsCancellationRequested)
            {
                // Wait for end of frame
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, token);

                sampleCount++;
                int velocityIndex = (sampleCount - 1) % velocitySamples.Length;
                int angularVelocityIndex = (sampleCount - 1) % angularVelocitySamples.Length;

                velocitySamples[velocityIndex] = (transform.position - previousPosition) / Time.deltaTime;
                angularVelocitySamples[angularVelocityIndex] = ComputeAngularVelocity(previousRotation, transform.rotation);

                transform.GetPositionAndRotation(out previousPosition, out previousRotation);
            }
        }

        private Vector3 ComputeAngularVelocity(Quaternion fromRotation, Quaternion toRotation)
        {
            Quaternion deltaRotation = toRotation * Quaternion.Inverse(fromRotation);
            deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 axisOfRotation);

            if (angleInDegrees > 180)
            {
                angleInDegrees -= 360;
            }

            return axisOfRotation * (angleInDegrees * Mathf.Deg2Rad / Time.deltaTime);
        }
    }
}
