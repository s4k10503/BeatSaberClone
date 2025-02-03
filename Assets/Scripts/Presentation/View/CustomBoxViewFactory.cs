using Zenject;

namespace BeatSaberClone.Presentation
{
    public sealed class CustomBoxViewFactory : IFactory<int, float, float, BoxView>
    {
        private readonly BoxView.Factory _redBoxFactory;
        private readonly BoxView.Factory _blueBoxFactory;

        public CustomBoxViewFactory(
            [Inject(Id = "RedBox")] BoxView.Factory redBoxFactory,
            [Inject(Id = "BlueBox")] BoxView.Factory blueBoxFactory)
        {
            _redBoxFactory = redBoxFactory;
            _blueBoxFactory = blueBoxFactory;
        }

        public BoxView Create(int type, float speed, float originalY)
        {
            var boxView = type == 0
                ? _redBoxFactory.Create()
                : _blueBoxFactory.Create();

            boxView.SetParameters(type, speed, originalY);
            return boxView;
        }
    }
}
