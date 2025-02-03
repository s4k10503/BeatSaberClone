using NUnit.Framework;
using BeatSaberClone.Domain;
using System.Collections.Generic;

namespace BeatSaberClone.Tests
{
    [TestFixture]
    public sealed class NoteSpawnServiceTests
    {
        private NoteSpawnService _noteSpawnService;

        [SetUp]
        public void Setup()
        {
            _noteSpawnService = new NoteSpawnService();
        }

        [TearDown]
        public void TearDown()
        {
            _noteSpawnService = null;
        }

        [Test]
        public void GetNotesToSpawn_WhenCurrentTimeIsNegative_ShouldReturnEmptyList()
        {
            // Arrange
            var scheduledNotes = new Queue<(float, NoteInfo)>();
            scheduledNotes.Enqueue((1.0f, new NoteInfo()));
            float currentTime = -1.0f;

            // Act
            var result = _noteSpawnService.GetNotesToSpawn(scheduledNotes, currentTime);

            // Assert
            Assert.IsEmpty(result);
            Assert.AreEqual(1, scheduledNotes.Count); // Ensure no notes are dequeued
        }

        [Test]
        public void GetNotesToSpawn_WhenCurrentTimeIsBeforeAnySpawnTime_ShouldReturnEmptyList()
        {
            // Arrange
            var scheduledNotes = new Queue<(float, NoteInfo)>();
            scheduledNotes.Enqueue((5.0f, new NoteInfo()));
            float currentTime = 2.0f;

            // Act
            var result = _noteSpawnService.GetNotesToSpawn(scheduledNotes, currentTime);

            // Assert
            Assert.IsEmpty(result);
            Assert.AreEqual(1, scheduledNotes.Count); // Ensure no notes are dequeued
        }

        [Test]
        public void GetNotesToSpawn_WhenCurrentTimeMatchesSpawnTime_ShouldReturnMatchingNotes()
        {
            // Arrange
            var note1 = new NoteInfo();
            var scheduledNotes = new Queue<(float, NoteInfo)>();
            scheduledNotes.Enqueue((3.0f, note1));
            float currentTime = 3.0f;

            // Act
            var result = _noteSpawnService.GetNotesToSpawn(scheduledNotes, currentTime);

            // Assert
            Assert.Contains(note1, result);
            Assert.AreEqual(0, scheduledNotes.Count); // Ensure the note is dequeued
        }

        [Test]
        public void GetNotesToSpawn_WhenCurrentTimeIsAfterSpawnTime_ShouldReturnAndRemoveMatchingNotes()
        {
            // Arrange
            var note1 = new NoteInfo();
            var note2 = new NoteInfo();
            var scheduledNotes = new Queue<(float, NoteInfo)>();
            scheduledNotes.Enqueue((1.0f, note1));
            scheduledNotes.Enqueue((2.0f, note2));
            float currentTime = 2.5f;

            // Act
            var result = _noteSpawnService.GetNotesToSpawn(scheduledNotes, currentTime);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.Contains(note1, result);
            Assert.Contains(note2, result);
            Assert.AreEqual(0, scheduledNotes.Count); // Ensure all matching notes are dequeued
        }

        [Test]
        public void GetNotesToSpawn_WhenCurrentTimeIsBeforeSomeSpawnTimes_ShouldOnlyReturnMatchingNotes()
        {
            // Arrange
            var note1 = new NoteInfo();
            var note2 = new NoteInfo();
            var scheduledNotes = new Queue<(float, NoteInfo)>();
            scheduledNotes.Enqueue((1.0f, note1));
            scheduledNotes.Enqueue((3.0f, note2));
            float currentTime = 1.5f;

            // Act
            var result = _noteSpawnService.GetNotesToSpawn(scheduledNotes, currentTime);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.Contains(note1, result);
            Assert.AreEqual(1, scheduledNotes.Count); // Ensure only one note remains in the queue
            Assert.AreEqual((3.0f, note2), scheduledNotes.Peek()); // Ensure the remaining note is correct
        }
    }
}
