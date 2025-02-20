using Zenject;
using BeatSaberClone.Domain;
using BeatSaberClone.Presentation;
using UnityEngine;

namespace BeatSaberClone.Installer
{
    public sealed class InGameViewInstaller : MonoInstaller
    {
        [Header("ScriptableObject")]
        [SerializeField] private AudioVisualEffectParameters _audioVisualEffectParameters;

        [Header("Prefabs")]
        [SerializeField] private GameObject _leftBoxPrefab;
        [SerializeField] private GameObject _rightBoxPrefab;

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
                .Bind<CustomBoxViewPool>()
                .AsSingle();

            Container.BindMemoryPool<BoxView, BoxView.BoxPool>()
                .WithId("LeftBox")
                .WithInitialSize(10)
                .FromComponentInNewPrefab(_leftBoxPrefab);


            Container.BindMemoryPool<BoxView, BoxView.BoxPool>()
                .WithId("RightBox")
                .WithInitialSize(10)
                .FromComponentInNewPrefab(_rightBoxPrefab);

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
                .BindInterfacesTo<TrailGenerator>()
                .AsTransient();
        }
    }
}
