using System;
using UnityEngine;

namespace BeatSaberClone.UseCase
{
    public interface ISwingDetector : IDisposable
    {
        bool IsSwinging { get; }
        void UpdateSwing(Vector3 velocity);
    }
}
