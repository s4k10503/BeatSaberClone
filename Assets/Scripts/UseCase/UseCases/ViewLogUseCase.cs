using BeatSaberClone.Domain;
using Zenject;

namespace BeatSaberClone.UseCase
{
    public sealed class ViewLogUseCase : IViewLogUseCase
    {
        private readonly ILoggerService _logger;

        [Inject]
        public ViewLogUseCase(ILoggerService logger)
        {
            _logger = logger;
        }

        public void Log(string message)
            => _logger.Log(message);

        public void LogWarning(string message)
            => _logger.LogWarning(message);

        public void LogError(string message)
            => _logger.LogError(message);

        public void Dispose()
        {
            _logger.Dispose();
        }
    }
}