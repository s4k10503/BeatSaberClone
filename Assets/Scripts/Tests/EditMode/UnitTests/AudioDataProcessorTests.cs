using NUnit.Framework;
using UnityEngine;
using BeatSaberClone.Domain;
using BeatSaberClone.Infrastructure;
using UnityEngine.TestTools;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace BeatSaberClone.Tests
{
    [TestFixture]
    public class AudioDataProcessorTests
    {
        private AudioDataProcessor _audioDataProcessor;
        private AudioSource _audioSource;
        private FFTResolution _fftResolution;
        private FFTWindow _fftWindow;

        [SetUp]
        public void SetUp()
        {
            var gameObject = new GameObject();
            _audioSource = gameObject.AddComponent<AudioSource>();
            _fftResolution = FFTResolution._1024;
            _fftWindow = FFTWindow.Hamming;
            _audioDataProcessor = new AudioDataProcessor(_audioSource, _fftResolution, _fftWindow);
        }

        [Test]
        public void Initialize_WhenAudioClipIsNotNull_InitializesSpectrumData()
        {
            // Arrange
            _audioSource.clip = AudioClip.Create("TestClip", 44100, 1, 44100, false);

            // Assert
            Assert.IsNotNull(_audioDataProcessor.SpectrumData);
            Assert.AreEqual((int)_fftResolution, _audioDataProcessor.SpectrumData.Length);
        }

        [Test]
        public void Initialize_WhenAudioClipIsNull_DoesNotInitializeSpectrumData()
        {
            // Arrange
            _audioSource.clip = null;

            // Assert
            Assert.IsNull(_audioDataProcessor.SpectrumData);
        }

        [Test]
        public void UpdateSpectrumData_WhenAudioIsPlaying_UpdatesSpectrumData()
        {
            // Arrange
            _audioSource.clip = AudioClip.Create("TestClip", 44100, 1, 44100, false);
            _audioSource.Play();

            // Act
            _audioDataProcessor.UpdateSpectrumData();

            // Assert
            Assert.IsNotNull(_audioDataProcessor.SpectrumData);
            Assert.IsTrue(_audioDataProcessor.SpectrumData.Length > 0);
            bool hasNonZeroValue = false;
            foreach (var value in _audioDataProcessor.SpectrumData)
            {
                if (value != 0f)
                {
                    hasNonZeroValue = true;
                    break;
                }
            }
            Assert.IsTrue(hasNonZeroValue, "SpectrumData should contain non-zero values when audio is playing.");
        }

        [Test]
        public void UpdateSpectrumData_WhenAudioIsNotPlaying_ClearsSpectrumData()
        {
            // Arrange
            _audioSource.clip = AudioClip.Create("TestClip", 44100, 1, 44100, false);
            _audioSource.Stop();

            // Act
            _audioDataProcessor.UpdateSpectrumData();

            // Assert
            foreach (var value in _audioDataProcessor.SpectrumData)
            {
                Assert.AreEqual(0f, value);
            }
        }

        [UnityTest]
        public void CalculateAverageSpectrum_WhenSpectrumDataIsInitialized_ReturnsCorrectAverage() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            _audioSource.clip = AudioClip.Create("TestClip", 44100, 1, 44100, false);
            for (int i = 0; i < _audioDataProcessor.SpectrumData.Length; i++)
            {
                _audioDataProcessor.SpectrumData[i] = 1f; // Fill with sample data
            }

            // Act
            float average = await _audioDataProcessor.CalculateAverageSpectrumAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(1f, average);
        });

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_audioSource.gameObject);
            _audioDataProcessor = null;
            _audioSource = null;
        }
    }
}
