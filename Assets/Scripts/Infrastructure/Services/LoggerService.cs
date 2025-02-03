using BeatSaberClone.Domain;

namespace BeatSaberClone.Infrastructure
{
    public sealed class LoggerService : ILoggerService
    {

        public void Log(string message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log(message);
#endif
        }

        public void LogWarning(string message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogWarning(message);
#endif
        }

        public void LogError(string message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogError(message);
#endif
        }

        public void Dispose()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            //UnityEngine.Debug.Log("Dispose Logger");
#endif
        }
    }
}

