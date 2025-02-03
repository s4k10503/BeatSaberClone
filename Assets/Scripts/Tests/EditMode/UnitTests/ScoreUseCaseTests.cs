using NSubstitute;
using NUnit.Framework;
using BeatSaberClone.Domain;
using BeatSaberClone.UseCase;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.TestTools;
using System.Collections;

namespace BeatSaberClone.Tests
{
    [TestFixture]
    public sealed class ScoreUseCaseTests
    {
        private IScoreRepository _mockScoreRepository;
        private IScoreService _mockScoreService;
        private ScoreUseCase _scoreUseCase;
        private Guid _entityId;

        [SetUp]
        public void SetUp()
        {
            // Mock creation
            _mockScoreRepository = Substitute.For<IScoreRepository>();
            _mockScoreService = Substitute.For<IScoreService>();

            // Set Createentity with mock
            var scoreEntity = new ScoreEntity(Guid.NewGuid(), 0, 0, 1);
            _mockScoreService.CreateEntity(0, 0).Returns(scoreEntity);
            _mockScoreService.GetEntity(scoreEntity.Id).Returns(scoreEntity);
            _entityId = scoreEntity.Id;

            // Set the mock Updatescore operation
            _mockScoreService
                .When(x => x.UpdateScore(Arg.Any<Guid>(), Arg.Any<float>()))
                .Do(callInfo =>
                {
                    var id = callInfo.ArgAt<Guid>(0);
                    var multiplier = callInfo.ArgAt<float>(1);
                    var entity = _mockScoreService.GetEntity(id);
                    entity?.AddScore(multiplier);
                });

            // Set mock UpdateCombo operation
            _mockScoreService
                .When(x => x.UpdateCombo(Arg.Any<Guid>(), Arg.Any<float>()))
                .Do(callInfo =>
                {
                    var id = callInfo.ArgAt<Guid>(0);
                    var multiplier = callInfo.ArgAt<float>(1);
                    var entity = _mockScoreService.GetEntity(id);
                    if (multiplier == 0)
                        entity?.ResetCombo();
                    else
                        entity?.AddCombo();
                });

            // Initialize the class to be tested
            _scoreUseCase = new ScoreUseCase(
                _mockScoreRepository,
                _mockScoreService);
        }

        [TearDown]
        public void TearDown()
        {
            _mockScoreRepository = null;
            _mockScoreService = null;
            _scoreUseCase = null;
        }

        [Test]
        public void UpdateScore_WithPositiveMultiplier_ShouldCallUpdateScoreOnService()
        {
            // Arrange
            float multiplier = 2.0f;

            // Act
            _scoreUseCase.UpdateScore(multiplier);

            // Assert
            _mockScoreService.Received(1).UpdateScore(_entityId, multiplier);
        }

        [Test]
        public void UpdateCombo_WithPositiveMultiplier_ShouldCallUpdateComboOnService()
        {
            // Arrange
            float multiplier = 1.0f;

            // Act
            _scoreUseCase.UpdateCombo(multiplier);

            // Assert
            _mockScoreService.Received(1).UpdateCombo(_entityId, multiplier);
        }

        [Test]
        public void UpdateCombo_WithZeroMultiplier_ShouldResetCombo()
        {
            // Arrange
            float multiplier = 0.0f;

            // Act
            _scoreUseCase.UpdateCombo(multiplier);

            // Assert
            _mockScoreService.Received(1).UpdateCombo(_entityId, multiplier);
        }

        [UnityTest]
        public IEnumerator LoadScore_ShouldRestoreStateFromRepository() => UniTask.ToCoroutine(async () =>
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
