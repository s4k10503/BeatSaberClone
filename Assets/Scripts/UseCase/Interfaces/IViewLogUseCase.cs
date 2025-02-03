using System;

namespace BeatSaberClone.UseCase
{
    public interface IViewLogUseCase : IDisposable
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);

    }
}
