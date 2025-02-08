using System;
using UnityEngine;

namespace BeatSaberClone.Presentation
{
    public interface ISliceDetector : IDisposable
    {
        bool CheckForSlice(out GameObject slicedObject);
    }
}
