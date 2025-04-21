using NUnit.Framework;
using NSubstitute;
using BeatSaberClone.Domain;
using BeatSaberClone.UseCase;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;
using BeatSaberClone.Infrastructure;
using UniRx;

namespace BeatSaberClone.Tests
{
    [TestFixture]
    public sealed class ScoreUseCaseTests
    {
        private IScoreRepository _mockScoreRepository;
        private ScoreService _scoreService;
        private ScoreUseCase _scoreUseCase;

        [SetUp]
        public void SetUp()
        {
            _mockScoreRepository = Substitute.For<IScoreRepository>();
            _scoreService = new ScoreService();
            _scoreUseCase = new ScoreUseCase(
                _mockScoreRepository,
                _scoreService);

            Assert.AreEqual(0, _scoreUseCase.CurrentScore.Value);
            Assert.AreEqual(0, _scoreUseCase.CurrentCombo.Value);
            Assert.AreEqual(1.0f, _scoreUseCase.CurrentComboMultiplier.Value);
        }

        [TearDown]
        public void TearDown()
        {
            _scoreService.Dispose();
        }

        [Test]
        public void UpdateScore_ShouldCorrectlyUpdateScore()
        {
            // Arrange
            _scoreUseCase.UpdateCombo(1.0f);
            float accuracy = 0.9f;

            // Act
            _scoreUseCase.UpdateScore(accuracy);

            // Assert
            var score = new Score(0);
            var expectedScore = score.Add(score.CalculatePoints(accuracy, 1.0f)).Value;
            Assert.AreEqual(expectedScore, _scoreUseCase.CurrentScore.Value);
        }

        [Test]
        public void UpdateCombo_WithPositiveMultiplier_ShouldIncreaseCombo()
        {
            // Arrange
            Assert.AreEqual(0, _scoreUseCase.CurrentCombo.Value);
            float multiplier = 1.0f;

            // Act
            _scoreUseCase.UpdateCombo(multiplier);

            // Assert
            Assert.AreEqual(1, _scoreUseCase.CurrentCombo.Value);
        }

        [Test]
        public void UpdateCombo_WithZeroMultiplier_ShouldResetCombo()
        {
            // Arrange
            _scoreUseCase.UpdateCombo(1.0f);
            Assert.AreEqual(1, _scoreUseCase.CurrentCombo.Value);

            // Act
            _scoreUseCase.UpdateCombo(0.0f);

            // Assert
            Assert.AreEqual(0, _scoreUseCase.CurrentCombo.Value);
        }

        [UnityTest]
        public IEnumerator LoadScore_ShouldRestoreState() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var scoreEntity = new ScoreEntity(Guid.NewGuid(), 200, 2, 2.0f, 2);
            _mockScoreRepository
                .LoadScore(Arg.Any<CancellationToken>())
                .Returns(UniTask.FromResult(scoreEntity));

            // Act
            await _scoreUseCase.LoadScore(CancellationToken.None);

            // Assert
            Assert.AreEqual(200, _scoreUseCase.CurrentScore.Value);
            Assert.AreEqual(2, _scoreUseCase.CurrentCombo.Value);
            Assert.AreEqual(2.0f, _scoreUseCase.CurrentComboMultiplier.Value);
        });

        [Test]
        public void CreateAndUpdateScore_WithAccuracyRanges_ShouldApplyCorrectMultipliers()
        {
            var entity1 = _scoreService.CreateEntity();
            var entity2 = _scoreService.CreateEntity();
            var entity3 = _scoreService.CreateEntity();

            // Test middle accuracy
            _scoreUseCase.UpdateCombo(1.0f);
            _scoreUseCase.UpdateScore(0.65f);
            Assert.AreEqual(65, _scoreUseCase.CurrentScore.Value, "Accuracy 0.65 should give 65 points");

            // Test minimum accuracy
            _scoreUseCase = new ScoreUseCase(_mockScoreRepository, _scoreService);
            _scoreUseCase.UpdateCombo(1.0f);
            _scoreUseCase.UpdateScore(0.6f);
            Assert.AreEqual(60, _scoreUseCase.CurrentScore.Value, "Accuracy 0.6 should give 60 points");

            // Test maximum accuracy
            _scoreUseCase = new ScoreUseCase(_mockScoreRepository, _scoreService);
            _scoreUseCase.UpdateCombo(1.0f);
            _scoreUseCase.UpdateScore(1.0f);
            Assert.AreEqual(100, _scoreUseCase.CurrentScore.Value, "Accuracy 1.0 should give 100 points");
        }

        [Test]
        public void UpdateScore_WithInvalidAccuracy_ShouldThrowException()
        {
            Assert.Throws<DomainException>(() => _scoreUseCase.UpdateScore(1.1f));
        }

        [Test]
        public void UpdateScore_WithMultipleCombos_ShouldMultiplyScore()
        {
            // Set combo to 3 (which falls in Tier1: multiplier 2.0)
            _scoreUseCase.UpdateCombo(1.0f);
            _scoreUseCase.UpdateCombo(1.0f);
            _scoreUseCase.UpdateCombo(1.0f);

            _scoreUseCase.UpdateScore(0.9f);
            Assert.AreEqual(180, _scoreUseCase.CurrentScore.Value); // 100 * 0.9 * 2.0
        }

        [Test]
        public void UpdateScore_AfterComboReset_ShouldCalculateWithNewCombo()
        {
            // Set combo to 3
            _scoreUseCase.UpdateCombo(1.0f);
            _scoreUseCase.UpdateCombo(1.0f);
            _scoreUseCase.UpdateCombo(1.0f);

            // Reset combo
            _scoreUseCase.UpdateCombo(0.0f);

            // New combo
            _scoreUseCase.UpdateCombo(1.0f);
            _scoreUseCase.UpdateScore(0.9f);
            Assert.AreEqual(90, _scoreUseCase.CurrentScore.Value); // 100 * 0.9 * 1.0
        }

        [Test]
        public void UpdateCombo_WithZeroAccuracy_ShouldResetCombo()
        {
            _scoreUseCase.UpdateCombo(1.0f);
            _scoreUseCase.UpdateCombo(1.0f);

            _scoreUseCase.UpdateCombo(0.0f);

            Assert.AreEqual(0, _scoreUseCase.CurrentCombo.Value);
        }

        [Test]
        public void UpdateCombo_WithNonZeroAccuracy_ShouldIncreaseCombo()
        {
            _scoreUseCase.UpdateCombo(1.0f);
            Assert.AreEqual(1, _scoreUseCase.CurrentCombo.Value);
        }

        [Test]
        public void UpdateCombo_WithInvalidAccuracy_ShouldThrowException()
        {
            Assert.Throws<DomainException>(() => _scoreUseCase.UpdateCombo(1.1f));
        }
    }
}
