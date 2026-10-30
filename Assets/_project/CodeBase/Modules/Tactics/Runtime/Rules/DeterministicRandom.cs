using System;

namespace PetProject.Tactics
{
    /// <summary>xorshift32-v1. Its stream is independent of Unity and presentation randomness.</summary>
    public sealed class DeterministicRandom : IRandomSource
    {
        public uint State { get; private set; }
        public DeterministicRandom(uint state)
        {
            if (state == 0) throw new ArgumentOutOfRangeException(nameof(state), "Random state must be nonzero");
            State = state;
        }

        public int NextInt(int minimumInclusive, int maximumExclusive)
        {
            if (minimumInclusive >= maximumExclusive) throw new ArgumentOutOfRangeException(nameof(maximumExclusive));
            var width = (uint)((long)maximumExclusive - minimumInclusive);
            var threshold = unchecked(0u - width) % width;
            uint value;
            do { value = NextUInt(); } while (value < threshold);
            return (int)(minimumInclusive + (long)(value % width));
        }

        private uint NextUInt()
        {
            var value = State;
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            State = value;
            return value;
        }
    }
}
