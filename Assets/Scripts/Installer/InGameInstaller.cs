using Zenject;
using BeatSaberClone.Domain;
using BeatSaberClone.UseCase;
using BeatSaberClone.Infrastructure;
using BeatSaberClone.Presentation;
using UnityEngine;

namespace BeatSaberClone.Installer
{
    public class EntryPointInstaller : MonoInstaller
    {
        [SerializeField] private AudioSource _trackSource;
        [SerializeField] private AudioSource _soudEffectSource;
        [SerializeField] private AudioClipList _audioClipList;
        [SerializeField] private AudioVisualEffectParameters _audioVisualEffectParameters;

        [Tooltip("FFT resolution must be a power of two between 64 and 8192.")]
        [SerializeField] private FFTResolution _fftResolution = FFTResolution._512;
        [SerializeField] private FFTWindow _fftWindow = FFTWindow.Triangle;

        [SerializeField] private BoxMoveSpeed _boxMoveSpeed;
        [SerializeField] private GameObject _redBoxPrefab;
        [SerializeField] private GameObject _blueBoxPrefab;

        // Speed thresholds for detecting the start and end of swing
        [SerializeField] private float _hapticDuration = 0.1f;
        [SerializeField] private float _hapticIntensity = 0.5f;

        public override void InstallBindings()
        {
            // Repository
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

            // UseCase
            Container
                .BindInterfacesTo<NotesManagementUseCase>()
                .FromNew()
                .AsSingle();

            Container
                .BindInterfacesTo<ScoreUseCase>()
                .FromNew()
                .AsSingle();

            Container
                .BindInterfacesTo<HapticFeedback>()
                .AsTransient()
                .WithArguments(_hapticDuration, _hapticIntensity);

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

            // View
            Container
                .Bind<IInGameUiView>()
                .To<InGameUiView>()
                .FromComponentInHierarchy()
                .AsSingle();

            Container
                .Bind<IBoxSpawner>()
                .To<BoxSpawner>()
                .FromComponentInHierarchy()
                .AsSingle();

            Container
                .BindFactory<BoxView, BoxView.Factory>()
                .WithId("RedBox")
                .FromComponentInNewPrefab(_redBoxPrefab)
                .AsTransient();

            Container
                .BindFactory<BoxView, BoxView.Factory>()
                .WithId("BlueBox")
                .FromComponentInNewPrefab(_blueBoxPrefab)
                .AsTransient();

            Container
                .Bind<CustomBoxViewFactory>()
                .AsSingle();

            Container
                .Bind<IAudioVisualEffecter>()
                .To<AudioVisualEffecter>()
                .FromComponentInHierarchy()
                .AsSingle();

            Container
                .Bind<IMovementAnimationController>()
                .To<MovementAnimationController>()
                .FromNew()
                .AsTransient();

            Container
                .Bind<IParticleEffectHandler>()
                .To<ParticleEffectHandler>()
                .FromComponentInHierarchy()
                .AsSingle();

            Container
                .Bind<ISlicedObject>()
                .To<SlicedObject>()
                .FromNew()
                .AsSingle();

            Container
                .BindInterfacesTo<TrailGenerator>()
                .AsTransient();

            // Presenter
            Container
                .BindInterfacesTo<InGamePresenter>()
                .AsSingle()
                .NonLazy();

            // InGamePrsenter
            Container
                .Bind<float>()
                .WithId("BoxMoveSpeed")
                .FromInstance(_boxMoveSpeed.Speed);

            // SoundUseCase
            Container
                .Bind<AudioSource>()
                .WithId("Track")
                .FromInstance(_trackSource);

            Container
                .Bind<AudioSource>()
                .WithId("Se")
                .FromInstance(_soudEffectSource);

            // AudioClipRepository
            Container
                .Bind<AudioClipList>()
                .FromInstance(_audioClipList);

            // AudioVisualEffecter
            Container
                .Bind<Color>()
                .WithId("BaseFogColor")
                .FromInstance(_audioVisualEffectParameters.BaseFogColor);

            Container
                .Bind<Color>()
                .WithId("TargetFogColor")
                .FromInstance(_audioVisualEffectParameters.TargetFogColor);

            Container
                .Bind<float>()
                .WithId("IntensityScale")
                .FromInstance(_audioVisualEffectParameters.IntensityScale);

            Container
                .Bind<float>()
                .WithId("ScaleMultiplier")
                .FromInstance(_audioVisualEffectParameters.ScaleMultiplier);

            Container
                .Bind<float>()
                .WithId("LerpSpeed")
                .FromInstance(_audioVisualEffectParameters.LerpSpeed);

            // AudioDataProcessor
            Container
                .Bind<FFTResolution>()
                .FromInstance(_fftResolution);

            Container
                .Bind<FFTWindow>()
                .FromInstance(_fftWindow);
        }
    }
}