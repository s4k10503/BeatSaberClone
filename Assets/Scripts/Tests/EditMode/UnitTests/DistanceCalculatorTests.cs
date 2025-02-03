using NUnit.Framework;
using UnityEngine;
using BeatSaberClone.Infrastructure;

namespace BeatSaberClone.Tests
{
    [TestFixture]
    public sealed class DistanceCalculatorTests
    {
        private DistanceCalculator _distanceCalculator;

        [SetUp]
        public void Setup()
        {
            _distanceCalculator = new DistanceCalculator();
        }

        [TearDown]
        public void TearDown()
        {
            _distanceCalculator = null;
        }

        [Test]
        public void CalculateDistance_WhenSpawnPointIsNull_ShouldLogErrorAndReturnZero()
        {
            // Arrange
            Transform spawnPoint = null;
            Transform playerPoint = new GameObject().transform;

            // Act
            var ex = Assert.Throws<InfrastructureException>(() => _distanceCalculator.CalculateDistance(spawnPoint, playerPoint));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Spawnpoint is null."));
        }

        [Test]
        public void CalculateDistance_WhenPlayerPointIsNull_ShouldLogErrorAndReturnZero()
        {
            // Arrange
            Transform spawnPoint = new GameObject().transform;
            Transform playerPoint = null;

            // Act
            var ex = Assert.Throws<InfrastructureException>(() => _distanceCalculator.CalculateDistance(spawnPoint, playerPoint));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Playerpoint is null."));
        }

        [Test]
        public void CalculateDistance_WhenBothPointsAreValid_ShouldReturnCorrectDistance()
        {
            // Arrange
            Transform spawnPoint = new GameObject().transform;
            Transform playerPoint = new GameObject().transform;
            spawnPoint.position = new Vector3(0, 0, 10);
            playerPoint.position = new Vector3(0, 0, 5);

            // Act
            float result = _distanceCalculator.CalculateDistance(spawnPoint, playerPoint);

            // Assert
            Assert.AreEqual(5f, result);
        }

        [Test]
        public void CalculateDistance_WhenPositionsAreNegative_ShouldReturnCorrectDistance()
        {
            // Arrange
            Transform spawnPoint = new GameObject().transform;
            Transform playerPoint = new GameObject().transform;
            spawnPoint.position = new Vector3(0, 0, -15);
            playerPoint.position = new Vector3(0, 0, -10);

            // Act
            float result = _distanceCalculator.CalculateDistance(spawnPoint, playerPoint);

            // Assert
            Assert.AreEqual(5f, result);
        }
    }
}
