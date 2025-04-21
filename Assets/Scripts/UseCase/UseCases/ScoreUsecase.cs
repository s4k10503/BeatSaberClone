using UniRx;
using Zenject;
using BeatSaberClone.Domain;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace BeatSaberClone.UseCase
{
    public sealed class ScoreUseCase : IScoreUseCase, IDisposable
    {
        public IReadOnlyReactiveProperty<int> CurrentScore
            => _scoreService.GetEntity(_entityId)?.Value;
        public IReadOnlyReactiveProperty<int> CurrentCombo
            => _scoreService.GetEntity(_entityId)?.ComboCount;
        public IReadOnlyReactiveProperty<float> CurrentComboMultiplier
            => _scoreService.GetEntity(_entityId)?.ComboMultiplier;
        public IReadOnlyReactiveProperty<float> CurrentAccuracy
            => _scoreService.GetEntity(_entityId)?.Accuracy;

        public IReadOnlyReactiveProperty<float> CurrentComboProgress => _comboProgress;

        private Guid _entityId;
        private readonly IScoreService _scoreService;
        private readonly IScoreRepository _scoreRepository;
        private readonly CompositeDisposable _disposables = new();
        private readonly ReactiveProperty<float> _comboProgress;

        [Inject]
        public ScoreUseCase(
            IScoreRepository scoreRepository,
            IScoreService scoreDomainService)
        {
            _scoreRepository = scoreRepository;
            _scoreService = scoreDomainService;

            var entity = _scoreService.CreateEntity(0, 0, 1.0f)
                ?? throw new InvalidOperationException("Failed to create ScoreEntity.");
            _entityId = entity.Id;

            _comboProgress = new ReactiveProperty<float>();
            _comboProgress.AddTo(_disposables);

            entity.ComboProgress
                .Subscribe(x => _comboProgress.Value = x)
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public async UniTask SaveScore(CancellationToken ct)
        {
            var entity = _scoreService.GetEntity(_entityId);
            if (entity == null) return;
            await _scoreRepository.SaveScore(entity, ct);
        }

        public async UniTask LoadScore(CancellationToken ct)
        {
            var entity = await _scoreRepository.LoadScore(ct);
            if (entity == null) return;
            var newEntity = _scoreService.CreateEntity(entity.Value.Value, entity.ComboCount.Value, entity.Accuracy.Value);
            _entityId = newEntity.Id;
        }

        public int UpdateScore(float accuracy)
        {
            var entity = _scoreService.GetEntity(_entityId);
            if (entity == null) return 0;

            // Calculate note score before updating
            var noteScore = new Score(0).CalculatePoints(accuracy, entity.ComboMultiplier.Value);

            // Update the score
            _scoreService.UpdateScore(_entityId, accuracy);

            // Return the note score value
            return noteScore.Value;
        }

        public int GetNoteScore(float accuracy)
        {
            var entity = _scoreService.GetEntity(_entityId);
            if (entity == null) return 0;
            return new Score(0).CalculatePoints(accuracy, entity.ComboMultiplier.Value).Value;
        }

        public void UpdateCombo(float accuracy)
        {
            _scoreService.UpdateCombo(_entityId, accuracy);
        }
    }
}
