using System;
using System.IO;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using BeatSaberClone.Domain;
using Zenject;

namespace BeatSaberClone.Infrastructure
{
    public sealed class JsonScoreRepository : IScoreRepository
    {
        private const string ScoreFileName = "score.json";

        [Inject]
        public JsonScoreRepository()
        {
        }

        public void Dispose()
        {
        }

        public async UniTask SaveScore(ScoreEntity scoreEntity, CancellationToken ct)
        {
            try
            {
                var scoreContainer = new ScoreContainer
                {
                    Score = scoreEntity.Score.Value,
                    MaxCombo = scoreEntity.MaxCombo.Value
                };

                string json = JsonUtility.ToJson(scoreContainer);
                string path = Path.Combine(Application.persistentDataPath, ScoreFileName);
                await File.WriteAllTextAsync(path, json, ct);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Failed to save score to JSON: ", ex);
            }
        }

        public async UniTask<ScoreEntity> LoadScore(CancellationToken ct)
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, ScoreFileName);

                if (!File.Exists(path))
                {
                    throw new InfrastructureException("Score file not found.");
                }

                string json = await File.ReadAllTextAsync(path, ct);
                ScoreContainer loadedScore = JsonUtility.FromJson<ScoreContainer>(json)
                    ?? throw new InfrastructureException("Loaded score data is invalid.");

                return new ScoreEntity(
                    Guid.NewGuid(),
                    loadedScore.Score,
                    0,
                    loadedScore.MaxCombo
                );
            }
            catch (Exception ex)
            {
                throw new InfrastructureException("Failed to load score from JSON: ", ex);
            }
        }
    }
}
