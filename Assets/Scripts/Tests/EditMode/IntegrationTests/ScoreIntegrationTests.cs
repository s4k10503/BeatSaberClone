using NUnit.Framework;
using NSubstitute;
using BeatSaberClone.Domain;
using BeatSaberClone.UseCase;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;

namespace BeatSaberClone.Tests
{
    // Note: It is not a complete integration test because it uses Repository mock
    [TestFixture]
    public sealed class ScoreIntegrationTests
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

        [Test]
        public void UpdateScore_ShouldCorrectlyUpdateScore()
        {
            // Arrange
            _scoreUseCase.UpdateCombo(1.0f);
            float multiplier = 2.0f;

            // Act
            _scoreUseCase.UpdateScore(multiplier);

            // Assert
            Assert.AreEqual(200, _scoreUseCase.CurrentScore.Value);
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
            var scoreEntity = new ScoreEntity(Guid.NewGuid(), 200, 2, 2);
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
    }
}
