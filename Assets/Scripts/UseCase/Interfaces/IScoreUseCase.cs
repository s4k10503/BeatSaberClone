using UniRx;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace BeatSaberClone.UseCase
{
    public interface IScoreUseCase : IDisposable
    {
        IReadOnlyReactiveProperty<int> CurrentScore { get; }
        IReadOnlyReactiveProperty<int> CurrentCombo { get; }
        IReadOnlyReactiveProperty<float> CurrentComboMultiplier { get; }
        IReadOnlyReactiveProperty<float> CurrentComboProgress { get; }

        UniTask SaveScore(CancellationToken ct);
        UniTask LoadScore(CancellationToken ct);
        int UpdateScore(float multiplier);
        void UpdateCombo(float multiplier);
    }
}
