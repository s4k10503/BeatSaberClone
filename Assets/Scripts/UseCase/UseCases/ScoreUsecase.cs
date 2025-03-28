using UniRx;
using Zenject;
using BeatSaberClone.Domain;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BeatSaberClone.UseCase
{
    public sealed class ScoreUseCase : IScoreUseCase, IDisposable
    {
        public IReadOnlyReactiveProperty<int> CurrentScore
            => _scoreService.GetEntity(_entityId)?.Score;
        public IReadOnlyReactiveProperty<int> CurrentCombo
            => _scoreService.GetEntity(_entityId)?.Combo;
        public IReadOnlyReactiveProperty<float> CurrentComboMultiplier
            => _scoreService.GetEntity(_entityId)?.ComboMultiplier;

        public IReadOnlyReactiveProperty<float> CurrentComboProgress { get; }

        private readonly Guid _entityId;
        private readonly IScoreService _scoreService;
        private readonly IScoreRepository _scoreRepository;
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public ScoreUseCase(
            IScoreRepository scoreRepository,
            IScoreService scoreDomainService)
        {
            _scoreRepository = scoreRepository;
            _scoreService = scoreDomainService;

            var entity = _scoreService.CreateEntity(0, 0)
                ?? throw new InvalidOperationException("Failed to create ScoreEntity.");
            _entityId = entity.Id;

            CurrentComboProgress = Observable.CombineLatest(
                entity.Combo, entity.ComboMultiplier,
                (combo, multiplier) =>
                {
                    if (multiplier == 1f)
                    {
                        // In the case of 0 to 1, 0 to 2 to make it easier to understand the progress.
                        return combo / 2f;
                    }
                    else if (multiplier == 2f)
                    {
                        // Combo 2-5 (width 3)
                        return (combo - 2) / 3f;
                    }
                    else if (multiplier == 4f)
                    {
                        // Combo 6-13 (width 7)
                        return (combo - 6) / 7f;
                    }
                    else if (multiplier == 8f)
                    {
                        // Assuming combo 14-21 (width 7)
                        return Mathf.Clamp01((combo - 14) / 7f);
                    }
                    return 0f;
                })
                .ToReactiveProperty();
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _scoreService.Dispose();
            _scoreRepository.Dispose();
        }

        public async UniTask SaveScore(CancellationToken ct)
        {
            var entity = _scoreService.GetEntity(_entityId);
            await _scoreRepository.SaveScore(entity, ct);
        }

        public async UniTask LoadScore(CancellationToken ct)
        {
            var loadedEntity = await _scoreRepository.LoadScore(ct);
            if (loadedEntity != null)
            {
                var entity = _scoreService.GetEntity(_entityId);
                if (entity != null)
                {
                    entity.Score.Value = loadedEntity.Score.Value;
                    entity.Combo.Value = loadedEntity.Combo.Value;
                }
            }
        }

        public int UpdateScore(float multiplier)
        {
            return _scoreService.UpdateScore(_entityId, multiplier);
        }

        public void UpdateCombo(float multiplier)
        {
            _scoreService.UpdateCombo(_entityId, multiplier);
        }
    }
}
