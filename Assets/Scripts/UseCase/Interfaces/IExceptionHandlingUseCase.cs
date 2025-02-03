using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using BeatSaberClone.Domain;

namespace BeatSaberClone.UseCase
{
    public interface IExceptionHandlingUseCase
    {
        UniTask RetryAsync(Func<UniTask> action, int maxRetries, CancellationToken cts);
        UniTask SafeExecuteAsync(Func<UniTask> action, CancellationToken cts);
        void SafeExecute(Action action);
    }
}
