using System;

namespace BeatSaberClone.Domain
{
    public readonly struct Score
    {
        public int Value { get; }

        public Score(int value)
        {
            if (value < 0) throw new ArgumentException("Score cannot be negative.");
            Value = value;
        }

        public Score Add(Score other)
        {
            return new Score(Value + other.Value);
        }

        public Score CalculatePoints(float accuracy, float comboMultiplier)
        {
            if (accuracy < 0 || accuracy > 1)
                throw new DomainException("Accuracy must be between 0 and 1.");
            if (comboMultiplier < 0)
                throw new DomainException("Combo multiplier cannot be negative.");

            int basePoints = 100;
            return new Score((int)Math.Round(basePoints * accuracy * comboMultiplier));
        }

        public override bool Equals(object obj)
        {
            return obj is Score score && Value == score.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(Score left, Score right)
        {
            return left.Value == right.Value;
        }

        public static bool operator !=(Score left, Score right)
        {
            return !(left == right);
        }
    }
}
