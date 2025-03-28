using UnityEngine;
using TMPro;
using UniRx;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Pool;

namespace BeatSaberClone.Presentation
{
    public sealed class InGameUiView : MonoBehaviour, IInGameUiView
    {
        [Header("TM Pro")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private TextMeshProUGUI _timerTMP;
        [SerializeField] private TextMeshProUGUI _scoreTMP;
        [SerializeField] private TextMeshProUGUI _comboTMP;
        [SerializeField] private TextMeshProUGUI _comboMultiplierTMP;
        [SerializeField] private TextMeshProUGUI _sliceScorePrefab;
        [SerializeField] private int _sliceScorePoolSize = 10;
        [SerializeField] private float _sliceScoreDisplayDuration = 1.0f;
        [SerializeField] private float _sliceScoreForwardOffset = 5f;
        [SerializeField] private float _sliceScoreUpwardOffset = 0.5f;

        [Header("Images")]
        [SerializeField] private Image _progressBar;

        private float _totalDuration;
        private ObjectPool<TextMeshProUGUI> _sliceScorePool;

        private Subject<string> _onErrorOccurred = new Subject<string>();
        public IObservable<string> OnErrorOccurred => _onErrorOccurred.AsObservable();

        private void Awake()
        {
            InitializeSliceScorePool();
        }

        private void OnDestroy()
        {
            _canvas = null;
            _timerTMP = null;
            _scoreTMP = null;
            _comboTMP = null;
            _comboMultiplierTMP = null;
            _progressBar = null;
            _sliceScorePrefab = null;
            _sliceScorePool = null;
            _onErrorOccurred?.Dispose();
            _onErrorOccurred = null;
        }

        private void InitializeSliceScorePool()
        {
            _sliceScorePool = new ObjectPool<TextMeshProUGUI>(
                createFunc: CreateSliceScore,
                actionOnGet: OnGetSliceScore,
                actionOnRelease: OnReleaseSliceScore,
                actionOnDestroy: OnDestroySliceScore,
                collectionCheck: true,
                defaultCapacity: _sliceScorePoolSize,
                maxSize: _sliceScorePoolSize * 2
            );
        }

        // Generate TextMeshProUGUI for Slice Score
        private TextMeshProUGUI CreateSliceScore()
        {
            var scoreText = Instantiate(_sliceScorePrefab, _canvas.transform);
            scoreText.gameObject.SetActive(false);
            return scoreText;
        }

        // Processing when retrieving from a pool
        private void OnGetSliceScore(TextMeshProUGUI scoreText)
        {
            scoreText.gameObject.SetActive(true);
        }

        // Processing when returning to the pool
        private void OnReleaseSliceScore(TextMeshProUGUI scoreText)
        {
            scoreText.gameObject.SetActive(false);
        }

        // Processing when object pool limit exceeds or when discarded
        private void OnDestroySliceScore(TextMeshProUGUI scoreText)
        {
            Destroy(scoreText.gameObject);
            scoreText = null;
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

            var elapsedMinutes = Mathf.FloorToInt(elapsedTime / 60);
            var elapsedSeconds = Mathf.FloorToInt(elapsedTime % 60);
            var totalMinutes = Mathf.FloorToInt(_totalDuration / 60);
            var totalSeconds = Mathf.FloorToInt(_totalDuration % 60);

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
                float targetProgress = Mathf.Clamp01(progress);
                _progressBar.DOFillAmount(targetProgress, 0.25f).SetEase(Ease.OutCubic);
            }
            else
            {
                LogError("_progressBar is not assigned.");
            }
        }

        public void DisplaySliceScore(Vector3 displayPosition, int score)
        {
            if (_sliceScorePool == null)
            {
                LogError("Slice score pool is not initialized.");
                return;
            }

            // Retrieved from the object pool
            var sliceScoreText = _sliceScorePool.Get();

            // Adjust the display position in world space
            var tmpPosition = displayPosition + Vector3.forward * _sliceScoreForwardOffset;
            sliceScoreText.transform.position = tmpPosition;
            sliceScoreText.text = score == 0 ? "MISS" : $"{score}";
            sliceScoreText.alpha = 1f;

            // Rise and fade out with Tween
            sliceScoreText.transform.DOMove(tmpPosition + Vector3.up * _sliceScoreUpwardOffset, _sliceScoreDisplayDuration)
                .SetEase(Ease.OutQuad);
            sliceScoreText.DOFade(0f, _sliceScoreDisplayDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    // Return to the pool
                    _sliceScorePool.Release(sliceScoreText);
                });
        }

        private void LogError(string message)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            _onErrorOccurred.OnNext(message);
#endif
        }
    }
}
