using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace BeatSaberClone.UseCase
{
    public interface ISceneLoaderUseCase : IDisposable
    {
        UniTask LoadSceneAsync(string sceneName, CancellationToken ct);
    }
}
