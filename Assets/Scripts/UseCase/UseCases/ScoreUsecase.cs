using UniRx;
using Zenject;
using BeatSaberClone.Domain;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace BeatSaberClone.UseCase
{
    public class ScoreUseCase : IScoreUseCase, IDisposable
    {
        public IReadOnlyReactiveProperty<int> CurrentScore
            => _scoreService.GetEntity(_entityId)?.Score;
        public IReadOnlyReactiveProperty<int> CurrentCombo
            => _scoreService.GetEntity(_entityId)?.Combo;
        public IReadOnlyReactiveProperty<float> CurrentComboMultiplier
            => _scoreService.GetEntity(_entityId)?.ComboMultiplier;

        private readonly Guid _entityId;
        private readonly IScoreService _scoreService;
        private readonly IScoreRepository _scoreRepository;
        private readonly ILoggerService _logger;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        [Inject]
        public ScoreUseCase(
            IScoreRepository scoreRepository,
            IScoreService scoreDomainService,
            ILoggerService logger)
        {
            _scoreRepository = scoreRepository;
            _scoreService = scoreDomainService;
            _logger = logger;

            var entity = _scoreService.CreateEntity(0, 0);
            if (entity == null)
            {
                throw new InvalidOperationException("Failed to create ScoreEntity.");
            }
            _entityId = entity.Id;
        }

        public void Dispose()
        {
            //_logger.Log("Dispose ScoreUseCase");
            _disposables.Dispose();
            _scoreService.Dispose();
            _scoreRepository.Dispose();
            _logger.Dispose();
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

        public void UpdateScore(float multiplier)
        {
            try
            {
                _scoreService.UpdateScore(_entityId, multiplier);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.Message);
            }
        }

        public void UpdateCombo(float multiplier)
        {
            try
            {
                _scoreService.UpdateCombo(_entityId, multiplier);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}
