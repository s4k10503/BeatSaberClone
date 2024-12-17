using NUnit.Framework;
using UnityEngine;
using BeatSaberClone.Infrastructure;

namespace BeatSaberClone.Tests
{
    [TestFixture]
    public class DistanceCalculatorTests
    {
        private DistanceCalculator _distanceCalculator;

        [SetUp]
        public void Setup()
        {
            _distanceCalculator = new DistanceCalculator();
        }

        [Test]
        public void CalculateDistance_WhenSpawnPointIsNull_ShouldLogErrorAndReturnZero()
        {
            // Arrange
            Transform spawnPoint = null;
            Transform playerPoint = new GameObject().transform;

            // Act
            float result = _distanceCalculator.CalculateDistance(spawnPoint, playerPoint);

            // Assert
            Assert.AreEqual(0f, result);
        }

        [Test]
        public void CalculateDistance_WhenPlayerPointIsNull_ShouldLogErrorAndReturnZero()
        {
            // Arrange
            Transform spawnPoint = new GameObject().transform;
            Transform playerPoint = null;

            // Act
            float result = _distanceCalculator.CalculateDistance(spawnPoint, playerPoint);

            // Assert
            Assert.AreEqual(0f, result);
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
