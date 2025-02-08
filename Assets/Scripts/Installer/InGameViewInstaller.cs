using Zenject;
using BeatSaberClone.Presentation;
using UnityEngine;

namespace BeatSaberClone.Installer
{
    public sealed class InGameViewInstaller : MonoInstaller
    {
        [Header("ScriptableObject")]
        [SerializeField] private AudioVisualEffectParameters _audioVisualEffectParameters;
        [SerializeField] private BoxSettings _boxSettings;

        [Header("Prefabs")]
        [SerializeField] private GameObject _redBoxPrefab;
        [SerializeField] private GameObject _blueBoxPrefab;

        public override void InstallBindings()
        {
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
                .Bind<float>()
                .WithId("BoxInitialMoveSpeed")
                .FromInstance(_boxSettings.InitialMoveSpeed);

            Container
                .Bind<float>()
                .WithId("BoxFinalMoveSpeed")
                .FromInstance(_boxSettings.FinalMoveSpeed);

            Container
                .Bind<float>()
                .WithId("BoxSlowDownDistance")
                .FromInstance(_boxSettings.SlowDownDistance);

            Container
                .Bind<float>()
                .WithId("BoxLerpSpeed")
                .FromInstance(_boxSettings.LerpSpeed);

            Container
                .Bind<float>()
                .WithId("BoxDestroyZCoordinates")
                .FromInstance(_boxSettings.DestroyZCoordinates);

            Container
                .Bind<float>()
                .WithId("BoxRotationDuration")
                .FromInstance(_boxSettings.RotationDuration);



            Container
                .Bind<float>()
                .WithId("BoxRotationDelay")
                .FromInstance(_boxSettings.RotationDelay);

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
        }
    }
}
