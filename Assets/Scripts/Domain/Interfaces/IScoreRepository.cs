using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace BeatSaberClone.Domain
{
    public interface IScoreRepository : IDisposable
    {
        UniTask SaveScore(ScoreEntity scoreEntity, CancellationToken ct);
        UniTask<ScoreEntity> LoadScore(CancellationToken ct);
    }
}
