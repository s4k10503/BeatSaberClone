using BeatSaberClone.UseCase;
using UnityEngine;

namespace BeatSaberClone.Infrastructure
{
    public class DistanceCalculator : IDistanceCalculator
    {
        public float CalculateDistance(Transform spawnPoint, Transform playerPoint)
        {
            if (spawnPoint == null || playerPoint == null) return 0f;
            return Mathf.Abs(spawnPoint.position.z - playerPoint.position.z);
        }

        public void Dispose()
        {

        }
    }
}
