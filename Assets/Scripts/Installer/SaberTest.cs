using Zenject;
using BeatSaberClone.Domain;
using BeatSaberClone.UseCase;
using BeatSaberClone.Infrastructure;
using BeatSaberClone.Presentation;

namespace BeatSaberClone.Installer
{
    public sealed class SaberTestInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Repository

            // Service
            Container
                .Bind<ILoggerService>()
                .To<LoggerService>()
                .AsSingle();

            // UseCase
            Container
                .BindInterfacesTo<ViewLogUseCase>()
                .FromNew()
                .AsSingle();

            // View
            Container
                .Bind<IParticleEffectHandler>()
                .To<ParticleEffectHandler>()
                .FromComponentInHierarchy()
                .AsSingle();

            Container
                .BindInterfacesTo<BoxView>()
                .FromNew()
                .AsSingle();

            Container
                .BindInterfacesTo<TrailGenerator>()
                .AsTransient();

            // Presenter
            Container
                .BindInterfacesTo<SaberTestPresenter>()
                .FromNew()
                .AsSingle();
        }
    }
}