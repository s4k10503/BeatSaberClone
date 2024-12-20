﻿/*
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
	public class VelocityEstimator : MonoBehaviour
	{
		[Tooltip("How many frames to average over for computing velocity")]
		public int velocityAverageFrames = 5;
		[Tooltip("How many frames to average over for computing angular velocity")]
		public int angularVelocityAverageFrames = 11;

		public bool estimateOnAwake = false;

		private int sampleCount;
		private Vector3[] velocitySamples;
		private Vector3[] angularVelocitySamples;
		private CancellationTokenSource cancellation;

		void Awake()
		{
			InitializeSamples();
			if (estimateOnAwake)
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
			velocitySamples = new Vector3[velocityAverageFrames];
			angularVelocitySamples = new Vector3[angularVelocityAverageFrames];
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
			Vector3 previousPosition = transform.position;
			Quaternion previousRotation = transform.rotation;

			while (!token.IsCancellationRequested)
			{
				// Wait for end of frame
				await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, token);


				sampleCount++;
				int velocityIndex = (sampleCount - 1) % velocitySamples.Length;
				int angularVelocityIndex = (sampleCount - 1) % angularVelocitySamples.Length;

				velocitySamples[velocityIndex] = (transform.position - previousPosition) / Time.deltaTime;
				angularVelocitySamples[angularVelocityIndex] = ComputeAngularVelocity(previousRotation, transform.rotation);

				previousPosition = transform.position;
				previousRotation = transform.rotation;
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
