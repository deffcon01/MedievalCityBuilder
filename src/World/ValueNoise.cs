using System;

namespace MedievalCityBuilder.World
{
    /// <summary>
    /// Pure static functions for value noise generation.
    /// Uses SplitMix64-style integer mixing for deterministic lattice values.
    /// No state, no ordering dependence.
    /// </summary>
    public static class ValueNoise
    {
        // SplitMix64 constants for mixing
        private const ulong SPLITMIX64_CONSTANT1 = 0x9E3779B97F4A7C15UL;
        private const ulong SPLITMIX64_CONSTANT2 = 0xBF58476D1CE4E5B9UL;
        private const ulong SPLITMIX64_CONSTANT3 = 0x94D049BB133111EBUL;

        /// <summary>
        /// Generates a deterministic lattice value in [0, 1) for the given integer coordinates.
        /// Uses SplitMix64-style mixing of noiseSeed with ix and iz.
        /// No state, no ordering dependence - same inputs always produce same output.
        /// </summary>
        /// <param name="noiseSeed">The noise seed for this octave.</param>
        /// <param name="ix">Integer X lattice coordinate.</param>
        /// <param name="iz">Integer Z lattice coordinate.</param>
        /// <returns>A double in [0, 1).</returns>
        public static double LatticeValue(ulong noiseSeed, int ix, int iz)
        {
            // Mix the seed with coordinates using SplitMix64-style mixing
            // Use a simple but effective approach: mix seed with ix and iz
            ulong mixed = noiseSeed;
            mixed ^= (ulong)(ix & 0xFFFFFFFF) + 0x9E3779B97F4A7C15UL;
            mixed = (mixed ^ (mixed >> 30)) * SPLITMIX64_CONSTANT2;
            mixed = (mixed ^ (mixed >> 27)) * SPLITMIX64_CONSTANT3;
            mixed ^= mixed >> 31;

            mixed ^= (ulong)(iz & 0xFFFFFFFF) + 0x9E3779B97F4A7C15UL;
            mixed = (mixed ^ (mixed >> 30)) * SPLITMIX64_CONSTANT2;
            mixed = (mixed ^ (mixed >> 27)) * SPLITMIX64_CONSTANT3;
            mixed ^= mixed >> 31;

            // Take top 53 bits and convert to [0, 1)
            ulong mantissa = mixed >> 11;
            return (double)mantissa * (1.0 / 9007199254740992.0);
        }

        /// <summary>
        /// Smoothstep function: t * t * (3 - 2 * t) for t in [0, 1].
        /// </summary>
        private static double Smoothstep(double t)
        {
            return t * t * (3.0 - 2.0 * t);
        }

        /// <summary>
        /// Samples the value noise at continuous coordinates using bilinear interpolation
        /// with smoothstep weights.
        /// </summary>
        /// <param name="noiseSeed">The noise seed for this octave.</param>
        /// <param name="x">Continuous X coordinate.</param>
        /// <param name="z">Continuous Z coordinate.</param>
        /// <returns>Interpolated noise value in [0, 1).</returns>
        public static double Sample(ulong noiseSeed, double x, double z)
        {
            // Find the integer lattice coordinates
            int ix = (int)Math.Floor(x);
            int iz = (int)Math.Floor(z);

            // Compute fractional parts
            double fx = x - ix;
            double fz = z - iz;

            // Apply smoothstep to fractional parts
            double sx = Smoothstep(fx);
            double sz = Smoothstep(fz);

            // Get the four corner lattice values
            double v00 = LatticeValue(noiseSeed, ix, iz);
            double v10 = LatticeValue(noiseSeed, ix + 1, iz);
            double v01 = LatticeValue(noiseSeed, ix, iz + 1);
            double v11 = LatticeValue(noiseSeed, ix + 1, iz + 1);

            // Bilinear interpolation with smoothstep weights
            double v0 = v00 * (1.0 - sx) + v10 * sx;
            double v1 = v01 * (1.0 - sx) + v11 * sx;

            return v0 * (1.0 - sz) + v1 * sz;
        }
    }
}