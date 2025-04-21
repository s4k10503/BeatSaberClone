using System;

namespace BeatSaberClone.Domain
{
    public readonly struct Combo
    {
        private const int ComboTier1Start = 2;
        private const int ComboTier1End = 5;
        private const int ComboTier2Start = 6;
        private const int ComboTier2End = 13;
        private const int ComboTier3Start = 14;
        private const int ComboTier3Max = 21;

        public static readonly Combo Zero = new(0, 0);

        public int Value { get; }
        public int MaxValue { get; }
        public float Multiplier => GetMultiplier();
        public float Progress => CalculateProgress();

        public Combo(int value, int maxValue)
        {
            if (value < 0) throw new DomainException("Combo cannot be negative.");
            Value = value;
            MaxValue = maxValue;
        }

        public Combo Increment()
        {
            return new Combo(Value + 1, Math.Max(Value + 1, MaxValue));
        }

        public Combo Reset()
        {
            return new Combo(0, MaxValue);
        }

        private float GetMultiplier()
        {
            if (Value == 0) return 1f;
            if (Value == 1) return 1f;
            if (Value >= ComboTier1Start && Value <= ComboTier1End) return 2f;
            if (Value >= ComboTier2Start && Value <= ComboTier2End) return 4f;
            if (Value >= ComboTier3Start) return 8f;
            return 1f;
        }

        private float CalculateProgress()
        {
            if (Value == 0) return 0f;
            if (Value == 1) return 0.5f;
            if (Value >= ComboTier1Start && Value <= ComboTier1End)
                return (Value - ComboTier1Start) / (float)(ComboTier1End - ComboTier1Start);
            if (Value >= ComboTier2Start && Value <= ComboTier2End)
                return (Value - ComboTier2Start) / (float)(ComboTier2End - ComboTier2Start);
            if (Value >= ComboTier3Start)
                return Math.Min(1f, (Value - ComboTier3Start) / (float)(ComboTier3Max - ComboTier3Start));
            return 0f;
        }
    }
}
