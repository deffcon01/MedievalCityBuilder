using System;

namespace MedievalCityBuilder.World
{
    /// <summary>
    /// Immutable settings for world generation. Contains seed, generator version, and world size.
    /// </summary>
    public record WorldGenerationSettings
    {
        /// <summary>
        /// Current generator version. Must be >= 1.
        /// </summary>
        public const int CurrentGeneratorVersion = 1;

        /// <summary>
        /// The world seed for deterministic generation.
        /// </summary>
        public ulong Seed { get; init; }

        /// <summary>
        /// The generator version. Must be >= 1.
        /// </summary>
        public int GeneratorVersion { get; init; }

        /// <summary>
        /// The world size category.
        /// </summary>
        public WorldSize Size { get; init; }

        /// <summary>
        /// Creates a new WorldGenerationSettings instance.
        /// </summary>
        /// <param name="seed">The world seed.</param>
        /// <param name="generatorVersion">The generator version (must be >= 1).</param>
        /// <param name="size">The world size.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when generatorVersion < 1.</exception>
        public WorldGenerationSettings(ulong seed, int generatorVersion, WorldSize size)
        {
            if (generatorVersion < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(generatorVersion), "GeneratorVersion must be >= 1.");
            }

            Seed = seed;
            GeneratorVersion = generatorVersion;
            Size = size;
        }
    }
}