using System;
using UnityEngine;

namespace BeatSaberClone.UseCase
{
    public interface IDistanceCalculator : IDisposable
    {
        float CalculateDistance(Transform spawnPoint, Transform playerPoint);
    }
}
