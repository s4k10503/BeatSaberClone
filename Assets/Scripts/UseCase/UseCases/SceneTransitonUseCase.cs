using BeatSaberClone.Domain;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

namespace BeatSaberClone.UseCase
{
    public class SceneLoaderUseCase : ISceneLoaderUseCase
    {
        private readonly ISceneLoadService _sceneLoadService;
        private readonly ILoggerService _logger;

        [Inject]
        public SceneLoaderUseCase(
            ISceneLoadService sceneLoadServie,
            ILoggerService logger)
        {
            _sceneLoadService = sceneLoadServie;
            _logger = logger;
        }

        public async UniTask LoadSceneAsync(string sceneName, CancellationToken ct)
        {
            await _sceneLoadService.LoadSceneAsync(sceneName, ct);
        }

        public void Dispose()
        {
            //_logger.Log("Dispose SceneLoaderUseCase");
            _logger.Dispose();
            _sceneLoadService.Dispose();
        }
    }
}
