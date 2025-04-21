using System;
using System.Collections.Generic;
using UniRx;

namespace BeatSaberClone.Domain
{
    public sealed class ScoreService : IScoreService, IDisposable
    {
        private readonly Dictionary<Guid, ScoreEntity> _entities = new Dictionary<Guid, ScoreEntity>();

        public ScoreEntity CreateEntity(int initialScore = 0, int initialCombo = 0, float initialAccuracy = 1.0f)
        {
            var entity = new ScoreEntity(Guid.NewGuid(), initialScore, initialCombo, initialAccuracy, 0);
            _entities.Add(entity.Id, entity);
            return entity;
        }

        public ScoreEntity CreateEntityWithId(Guid id, int initialScore, int initialCombo, float initialAccuracy)
        {
            if (_entities.ContainsKey(id))
            {
                throw new DomainException($"Entity with ID {id} already exists.");
            }

            var entity = new ScoreEntity(id, initialScore, initialCombo, initialAccuracy, 0);
            _entities.Add(entity.Id, entity);
            return entity;
        }

        public ScoreEntity GetEntity(Guid id)
        {
            if (!_entities.TryGetValue(id, out var entity))
            {
                throw new DomainException($"ScoreEntity with ID {id} not found.");
            }
            return entity;
        }

        public int UpdateScore(Guid id, float accuracy)
        {
            if (accuracy < 0 || accuracy > 1)
            {
                throw new DomainException("Accuracy must be between 0 and 1.");
            }

            var entity = GetEntity(id);
            entity.UpdateAccuracy(accuracy);
            return entity.Value.Value;
        }

        public void UpdateCombo(Guid id, float accuracy)
        {
            if (accuracy < 0 || accuracy > 1)
            {
                throw new DomainException("Accuracy must be between 0 and 1.");
            }

            var entity = GetEntity(id);
            if (accuracy == 0)
            {
                entity.ResetCombo();
            }
            else
            {
                entity.AddCombo();
            }
        }

        public void RegisterEntity(ScoreEntity entity)
        {
            if (_entities.ContainsKey(entity.Id))
            {
                _entities[entity.Id].Dispose();
            }
            _entities[entity.Id] = entity;
        }

        public void Dispose()
        {
            foreach (var entity in _entities.Values)
            {
                entity.Dispose();
            }
            _entities.Clear();
        }
    }
}
