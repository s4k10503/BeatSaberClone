using System;
using UnityEngine;

namespace BeatSaberClone.UseCase
{
    public interface ISliceDetector : IDisposable
    {
        bool CheckForSlice(out GameObject slicedObject);
    }
}
