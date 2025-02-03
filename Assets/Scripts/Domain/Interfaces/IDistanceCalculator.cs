using System;
using UnityEngine;

namespace BeatSaberClone.Domain
{
    public interface IDistanceCalculator : IDisposable
    {
        float CalculateDistance(Transform spawnPoint, Transform playerPoint);
    }
}
