using UniRx;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;
using BeatSaberClone.UseCase;

namespace BeatSaberClone.Presentation
{
    public sealed class SaberTestPresenter : IInitializable, ITickable, ILateTickable
    {
        // UseCases
        private readonly IViewLogUseCase _viewLogUseCase;

        // View
        private readonly IObjectSlicer _objectSlicerR;

        // Others
        private CompositeDisposable _disposables;
        private CancellationTokenSource _cts;

        [Inject]
        public SaberTestPresenter(
            IViewLogUseCase viewLogUseCase,
            [Inject(Id = "RightSlicer")] IObjectSlicer objectSlicerR)
        {
            _viewLogUseCase = viewLogUseCase;
            _objectSlicerR = objectSlicerR;

            _disposables = new CompositeDisposable();
            _cts = new CancellationTokenSource();
        }

        void IInitializable.Initialize()
        {
            _objectSlicerR.InitializeAsync(_cts.Token).Forget();
            SubscribeToSlicer(_objectSlicerR);
        }

        void ITickable.Tick()
        {
        }

        void ILateTickable.LateTick()
        {
            _objectSlicerR.UpdateTrailAsync(_cts.Token);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void SubscribeToSlicer(IObjectSlicer slicer)
        {
            slicer.HitObject
                .Where(hit => hit != null)
                .Subscribe(hit =>
                {
                    var boxView = hit.GetComponent<BoxView>();
                    if (boxView != null && !boxView.IsSliced)
                    {
                        // Asynchronous slice processing
                        boxView.Sliced(
                            slicer.TipTransform.position,
                            slicer.PlaneNormal,
                            slicer.CrossSectionMaterial,
                            slicer.CutForce,
                            _cts.Token).Forget();
                    }
                })
                .AddTo(_disposables);
        }
    }
}
