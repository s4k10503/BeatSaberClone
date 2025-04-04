using System;

namespace BeatSaberClone.Domain
{
    public interface IScoreService : IDisposable
    {
        ScoreEntity CreateEntity(int initialScore, int initialCombo);
        ScoreEntity GetEntity(Guid id);
        int UpdateScore(Guid id, float multiplier);
        void UpdateCombo(Guid id, float multiplier);

    }
}
