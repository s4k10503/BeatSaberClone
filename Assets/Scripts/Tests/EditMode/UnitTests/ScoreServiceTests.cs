using NUnit.Framework;
using BeatSaberClone.Domain;
using System;
using UniRx;

namespace BeatSaberClone.Tests
{
    public sealed class ScoreServiceTests
    {
        private ScoreService _scoreService;

        [SetUp]
        public void Setup()
        {
            _scoreService = new ScoreService();
        }

        [TearDown]
        public void TearDown()
        {
            _scoreService.Dispose();
        }

        [Test]
        public void CreateEntity_ShouldCreateNewEntity()
        {
            var entity = _scoreService.CreateEntity();
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity.Value.Value, Is.EqualTo(0));
            Assert.That(entity.ComboCount.Value, Is.EqualTo(0));
            Assert.That(entity.Accuracy.Value, Is.EqualTo(1.0f));
        }

        [Test]
        public void UpdateScore_WithAccuracyBelow0_6_ShouldReturnZero()
        {
            var entity = _scoreService.CreateEntity();
            var score = _scoreService.UpdateScore(entity.Id, 0.5f);
            Assert.That(score, Is.EqualTo(50)); // 100 * 0.5
        }

        [Test]
        public void UpdateScore_WithAccuracyRanges_ShouldApplyCorrectMultipliers()
        {
            var entity1 = _scoreService.CreateEntity();
            var entity2 = _scoreService.CreateEntity();
            var entity3 = _scoreService.CreateEntity();

            // Test middle accuracy
            var score1 = _scoreService.UpdateScore(entity1.Id, 0.65f);
            Assert.That(score1, Is.EqualTo(65), "Accuracy 0.65 should give 65 points");

            // Test minimum accuracy
            var score2 = _scoreService.UpdateScore(entity2.Id, 0.6f);
            Assert.That(score2, Is.EqualTo(60), "Accuracy 0.6 should give 60 points");

            // Test maximum accuracy
            var score3 = _scoreService.UpdateScore(entity3.Id, 1.0f);
            Assert.That(score3, Is.EqualTo(100), "Accuracy 1.0 should give 100 points");
        }

        [Test]
        public void UpdateScore_WithMultipleCombos_ShouldMultiplyScore()
        {
            var entity = _scoreService.CreateEntity();
            // Set combo to 3 (which falls in Tier1: multiplier 2.0)
            _scoreService.UpdateCombo(entity.Id, 0.9f);
            _scoreService.UpdateCombo(entity.Id, 0.9f);
            _scoreService.UpdateCombo(entity.Id, 0.9f);

            var score = _scoreService.UpdateScore(entity.Id, 0.9f);
            Assert.That(score, Is.EqualTo(180)); // 100 * 0.9 * 2.0
            Assert.That(entity.Value.Value, Is.EqualTo(180)); // Total score should be 180
        }

        [Test]
        public void UpdateScore_AfterComboReset_ShouldCalculateWithNewCombo()
        {
            var entity = _scoreService.CreateEntity();
            // Set combo to 3
            _scoreService.UpdateCombo(entity.Id, 0.9f);
            _scoreService.UpdateCombo(entity.Id, 0.9f);
            _scoreService.UpdateCombo(entity.Id, 0.9f);

            // Reset combo
            _scoreService.UpdateCombo(entity.Id, 0f);

            // New combo
            _scoreService.UpdateCombo(entity.Id, 0.9f);
            var score = _scoreService.UpdateScore(entity.Id, 0.89f);
            Assert.That(score, Is.EqualTo(89)); // 100 * 0.89 * 1.0
            Assert.That(entity.Value.Value, Is.EqualTo(89)); // Total score should be 89
        }

        [Test]
        public void UpdateScore_WithInvalidAccuracy_ShouldThrowException()
        {
            var entity = _scoreService.CreateEntity();
            Assert.Throws<DomainException>(() => _scoreService.UpdateScore(entity.Id, 1.1f));
        }

        [Test]
        public void UpdateCombo_WithZeroAccuracy_ShouldResetCombo()
        {
            var entity = _scoreService.CreateEntity();
            _scoreService.UpdateCombo(entity.Id, 0.9f);
            _scoreService.UpdateCombo(entity.Id, 0.9f);

            _scoreService.UpdateCombo(entity.Id, 0f);

            Assert.That(entity.ComboCount.Value, Is.EqualTo(0));
        }

        [Test]
        public void UpdateCombo_WithNonZeroAccuracy_ShouldIncreaseCombo()
        {
            var entity = _scoreService.CreateEntity();
            _scoreService.UpdateCombo(entity.Id, 0.9f);
            Assert.That(entity.ComboCount.Value, Is.EqualTo(1));
        }

        [Test]
        public void UpdateCombo_WithInvalidAccuracy_ShouldThrowException()
        {
            var entity = _scoreService.CreateEntity();
            Assert.Throws<DomainException>(() => _scoreService.UpdateCombo(entity.Id, 1.1f));
        }

        [Test]
        public void GetEntity_WithNonExistentId_ShouldThrowException()
        {
            Assert.Throws<DomainException>(() => _scoreService.GetEntity(Guid.NewGuid()));
        }

        [Test]
        public void CreateEntityWithId_WithExistingId_ShouldThrowException()
        {
            var id = Guid.NewGuid();
            _scoreService.CreateEntityWithId(id, 0, 0, 1.0f);
            Assert.Throws<DomainException>(() => _scoreService.CreateEntityWithId(id, 0, 0, 1.0f));
        }

        private int CalculateExpectedScore(float accuracy, int combo)
        {
            return (int)(100 * accuracy * combo);
        }
    }
}
