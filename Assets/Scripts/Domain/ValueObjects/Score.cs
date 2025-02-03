using System;

namespace BeatSaberClone.Domain
{
    public sealed class Score
    {
        public int Value { get; private set; }

        public Score(int value)
        {
            if (value < 0) throw new ArgumentException("Score cannot be negative.");
            Value = value;
        }

        public Score Add(int points)
        {
            return new Score(Value + points);
        }
    }
}
