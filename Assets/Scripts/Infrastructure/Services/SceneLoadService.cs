using BeatSaberClone.Domain;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace BeatSaberClone.Infrastructure
{
    public class SceneLoadeService : ISceneLoadService
    {
        public async UniTask LoadSceneAsync(string sceneName, CancellationToken ct)
        {
            var loadOperation = SceneManager.LoadSceneAsync(sceneName);
            loadOperation.allowSceneActivation = false;

            while (loadOperation.progress < 0.9f)
            {
                await UniTask.Yield(ct);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.1), cancellationToken: ct);
            loadOperation.allowSceneActivation = true;
            await loadOperation.WithCancellation(ct);
        }

        public void Dispose()
        {

        }
    }
}
