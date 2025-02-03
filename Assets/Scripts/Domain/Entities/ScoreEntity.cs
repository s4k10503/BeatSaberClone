using System;
using UniRx;

namespace BeatSaberClone.Domain
{
    public sealed class ScoreEntity
    {
        public Guid Id { get; }
        public ReactiveProperty<int> Score { get; }
        public ReactiveProperty<int> Combo { get; }
        public ReactiveProperty<int> MaxCombo { get; }
        public IReadOnlyReactiveProperty<float> ComboMultiplier { get; }

        public ScoreEntity(Guid id, int initialScore, int initialCombo, int maxCombo = 0)
        {
            Id = id != Guid.Empty ? id : Guid.NewGuid();
            Score = new ReactiveProperty<int>(initialScore);
            Combo = new ReactiveProperty<int>(initialCombo);
            MaxCombo = new ReactiveProperty<int>(maxCombo);
            ComboMultiplier = Combo.Select(CalculateComboMultiplier).ToReactiveProperty();
        }

        public void AddScore(float multiplier)
        {
            if (multiplier < 0)
            {
                throw new DomainException("Multiplier must be positive.");
            }

            Score.Value += (int)(100 * multiplier * ComboMultiplier.Value);
        }

        public void AddCombo()
        {
            Combo.Value++;
            MaxCombo.Value = Math.Max(MaxCombo.Value, Combo.Value);
        }

        public void ResetCombo()
        {
            Combo.Value = 0;
        }

        private float CalculateComboMultiplier(int comboValue)
        {
            if (comboValue == 0) return 1f;
            if (comboValue == 1) return 1f;
            if (comboValue >= 2 && comboValue <= 5) return 2f;
            if (comboValue >= 6 && comboValue <= 13) return 4f;
            if (comboValue >= 14) return 8f;
            return 1f;
        }

        public override bool Equals(object obj)
        {
            if (obj is not ScoreEntity other) return false;
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
