using BeatSaberClone.Domain;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace BeatSaberClone.Infrastructure
{
    public sealed class SceneLoadeService : ISceneLoadService
    {
        public async UniTask LoadSceneAsync(string sceneName, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                throw new InfrastructureException("The scene name is not specified.");
            }

            try
            {
                var loadOperation = SceneManager.LoadSceneAsync(sceneName)
                    ?? throw new InfrastructureException($"The reading operation of the scene {sceneName} could not be obtained.");
                loadOperation.allowSceneActivation = false;

                // Wait until the reading progress reaches 0.9F
                while (loadOperation.progress < 0.9f)
                {
                    await UniTask.Yield(ct);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(0.1), cancellationToken: ct);
                loadOperation.allowSceneActivation = true;
                await loadOperation.WithCancellation(ct);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"An error occurred while reading the scene {sceneName}.", ex);
            }
        }

        public void Dispose()
        {
        }
    }
}
