using System;

namespace BeatSaberClone.Domain
{
    public interface IScoreService : IDisposable
    {
        ScoreEntity CreateEntity(int initialScore = 0, int initialCombo = 0, float initialAccuracy = 1.0f);
        ScoreEntity GetEntity(Guid id);
        int UpdateScore(Guid id, float accuracy);
        void UpdateCombo(Guid id, float accuracy);
    }
}
