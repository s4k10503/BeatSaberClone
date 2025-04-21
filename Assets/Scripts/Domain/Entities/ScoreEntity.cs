using System;
using System.Collections.Generic;
using UniRx;

namespace BeatSaberClone.Domain
{
    public sealed class ScoreEntity : IDisposable
    {
        private readonly Guid _id;
        private Score _score;
        private readonly ReactiveProperty<int> _value;
        private readonly ReactiveProperty<float> _accuracy;
        private readonly ReactiveProperty<int> _maxValue;
        private Combo _combo;
        private readonly List<IDisposable> _disposables = new List<IDisposable>(6); // Pre-allocate capacity
        private readonly ReactiveProperty<int> _comboCount;
        private readonly ReactiveProperty<float> _comboProgress;
        private readonly ReactiveProperty<float> _comboMultiplier;
        private float _cachedComboMultiplier;

        public Guid Id => _id;
        public IReadOnlyReactiveProperty<int> Value => _value;
        public IReadOnlyReactiveProperty<float> Accuracy => _accuracy;
        public IReadOnlyReactiveProperty<int> MaxValue => _maxValue;
        public IReadOnlyReactiveProperty<int> ComboCount => _comboCount;
        public IReadOnlyReactiveProperty<float> ComboProgress => _comboProgress;
        public IReadOnlyReactiveProperty<float> ComboMultiplier => _comboMultiplier;

        public ScoreEntity(Guid id, int initialScore, int initialCombo, float initialAccuracy, int maxValue)
        {
            _id = id;
            _score = new Score(initialScore);
            _combo = new Combo(initialCombo, maxValue);

            _value = new ReactiveProperty<int>(initialScore);
            _accuracy = new ReactiveProperty<float>(initialAccuracy);
            _maxValue = new ReactiveProperty<int>(maxValue);
            _comboCount = new ReactiveProperty<int>(initialCombo);
            _comboProgress = new ReactiveProperty<float>(_combo.Progress);
            _comboMultiplier = new ReactiveProperty<float>(_combo.Multiplier);
            _cachedComboMultiplier = _combo.Multiplier;

            AddDisposables();
        }

        public ScoreEntity() : this(Guid.NewGuid(), 0, 0, 1.0f, 0)
        {
        }

        public event Action OnComboReset;

        private void AddDisposables()
        {
            _disposables.Add(_value);
            _disposables.Add(_accuracy);
            _disposables.Add(_maxValue);
            _disposables.Add(_comboCount);
            _disposables.Add(_comboProgress);
            _disposables.Add(_comboMultiplier);
        }

        private void UpdateComboProperties()
        {
            _comboCount.Value = _combo.Value;
            _comboProgress.Value = _combo.Progress;
            _cachedComboMultiplier = _combo.Multiplier;
            _comboMultiplier.Value = _cachedComboMultiplier;
            _maxValue.Value = _combo.MaxValue;
        }

        public void AddScore(Score amount)
        {
            _score = _score.Add(amount);
            _value.Value = _score.Value;
        }

        public void UpdateAccuracy(float accuracy)
        {
            if (accuracy < 0 || accuracy > 1)
            {
                throw new DomainException("Accuracy must be between 0 and 1");
            }

            _accuracy.Value = accuracy;

            if (accuracy == 0)
            {
                ResetCombo();
            }
            else
            {
                var points = _score.CalculatePoints(accuracy, _cachedComboMultiplier);
                AddScore(points);
            }
        }

        public void AddCombo()
        {
            _combo = _combo.Increment();
            UpdateComboProperties();
        }

        public void ResetCombo()
        {
            _combo = _combo.Reset();
            UpdateComboProperties();
            OnComboReset?.Invoke();
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            _disposables.Clear();
        }
    }
}
