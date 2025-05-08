using Zenject;

namespace BeatSaberClone.Presentation
{
    public sealed class CustomBoxViewPool : IFactory<SpawnSettings, BoxView>
    {
        private readonly BoxView.BoxPool _leftBoxPool;
        private readonly BoxView.BoxPool _rightBoxPool;

        [Inject]
        public CustomBoxViewPool(
            [Inject(Id = "LeftBox")] BoxView.BoxPool leftBoxPool,
            [Inject(Id = "RightBox")] BoxView.BoxPool rightBoxPool)
        {
            _leftBoxPool = leftBoxPool;
            _rightBoxPool = rightBoxPool;
        }

        public BoxView Create(SpawnSettings spawnSettings)
        {
            var boxView = spawnSettings.Type == 0
                ? _leftBoxPool.Spawn(spawnSettings)
                : _rightBoxPool.Spawn(spawnSettings);
            return boxView;
        }
    }
}
