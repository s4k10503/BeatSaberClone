using BeatSaberClone.Domain;
using UnityEngine;

namespace BeatSaberClone.Infrastructure
{
    public sealed class DistanceCalculator : IDistanceCalculator
    {
        public float CalculateDistance(Transform spawnPoint, Transform playerPoint)
        {
            if (spawnPoint == null)
            {
                throw new InfrastructureException("Spawnpoint is null.");
            }
            if (playerPoint == null)
            {
                throw new InfrastructureException("Playerpoint is null.");
            }
            return Mathf.Abs(spawnPoint.position.z - playerPoint.position.z);
        }

        public void Dispose()
        {
        }
    }
}
