using System;
using System.Collections.Generic;

namespace BeatSaberClone.Domain
{
    public sealed class ScoreService : IScoreService
    {
        private readonly Dictionary<Guid, ScoreEntity> _entities;

        public ScoreService()
        {
            _entities = new Dictionary<Guid, ScoreEntity>();
        }

        public void Dispose()
        {
        }

        public ScoreEntity CreateEntity(int initialScore, int initialCombo)
        {
            var entity = new ScoreEntity(Guid.NewGuid(), initialScore, initialCombo);
            _entities[entity.Id] = entity;
            return entity;
        }

        public ScoreEntity GetEntity(Guid id)
        {
            return _entities.TryGetValue(id, out var entity) ? entity : null;
        }

        public int UpdateScore(Guid id, float multiplier)
        {
            var entity = GetEntity(id);
            if (entity == null)
                throw new DomainException("I can't find an entity.");

            if (multiplier < 0)
            {
                throw new DomainException("Multiplier must be over 0.");
            }

            return entity.AddScore(multiplier);
        }

        public void UpdateCombo(Guid id, float multiplier)
        {
            var entity = GetEntity(id) ?? throw new DomainException("I can't find an entity.");

            if (multiplier < 0)
            {
                throw new DomainException("Multiplier cannot be negative values.");
            }

            if (multiplier == 0)
            {
                entity.ResetCombo();
            }
            else
            {
                entity.AddCombo();
            }
        }
    }
}
