using NUnit.Framework;
using UnityEngine;
using NSubstitute;
using System.Threading;
using System;
using BeatSaberClone.Domain;
using BeatSaberClone.Infrastructure;

namespace BeatSaberClone.Tests
{
    [TestFixture]
    public sealed class AudioServiceTests
    {
        private AudioService _audioService;
        private IAudioSource _mockTrackSource;
        private IAudioSource _mockSESource;
        private AudioClip _dummyClip;
        private AudioAsset _audioAsset;
        private CancellationTokenSource _cts;

        [SetUp]
        public void Setup()
        {
            _mockTrackSource = Substitute.For<IAudioSource>();
            _mockSESource = Substitute.For<IAudioSource>();

            // AudioClip is sealed, so you actually create it instead of Substitute.For<AudioClip>()
            _dummyClip = AudioClip.Create("dummy", 1, 1, 44100, false);

            // Since AudioAsset is a domain object, create an instance directly
            _audioAsset = new AudioAsset("dummyId", 3.0f, _dummyClip);

            _audioService = new AudioService(_mockTrackSource, _mockSESource);
            _cts = new CancellationTokenSource();
        }

        [TearDown]
        public void TearDown()
        {
            _cts.Dispose();
            _cts = null;
            _mockTrackSource = null;
            _mockSESource = null;
            _dummyClip = null;
            _audioAsset = null;
            _audioService = null;
        }

        [Test]
        public void PlayTrack_WithNullAsset_ShouldThrowException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => _audioService.PlayTrack(null));
            Assert.That(ex.ParamName, Is.EqualTo("asset"));
        }

        [Test]
        public void PlayTrack_WithValidAsset_ShouldCallSetClipAndPlay()
        {
            _audioService.PlayTrack(_audioAsset);

            // SetClip が呼ばれたことを検証
            _mockTrackSource.Received(1).SetClip(_audioAsset);
            // その後 Play() が呼ばれたことを検証
            _mockTrackSource.Received(1).Play();
        }

        [Test]
        public void PlayEffect_WithNullAsset_ShouldThrowException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => _audioService.PlayEffect(null));
            Assert.That(ex.ParamName, Is.EqualTo("asset"));
        }

        [Test]
        public void PlayEffect_WithValidAsset_ShouldCallPlayOneShot()
        {
            _audioService.PlayEffect(_audioAsset);

            // IAudioSource の PlayOneShot が呼ばれたことを検証
            _mockSESource.Received(1).PlayOneShot(_audioAsset);
        }

        [Test]
        public void UpdateSettings_WithInvalidBGMVolume_ShouldThrowException()
        {
            float invalidVolume = -0.1f;
            var ex = Assert.Throws<ArgumentException>(() =>
                _audioService.UpdateSettings(invalidVolume, 0.5f));
            Assert.That(ex.Message, Contains.Substring("Track volume must be between 0 and 1"));
        }

        [Test]
        public void UpdateSettings_WithInvalidSEVolume_ShouldThrowException()
        {
            float invalidVolume = 1.1f;
            var ex = Assert.Throws<ArgumentException>(() =>
                _audioService.UpdateSettings(0.5f, invalidVolume));
            Assert.That(ex.Message, Contains.Substring("Effects volume must be between 0 and 1"));
        }

        [Test]
        public void UpdateSettings_WithValidValues_ShouldSetSourceVolumes()
        {
            float bgmVolume = 0.7f;
            float seVolume = 0.3f;

            _audioService.UpdateSettings(bgmVolume, seVolume);

            _mockTrackSource.Received(1).Volume = bgmVolume;
            _mockSESource.Received(1).Volume = seVolume;
        }

        [Test]
        public void PauseTrack_ShouldCallPauseOnTrackSource()
        {
            _audioService.PauseTrack();
            _mockTrackSource.Received(1).Pause();
        }

        [Test]
        public void ResumeTrack_ShouldCallUnPauseOnTrackSource()
        {
            _audioService.ResumeTrack();
            _mockTrackSource.Received(1).UnPause();
        }
    }
}
