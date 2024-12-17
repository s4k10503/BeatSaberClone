using System;
using System.Collections.Generic;

namespace BeatSaberClone.Domain
{
    public class ScoreService : IScoreService
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

        public void UpdateScore(Guid id, float multiplier)
        {
            var entity = GetEntity(id);
            if (entity == null)
                throw new ArgumentException("Entity not found.");

            if (multiplier < 0)
            {
                throw new ArgumentException("Multiplier must be greater than 0.");
            }

            //int points = (int)(1 * multiplier * entity.ComboMultiplier.Value);
            entity.AddScore(multiplier);
        }

        public void UpdateCombo(Guid id, float multiplier)
        {
            var entity = GetEntity(id);
            if (entity == null)
                throw new ArgumentException("Entity not found.");

            if (multiplier < 0)
            {
                throw new ArgumentException("Multiplier cannot be negative.");
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
