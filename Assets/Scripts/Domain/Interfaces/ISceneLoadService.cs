using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace BeatSaberClone.Domain
{
    public interface ISceneLoadService : IDisposable
    {
        UniTask LoadSceneAsync(string sceneName, CancellationToken ct);
    }
}
