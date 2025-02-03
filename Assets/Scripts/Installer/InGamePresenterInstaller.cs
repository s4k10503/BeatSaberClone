using Zenject;
using BeatSaberClone.Presentation;


namespace BeatSaberClone.Installer
{
    public sealed class InGamePresenterInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Presenter
            Container
                .BindInterfacesTo<InGamePresenter>()
                .AsSingle()
                .NonLazy();
        }
    }
}
