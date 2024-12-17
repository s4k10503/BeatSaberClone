using NUnit.Framework;
using BeatSaberClone.Domain;
using System.Collections.Generic;

namespace BeatSaberClone.Tests
{
    [TestFixture]
    public class NoteSchedulerTests
    {
        private NoteScheduler _noteScheduler;

        [SetUp]
        public void Setup()
        {
            _noteScheduler = new NoteScheduler();
        }

        [Test]
        public void ScheduleNotes_WhenBoxMoveSpeedIsZeroOrNegative_ShouldLogErrorAndReturnEmptyList()
        {
            // Arrange
            var notes = new List<NoteInfo> { new NoteInfo { _time = 5.0f } };
            float distance = 10.0f;
            float boxMoveSpeed = 0.0f;

            // Act
            var result = _noteScheduler.ScheduleNotes(notes, distance, boxMoveSpeed);

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void ScheduleNotes_WhenNoteTimeIsLessThanTimeToReachPlayer_ShouldLogWarningAndSkipNote()
        {
            // Arrange
            var notes = new List<NoteInfo> { new NoteInfo { _time = 2.0f } };
            float distance = 10.0f;
            float boxMoveSpeed = 5.0f; // Time to reach player = 10.0 / 5.0 = 2.0

            // Act
            var result = _noteScheduler.ScheduleNotes(notes, distance, boxMoveSpeed);

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void ScheduleNotes_WhenValidNotesAreProvided_ShouldScheduleNotesCorrectly()
        {
            // Arrange
            var note1 = new NoteInfo { _time = 6.0f };
            var note2 = new NoteInfo { _time = 10.0f };
            var notes = new List<NoteInfo> { note1, note2 };
            float distance = 10.0f;
            float boxMoveSpeed = 2.0f; // Time to reach player = 10.0 / 2.0 = 5.0

            // Act
            var result = _noteScheduler.ScheduleNotes(notes, distance, boxMoveSpeed);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1.0f, result[0].spawnTime); // note1 spawn time = 6.0 - 5.0
            Assert.AreEqual(5.0f, result[1].spawnTime); // note2 spawn time = 10.0 - 5.0
        }

        [Test]
        public void ScheduleNotes_WhenNoteTimeEqualsTimeToReachPlayer_ShouldScheduleTheNote()
        {
            // Arrange
            var note = new NoteInfo { _time = 5.0f };
            var notes = new List<NoteInfo> { note };
            float distance = 10.0f;
            float boxMoveSpeed = 2.0f; // Time to reach player = 10.0 / 2.0 = 5.0

            // Act
            var result = _noteScheduler.ScheduleNotes(notes, distance, boxMoveSpeed);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.That(result[0].spawnTime, Is.EqualTo(0.0f)); // note spawn time = 5.0 - 5.0
        }
    }
}
