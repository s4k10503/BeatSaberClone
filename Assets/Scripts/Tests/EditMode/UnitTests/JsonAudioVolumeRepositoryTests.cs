using NUnit.Framework;
using BeatSaberClone.Infrastructure;
using System.Threading;
using UnityEngine.TestTools;
using System.Collections;
using Cysharp.Threading.Tasks;
using System.IO;
using System;

namespace BeatSaberClone.Tests
{
    [TestFixture]
    public sealed class JsonAudioVolumeRepositoryTests
    {
        private JsonAudioVolumeRepository _repository;
        private CancellationTokenSource _cts;
        private string _testFilePath;
        private string _backupFilePath;
        private bool _originalFileExists;

        [SetUp]
        public void Setup()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), "BeatSaberClone", "SoundSettings.json");
            _backupFilePath = _testFilePath + ".backup";
            Directory.CreateDirectory(Path.GetDirectoryName(_testFilePath));

            // Back up the original file
            _originalFileExists = File.Exists(_testFilePath);
            if (_originalFileExists)
            {
                File.Copy(_testFilePath, _backupFilePath, true);
                File.Delete(_testFilePath);
            }

            _repository = new JsonAudioVolumeRepository(_testFilePath);
            _cts = new CancellationTokenSource();
        }

        [TearDown]
        public void TearDown()
        {
            _cts.Dispose();
            _cts = null;
            _repository.Dispose();
            _repository = null;

            // Delete the test file
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }

            // Restore original files
            if (_originalFileExists)
            {
                File.Copy(_backupFilePath, _testFilePath, true);
                File.Delete(_backupFilePath);
            }

            Directory.Delete(Path.GetDirectoryName(_testFilePath), true);
        }

        [UnityTest]
        public IEnumerator SaveAudioSettings_WithInvalidVolumeBGM_ShouldThrowException() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            float invalidVolumeBGM = -0.1f;
            float validVolumeSE = 0.5f;
            InfrastructureException actualException = null;

            // Act
            try
            {
                await _repository.SaveAudioSettingsAsync(invalidVolumeBGM, validVolumeSE, _cts.Token);
            }
            catch (InfrastructureException ex)
            {
                actualException = ex;
            }

            // Assert
            Assert.That(actualException, Is.Not.Null);
            Assert.That(actualException.Message, Is.EqualTo($"Invalid BGM volume value: {invalidVolumeBGM}. Must be between 0 and 1."));
        });

        [UnityTest]
        public IEnumerator SaveAudioSettings_WithInvalidVolumeSE_ShouldThrowException() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            float validVolumeBGM = 0.5f;
            float invalidVolumeSE = 1.1f;
            InfrastructureException actualException = null;

            // Act
            try
            {
                await _repository.SaveAudioSettingsAsync(validVolumeBGM, invalidVolumeSE, _cts.Token);
            }
            catch (InfrastructureException ex)
            {
                actualException = ex;
            }

            // Assert
            Assert.That(actualException, Is.Not.Null);
            Assert.That(actualException.Message, Is.EqualTo($"Invalid SE volume value: {invalidVolumeSE}. Must be between 0 and 1."));
        });

        [UnityTest]
        public IEnumerator LoadAudioSettings_WhenFileDoesNotExist_ShouldReturnDefaultValues() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            Assert.IsFalse(File.Exists(_testFilePath), "Test file should not exist before test");

            // Act
            var (volumeBGM, volumeSE) = await _repository.LoadAudioSettingsAsync(_cts.Token);

            // Assert
            Assert.AreEqual(0.5f, volumeBGM);
            Assert.AreEqual(0.5f, volumeSE);
            Assert.IsTrue(File.Exists(_testFilePath), "Default settings should be saved to file");
        });

        [UnityTest]
        public IEnumerator SaveAndLoadAudioSettings_WithValidValues_ShouldWork() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            float expectedVolumeBGM = 0.7f;
            float expectedVolumeSE = 0.3f;

            // Act
            await _repository.SaveAudioSettingsAsync(expectedVolumeBGM, expectedVolumeSE, _cts.Token);
            var (actualVolumeBGM, actualVolumeSE) = await _repository.LoadAudioSettingsAsync(_cts.Token);

            // Assert
            Assert.AreEqual(expectedVolumeBGM, actualVolumeBGM);
            Assert.AreEqual(expectedVolumeSE, actualVolumeSE);
        });
    }
}
