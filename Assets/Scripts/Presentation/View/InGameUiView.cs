using UnityEngine;
using TMPro;
using UniRx;
using System;
using UnityEngine.UI;
using DG.Tweening;

namespace BeatSaberClone.Presentation
{
    public sealed class InGameUiView : MonoBehaviour, IInGameUiView
    {
        [Header("TM Pro")]
        [SerializeField] private TextMeshProUGUI _timerTMP;
        [SerializeField] private TextMeshProUGUI _scoreTMP;
        [SerializeField] private TextMeshProUGUI _comboTMP;
        [SerializeField] private TextMeshProUGUI _comboMultiplierTMP;

        [Header("Images")]
        [SerializeField] private Image _progressBar;

        private float _totalDuration;

        private readonly Subject<string> _onErrorOccurred = new();
        public IObservable<string> OnErrorOccurred => _onErrorOccurred.AsObservable();

        private void OnDestroy()
        {
            _timerTMP = null;
            _scoreTMP = null;
            _comboTMP = null;
            _comboMultiplierTMP = null;
            _onErrorOccurred.Dispose();
        }

        // Set total playback time
        public void SetTotalDuration(float duration)
        {
            if (duration < 0)
            {
                LogError("Total duration cannot be negative.");
                return;
            }

            _totalDuration = duration;
        }

        // Display elapsed time (and total playback time)
        public void UpdateTimer(float elapsedTime)
        {
            if (elapsedTime < 0)
            {
                LogError("Elapsed time cannot be negative.");
                return;
            }

            if (_timerTMP == null)
            {
                LogError("_timerTMP is not assigned.");
                return;
            }

            // Elapsed time in minutes:seconds format
            var elapsedMinutes = Mathf.FloorToInt(elapsedTime / 60);
            var elapsedSeconds = Mathf.FloorToInt(elapsedTime % 60);

            // Total playback time in minutes:seconds format
            var totalMinutes = Mathf.FloorToInt(_totalDuration / 60);
            var totalSeconds = Mathf.FloorToInt(_totalDuration % 60);

            // Update text: "Elapsed time / Total playback time"
            _timerTMP.text = $"{elapsedMinutes:00}:{elapsedSeconds:00} / {totalMinutes:00}:{totalSeconds:00}";
        }

        public void DispalyScore(int score)
        {
            if (_scoreTMP == null)
            {
                LogError("_scoreTMP is not assigned.");
                return;
            }

            _scoreTMP.text = $"{score}";
        }

        public void DispalyCombo(int combo)
        {
            if (_comboTMP == null)
            {
                LogError("_comboTMP is not assigned.");
                return;
            }

            _comboTMP.text = $"COMBO\n{combo}";
        }

        public void DispalyComboMultiplier(float comboMultiplier)
        {
            if (_comboMultiplierTMP == null)
            {
                LogError("_comboMultiplierTMP is not assigned.");
                return;
            }

            _comboMultiplierTMP.text = $"x{comboMultiplier}";
        }

        public void UpdateComboProgress(float progress)
        {
            if (_progressBar != null)
            {
                // CLAMP for Progress from 0 to 1
                float targetProgress = Mathf.Clamp01(progress);

                //Turn 0.5 seconds tween to TargetProgress
                _progressBar.DOFillAmount(targetProgress, 0.25f).SetEase(Ease.OutCubic);
            }
            else
            {
                LogError("_progressBar is not assigned.");
            }
        }

        private void LogError(string message)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            _onErrorOccurred.OnNext(message);
#endif
        }
    }
}
