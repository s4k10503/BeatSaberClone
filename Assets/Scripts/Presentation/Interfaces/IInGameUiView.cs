using System;

namespace BeatSaberClone.Presentation
{
    public interface IInGameUiView
    {
        IObservable<string> OnErrorOccurred { get; }

        void SetTotalDuration(float duration);
        void UpdateTimer(float elapsedTime);
        void DispalyScore(int score);
        void DispalyCombo(int combo);
        void DispalyComboMultiplier(float comboMultiplier);
        void UpdateComboProgress(float progress);
    }
}
