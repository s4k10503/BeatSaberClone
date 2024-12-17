using System;

namespace BeatSaberClone.Domain
{
    public interface ILoggerService : IDisposable
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}
