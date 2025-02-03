using NUnit.Framework;
using BeatSaberClone.Domain;
using System;

namespace BeatSaberClone.Tests
{
    public sealed class ScoreServiceTests
    {
        private ScoreService _scoreService;
        private Guid _entityId;

        [SetUp]
        public void Setup()
        {
            _scoreService = new ScoreService();
            var entity = _scoreService.CreateEntity(0, 0);
            _entityId = entity.Id;
        }

        [TearDown]
        public void TearDown()
        {
            _scoreService = null;
        }

        [Test]
        public void UpdateScore_WithPositiveMultiplier_ShouldUpdateScore()
        {
            // Arrange
            float multiplier = 2.0f;
            var entity = _scoreService.GetEntity(_entityId);
            int initialScore = entity.Score.Value;

            // Act
            _scoreService.UpdateScore(_entityId, multiplier);

            // Assert
            Assert.AreEqual(initialScore + (int)(100 * multiplier * entity.ComboMultiplier.Value), entity.Score.Value);
        }

        [Test]
        public void UpdateScore_WithNegativeMultiplier_ShouldThrowArgumentException()
        {
            // Arrange
            float negativeMultiplier = -1.0f;

            // Act & Assert
            var ex = Assert.Throws<DomainException>(() => _scoreService.UpdateScore(_entityId, negativeMultiplier));
            Assert.That(ex.Message, Is.EqualTo("Multiplier must be over 0."));
        }

        [Test]
        public void UpdateCombo_WithPositiveMultiplier_ShouldIncreaseCombo()
        {
            // Arrange
            float multiplier = 1.0f;
            var entity = _scoreService.GetEntity(_entityId);
            int initialCombo = entity.Combo.Value;

            // Act
            _scoreService.UpdateCombo(_entityId, multiplier);

            // Assert
            Assert.AreEqual(initialCombo + 1, entity.Combo.Value);
        }

        [Test]
        public void UpdateCombo_WithZeroMultiplier_ShouldResetCombo()
        {
            // Arrange
            _scoreService.UpdateCombo(_entityId, 1.0f);
            var entity = _scoreService.GetEntity(_entityId);

            // Act
            _scoreService.UpdateCombo(_entityId, 0.0f);

            // Assert
            Assert.AreEqual(0, entity.Combo.Value);
        }

        [Test]
        public void UpdateCombo_WithNegativeMultiplier_ShouldThrowArgumentException()
        {
            // Arrange
            float negativeMultiplier = -1.0f;

            // Act & Assert
            var ex = Assert.Throws<DomainException>(() => _scoreService.UpdateCombo(_entityId, negativeMultiplier));
            Assert.That(ex.Message, Is.EqualTo("Multiplier cannot be negative values."));
        }
    }
}
