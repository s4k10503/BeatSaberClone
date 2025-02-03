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

        [Header("AudioSouce")]
        [SerializeField] private AudioSource _trackSource;
        [SerializeField] private AudioSource _soudEffectSource;

        [Tooltip("FFT resolution must be a power of two between 64 and 8192.")]
        [SerializeField] private FFTResolution _fftResolution = FFTResolution._512;
        [SerializeField] private FFTWindow _fftWindow = FFTWindow.Triangle;

        public override void InstallBindings()
        {
            // Repository
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
                .Bind<ISoundVolumeRepository>()
                .To<JsonSoundVolumeRepository>()
                .FromNew()
                .AsSingle();

            Container
                .Bind<AudioClipList>()
                .FromInstance(_audioClipList);

            // Service
            Container
                .BindInterfacesTo<ScoreService>()
                .FromNew()
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
                .Bind<ILoggerService>()
                .To<LoggerService>()
                .AsSingle();

            Container
                .Bind<FFTResolution>()
                .FromInstance(_fftResolution);

            Container
                .Bind<FFTWindow>()
                .FromInstance(_fftWindow);

            // UseCase
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
                .BindInterfacesTo<SoundUseCase>()
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

            Container
                .Bind<AudioSource>()
                .WithId("TrackSource")
                .FromInstance(_trackSource);

            Container
                .Bind<AudioSource>()
                .WithId("SeSource")
                .FromInstance(_soudEffectSource);
        }
    }
}
