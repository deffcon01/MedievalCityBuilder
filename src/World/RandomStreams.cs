using System;
using System.Text;

namespace MedievalCityBuilder.World
{
    /// <summary>
    /// Static factory for deriving independent deterministic random streams from a world seed.
    /// Each stream is identified by a name and must produce the same sequence regardless of derivation order.
    /// </summary>
    public static class RandomStreams
    {
        /// <summary>
        /// Stream name for terrain generation.
        /// </summary>
        public const string Terrain = "terrain";

        /// <summary>
        /// Stream name for water generation.
        /// </summary>
        public const string Water = "water";

        /// <summary>
        /// Stream name for climate generation.
        /// </summary>
        public const string Climate = "climate";

        /// <summary>
        /// Stream name for biome/environment generation.
        /// </summary>
        public const string Biome = "biome";

        /// <summary>
        /// Stream name for renewable resources generation.
        /// </summary>
        public const string RenewableResources = "renewable_resources";

        /// <summary>
        /// Stream name for finite deposits generation.
        /// </summary>
        public const string FiniteDeposits = "finite_deposits";

        /// <summary>
        /// Stream name for starting location validation.
        /// </summary>
        public const string StartingLocation = "starting_location";

        /// <summary>
        /// Stream name for starting settlement generation.
        /// </summary>
        public const string StartingSettlement = "starting_settlement";

        /// <summary>
        /// Derives a new DeterministicRandom stream from the world seed and stream name.
        /// The stream seed is derived using a stable 64-bit FNV-1a hash of the UTF-8 bytes of streamName,
        /// mixed with SplitMix64 to ensure good distribution and order independence.
        /// </summary>
        /// <param name="worldSeed">The master world seed.</param>
        /// <param name="streamName">The name of the stream (use the constants defined in this class).</param>
        /// <returns>A new DeterministicRandom instance for this stream.</returns>
        public static DeterministicRandom Derive(ulong worldSeed, string streamName)
        {
            // FNV-1a 64-bit hash of streamName UTF-8 bytes
            ulong hash = 0xCBF29CE484222325UL; // FNV offset basis
            byte[] bytes = Encoding.UTF8.GetBytes(streamName);
            foreach (byte b in bytes)
            {
                hash ^= b;
                hash *= 0x100000001B3UL; // FNV prime
            }

            // Mix the world seed with the hash using SplitMix64-style mixing
            // This ensures order independence: same (worldSeed, streamName) always gives same result
            ulong mixed = worldSeed ^ hash;
            mixed = (mixed ^ (mixed >> 30)) * 0xBF58476D1CE4E5B9UL;
            mixed = (mixed ^ (mixed >> 27)) * 0x94D049BB133111EBUL;
            mixed ^= mixed >> 31;

            return new DeterministicRandom(mixed);
        }
    }
}