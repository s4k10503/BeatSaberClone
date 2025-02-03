using NUnit.Framework;
using BeatSaberClone.Domain;
using System.Collections.Generic;

namespace BeatSaberClone.Tests
{
    [TestFixture]
    public sealed class NoteSchedulerTests
    {
        private NoteScheduler _noteScheduler;

        [SetUp]
        public void Setup()
        {
            _noteScheduler = new NoteScheduler();
        }

        [TearDown]
        public void TearDown()
        {
            _noteScheduler = null;
        }

        [Test]
        public void ScheduleNotes_WhenInitialOrFinalSpeedIsZeroOrNegative_ShouldReturnEmptyList()
        {
            // Arrange
            var notes = new List<NoteInfo> { new NoteInfo { _time = 5.0f } };
            float totalDistance = 10.0f;
            float initialSpeed = 0.0f;
            float finalSpeed = 2.0f;
            float slowDownDistanceFromPlayer = 3.0f;

            // Act
            var result = _noteScheduler.ScheduleNotes(notes, totalDistance, initialSpeed, finalSpeed, slowDownDistanceFromPlayer);

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void ScheduleNotes_WhenNoteTimeIsLessThanTimeToReachPlayer_ShouldSkipNote()
        {
            // Arrange
            var notes = new List<NoteInfo> { new NoteInfo { _time = 2.0f } };
            float totalDistance = 10.0f;
            float initialSpeed = 5.0f;
            float finalSpeed = 2.0f;
            float slowDownDistanceFromPlayer = 4.0f;

            // Calculate expected time to reach player
            float fastPhaseDistance = totalDistance - slowDownDistanceFromPlayer; // 6.0
            float timeToReachPlayer = (fastPhaseDistance / initialSpeed) + (slowDownDistanceFromPlayer / finalSpeed); // (6.0 / 5.0) + (4.0 / 2.0) = 1.2 + 2.0 = 3.2

            // Act
            var result = _noteScheduler.ScheduleNotes(notes, totalDistance, initialSpeed, finalSpeed, slowDownDistanceFromPlayer);

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void ScheduleNotes_WhenValidNotesAreProvided_ShouldScheduleNotesCorrectly()
        {
            // Arrange
            var note1 = new NoteInfo { _time = 8.0f };
            var note2 = new NoteInfo { _time = 12.0f };
            var notes = new List<NoteInfo> { note1, note2 };
            float totalDistance = 10.0f;
            float initialSpeed = 4.0f;
            float finalSpeed = 2.0f;
            float slowDownDistanceFromPlayer = 6.0f;

            // Calculate expected time to reach player
            float fastPhaseDistance = totalDistance - slowDownDistanceFromPlayer; // 4.0
            float timeToReachPlayer = (fastPhaseDistance / initialSpeed) + (slowDownDistanceFromPlayer / finalSpeed); // (4.0 / 4.0) + (6.0 / 2.0) = 1.0 + 3.0 = 4.0

            // Act
            var result = _noteScheduler.ScheduleNotes(notes, totalDistance, initialSpeed, finalSpeed, slowDownDistanceFromPlayer);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(4.0f, result[0].spawnTime); // note1 spawn time = 8.0 - 4.0
            Assert.AreEqual(8.0f, result[1].spawnTime); // note2 spawn time = 12.0 - 4.0
        }

        [Test]
        public void ScheduleNotes_WhenNoteTimeEqualsTimeToReachPlayer_ShouldScheduleTheNote()
        {
            // Arrange
            var note = new NoteInfo { _time = 5.0f };
            var notes = new List<NoteInfo> { note };
            float totalDistance = 10.0f;
            float initialSpeed = 5.0f;
            float finalSpeed = 2.0f;
            float slowDownDistanceFromPlayer = 4.0f;

            // Calculate expected time to reach player
            float fastPhaseDistance = totalDistance - slowDownDistanceFromPlayer; // 6.0
            float timeToReachPlayer = (fastPhaseDistance / initialSpeed) + (slowDownDistanceFromPlayer / finalSpeed); // (6.0 / 5.0) + (4.0 / 2.0) = 1.2 + 2.0 = 3.2

            // Act
            var result = _noteScheduler.ScheduleNotes(notes, totalDistance, initialSpeed, finalSpeed, slowDownDistanceFromPlayer);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.That(result[0].spawnTime, Is.EqualTo(1.8f)); // note spawn time = 5.0 - 3.2
        }
    }
}
