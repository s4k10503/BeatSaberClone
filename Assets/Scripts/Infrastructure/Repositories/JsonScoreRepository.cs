using System.IO;
using UnityEngine;
using BeatSaberClone.Domain;
using System;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace BeatSaberClone.Infrastructure
{
    public class JsonScoreRepository : IScoreRepository
    {
        private const string ScoreFileName = "score.json";
        private readonly ILoggerService _logger;

        [Inject]
        public JsonScoreRepository(ILoggerService logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            _logger.Dispose();
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
            catch (Exception e)
            {
                _logger.LogError($"Failed to save score to JSON: {e.Message}");
            }
        }

        public async UniTask<ScoreEntity> LoadScore(CancellationToken ct)
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, ScoreFileName);

                if (File.Exists(path))
                {
                    string json = await File.ReadAllTextAsync(path, ct);
                    ScoreContainer loadedScore = JsonUtility.FromJson<ScoreContainer>(json);

                    return new ScoreEntity(
                        Guid.NewGuid(),
                        loadedScore.Score,
                        0,
                        loadedScore.MaxCombo
                    );
                }
                else
                {
                    _logger.LogError("Score file not found");
                    return new ScoreEntity(Guid.NewGuid(), 0, 0, 0);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to load score from JSON: {e.Message}");
                return new ScoreEntity(Guid.NewGuid(), 0, 0, 0);
            }
        }
    }
}
