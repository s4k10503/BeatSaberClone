using System;

namespace BeatSaberClone.Domain
{
    public sealed class Combo
    {
        public int Value { get; private set; }
        public int MaxCombo { get; private set; }
        public float Multiplier => GetMultiplier();

        public Combo(int value, int maxCombo)
        {
            if (value < 0) throw new ArgumentException("Combo cannot be negative.");
            Value = value;
            MaxCombo = maxCombo;
        }

        public Combo Add()
        {
            var newCombo = Value + 1;
            return new Combo(newCombo, Math.Max(newCombo, MaxCombo));
        }

        public Combo Reset()
        {
            return new Combo(0, MaxCombo);
        }

        private float GetMultiplier()
        {
            if (Value == 0) return 1f;
            if (Value == 1) return 1f;
            if (Value >= 2 && Value <= 5) return 2f;
            if (Value >= 6 && Value <= 13) return 4f;
            if (Value >= 14) return 8f;
            return 1f;
        }
    }
}
