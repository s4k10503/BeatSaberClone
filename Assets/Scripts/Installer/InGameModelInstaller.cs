using Zenject;
using BeatSaberClone.Domain;
using BeatSaberClone.UseCase;
using BeatSaberClone.Infrastructure;
using UnityEngine;

namespace BeatSaberClone.Installer
{
    public sealed class InGameModelInstaller : MonoInstaller
    {
        [Header("ScriptableObject")]
        [SerializeField] private AudioClipList _audioClipList;
        [SerializeField] private HapticSettings _hapticSettings;

        [Header("AudioSource")]
        [SerializeField] private AudioSource _trackSource;
        [SerializeField] private AudioSource _soundEffectSource;

        [Header("FFT Settings")]
        [Tooltip("FFT resolution must be a power of two between 64 and 8192.")]
        [SerializeField] private FFTResolution _fftResolution = FFTResolution._512;
        [SerializeField] private FFTWindowType _fftWindowType = FFTWindowType.Triangle;

        public override void InstallBindings()
        {
            // Audio Source Wrappers
            var trackWrapper = new AudioSourceWrapper(_trackSource);
            var effectWrapper = new AudioSourceWrapper(_soundEffectSource);

            Container
                .Bind<IAudioSource>()
                .WithId("TrackSource")
                .FromInstance(trackWrapper)
                .AsCached();

            Container
                .Bind<IAudioSource>()
                .WithId("SeSource")
                .FromInstance(effectWrapper)
                .AsCached();

            // Repositories
            Container
                .Bind<IExceptionHandler>()
                .To<ExceptionHandler>()
                .FromNew()
                .AsSingle();

            Container
                .Bind<INotesRepository>()
                .To<JsonNotesRepository>()
                .FromNew()
                .AsSingle();

            Container
                .Bind<IScoreRepository>()
                .To<JsonScoreRepository>()
                .FromNew()
                .AsSingle();

            Container
                .Bind<IAudioClipRepository>()
                .To<AudioClipRepository>()
                .FromNew()
                .AsSingle();

            Container
                .Bind<IAudioVolumeRepository>()
                .To<JsonAudioVolumeRepository>()
                .FromNew()
                .AsSingle();

            Container
                .Bind<AudioClipList>()
                .FromInstance(_audioClipList);

            // Services
            Container
                .BindInterfacesTo<ScoreService>()
                .FromNew()
                .AsSingle();

            Container
                .Bind<ILoggerService>()
                .To<LoggerService>()
                .FromNew()
                .AsSingle();

            Container
                .Bind<IAudioService>()
                .To<AudioService>()
                .AsSingle();

            Container
                .Bind<IAudioDataProcessor>()
                .To<AudioDataProcessor>()
                .FromNew()
                .AsSingle();

            Container
                .Bind<IDistanceCalculator>()
                .To<DistanceCalculator>()
                .FromNew()
                .AsSingle();

            Container
                .Bind<INoteScheduler>()
                .To<NoteScheduler>()
                .FromNew()
                .AsSingle();

            Container
                .Bind<INoteSpawnService>()
                .To<NoteSpawnService>()
                .FromNew()
                .AsSingle();

            Container
                .Bind<IHapticService>()
                .To<HapticService>()
                .AsSingle();

            Container
                .Bind<FFTResolution>()
                .FromInstance(_fftResolution);

            Container
                .Bind<FFTWindowType>()
                .FromInstance(_fftWindowType);

            // Use Cases
            Container
                .BindInterfacesTo<ExceptionHandlingUseCase>()
                .FromNew()
                .AsSingle();

            Container
                .BindInterfacesTo<NotesManagementUseCase>()
                .FromNew()
                .AsSingle();

            Container
                .BindInterfacesTo<ScoreUseCase>()
                .FromNew()
                .AsSingle();

            Container
                .BindInterfacesTo<HapticFeedbackUseCase>()
                .AsTransient()
                .WithArguments(_hapticSettings.HapticDuration, _hapticSettings.HapticIntensity);

            Container
                .BindInterfacesTo<AudioUseCase>()
                .FromNew()
                .AsSingle();

            Container
                .BindInterfacesTo<AudioDataProcessingUseCase>()
                .FromNew()
                .AsSingle();

            Container
                .BindInterfacesTo<ViewLogUseCase>()
                .FromNew()
                .AsSingle();
        }
    }
}
