using BeatSaberClone.Domain;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

namespace BeatSaberClone.UseCase
{
    public sealed class SceneLoaderUseCase : ISceneLoaderUseCase
    {
        private readonly ISceneLoadService _sceneLoadService;

        [Inject]
        public SceneLoaderUseCase(
            ISceneLoadService sceneLoadServie)
        {
            _sceneLoadService = sceneLoadServie;
        }

        public async UniTask LoadSceneAsync(string sceneName, CancellationToken ct)
        {
            await _sceneLoadService.LoadSceneAsync(sceneName, ct);
        }

        public void Dispose()
        {
            _sceneLoadService.Dispose();
        }
    }
}
