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

        [Header("Pool Settings")]
        [SerializeField] private int _initialPoolSize = 20;
        [SerializeField] private int _maxPoolSize = 50;

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

            // Memory Pools
            Container.BindMemoryPool<BoxView, BoxView.BoxPool>()
                .WithId("LeftBox")
                .WithInitialSize(_initialPoolSize)
                .WithMaxSize(_maxPoolSize)
                .FromComponentInNewPrefab(_leftBoxPrefab);

            Container.BindMemoryPool<BoxView, BoxView.BoxPool>()
                .WithId("RightBox")
                .WithInitialSize(_initialPoolSize)
                .WithMaxSize(_maxPoolSize)
                .FromComponentInNewPrefab(_rightBoxPrefab);

            // Effects
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
