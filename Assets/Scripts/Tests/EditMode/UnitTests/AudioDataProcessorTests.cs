using NUnit.Framework;
using UnityEngine;
using BeatSaberClone.Domain;
using BeatSaberClone.Infrastructure;
using System.Linq;
using NSubstitute;

namespace BeatSaberClone.Tests
{
    [TestFixture]
    public sealed class AudioDataProcessorTests
    {
        private AudioDataProcessor _audioDataProcessor;
        private IAudioSource _audioSource;
        private FFTResolution _fftResolution;
        private FFTWindowType _fftWindow;

        [SetUp]
        public void SetUp()
        {
            _audioSource = Substitute.For<IAudioSource>();
            _fftResolution = FFTResolution._1024;
            _fftWindow = FFTWindowType.Hamming;
            _audioDataProcessor = new AudioDataProcessor(_audioSource, _fftResolution, _fftWindow);
        }

        [TearDown]
        public void TearDown()
        {
            _audioDataProcessor = null;
            _audioSource = null;
        }

        [Test]
        public void Initialize_WhenAudioClipIsNotNull_InitializesSpectrumData()
        {
            // Arrange
            _audioSource.Samples.Returns(44100);

            // Assert
            Assert.IsNotNull(_audioDataProcessor.SpectrumData);
            Assert.AreEqual((int)_fftResolution, _audioDataProcessor.SpectrumData.Length);
        }

        [Test]
        public void Initialize_WhenAudioClipIsNull_DoesNotInitializeSpectrumData()
        {
            // Arrange
            _audioSource.Samples.Returns(0);

            // Assert
            Assert.IsNotNull(_audioDataProcessor.SpectrumData);
            Assert.IsTrue(_audioDataProcessor.SpectrumData.All(x => x == 0f));
        }

        [Test]
        public void UpdateSpectrumData_WhenAudioIsPlaying_UpdatesSpectrumData()
        {
            // Arrange
            _audioSource.IsPlaying.Returns(true);
            _audioSource.TimeSamples.Returns(0);
            _audioSource.Samples.Returns(44100);
            _audioSource.When(x => x.GetSpectrumData(Arg.Any<float[]>(), Arg.Any<int>(), Arg.Any<FFTWindowType>()))
                .Do(x =>
                {
                    var samples = x.Arg<float[]>();
                    for (int i = 0; i < samples.Length; i++)
                    {
                        samples[i] = 1.0f;
                    }
                });

            // Act
            _audioDataProcessor.UpdateSpectrumData();

            // Assert
            Assert.IsNotNull(_audioDataProcessor.SpectrumData);
            Assert.IsTrue(_audioDataProcessor.SpectrumData.Length > 0);
            Assert.IsTrue(_audioDataProcessor.SpectrumData.All(x => x == 1.0f));
        }

        [Test]
        public void UpdateSpectrumData_WhenAudioIsNotPlaying_ClearsSpectrumData()
        {
            // Arrange
            _audioSource.IsPlaying.Returns(false);
            _audioSource.Samples.Returns(44100);

            // Act
            _audioDataProcessor.UpdateSpectrumData();

            // Assert
            Assert.IsTrue(_audioDataProcessor.SpectrumData.All(x => x == 0f));
        }

        [Test]
        public void CalculateAverageSpectrum_WhenSpectrumDataIsInitialized_ReturnsCorrectAverage()
        {
            // Arrange
            for (int i = 0; i < _audioDataProcessor.SpectrumData.Length; i++)
            {
                _audioDataProcessor.SpectrumData[i] = 1f;
            }

            // Act
            float average = _audioDataProcessor.CalculateAverageSpectrum();

            // Assert
            Assert.AreEqual(1f, average);
        }
    }
}
