using System;

namespace MedievalCityBuilder.World
{
    /// <summary>
    /// SplitMix64 pseudo-random number generator for deterministic world generation.
    /// Does NOT use System.Random anywhere.
    /// </summary>
    public sealed class DeterministicRandom
    {
        private ulong _state;

        public DeterministicRandom(ulong seed)
        {
            _state = seed;
        }

        /// <summary>
        /// Generates the next raw 64-bit unsigned integer using SplitMix64 algorithm.
        /// </summary>
        public ulong NextUInt64()
        {
            ulong z = (_state += 0x9E3779B97F4A7C15UL);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        /// <summary>
        /// Generates a double-precision floating-point number in [0, 1) using the top 53 bits.
        /// </summary>
        public double NextDouble()
        {
            ulong value = NextUInt64();
            // Take top 53 bits
            ulong mantissa = value >> 11;
            // 53 bits of precision, divide by 2^53
            return (double)mantissa * (1.0 / 9007199254740992.0); // 1.0 / (1UL << 53)
        }

        /// <summary>
        /// Generates an integer in [minInclusive, maxExclusive) using rejection sampling to avoid modulo bias.
        /// Throws ArgumentOutOfRangeException when maxExclusive &lt;= minInclusive.
        /// </summary>
        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "maxExclusive must be greater than minInclusive.");
            }

            long range = (long)maxExclusive - minInclusive;
            if (range == 1)
            {
                return minInclusive;
            }

            // Rejection sampling to avoid modulo bias
            // We want to find a random ulong < limit where limit is the largest multiple of range that fits in ulong.
            // Or more simply: ulong maxValid = ulong.MaxValue - (ulong.MaxValue % (ulong)range);
            ulong uRange = (ulong)range;
            ulong maxValid = ulong.MaxValue - (ulong.MaxValue % uRange);

            while (true)
            {
                ulong sample = NextUInt64();
                if (sample < maxValid)
                {
                    return minInclusive + (int)(sample % uRange);
                }
            }
        }
    }
}
