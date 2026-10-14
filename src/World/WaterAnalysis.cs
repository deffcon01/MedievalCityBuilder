using System;

namespace MedievalCityBuilder.World
{
    /// <summary>
    /// Static pure functions for analyzing which terrain vertices lie below the water plane.
    /// All functions are deterministic and side-effect free.
    /// </summary>
    public static class WaterAnalysis
    {
        /// <summary>
        /// Returns true when the vertex at (x, z) is strictly below the sea level.
        /// A vertex exactly equal to the sea level is NOT underwater.
        /// </summary>
        /// <param name="heightmap">Heightmap to inspect.</param>
        /// <param name="x">X coordinate in vertices.</param>
        /// <param name="z">Z coordinate in vertices.</param>
        /// <param name="water">Water settings defining the sea level.</param>
        /// <exception cref="ArgumentNullException">Thrown when heightmap or water is null.</exception>
        public static bool IsUnderwater(Heightmap heightmap, int x, int z, WaterSettings water)
        {
            if (heightmap == null)
            {
                throw new ArgumentNullException(nameof(heightmap), "Heightmap must not be null.");
            }
            if (water == null)
            {
                throw new ArgumentNullException(nameof(water), "WaterSettings must not be null.");
            }

            return heightmap.Get(x, z) < water.SeaLevel;
        }

        /// <summary>
        /// Counts the number of vertices strictly below the sea level.
        /// </summary>
        /// <param name="heightmap">Heightmap to inspect.</param>
        /// <param name="water">Water settings defining the sea level.</param>
        /// <returns>The number of underwater vertices.</returns>
        /// <exception cref="ArgumentNullException">Thrown when heightmap or water is null.</exception>
        public static int CountUnderwaterVertices(Heightmap heightmap, WaterSettings water)
        {
            if (heightmap == null)
            {
                throw new ArgumentNullException(nameof(heightmap), "Heightmap must not be null.");
            }
            if (water == null)
            {
                throw new ArgumentNullException(nameof(water), "WaterSettings must not be null.");
            }

            int count = 0;
            for (int z = 0; z < heightmap.Height; z++)
            {
                for (int x = 0; x < heightmap.Width; x++)
                {
                    if (heightmap.Get(x, z) < water.SeaLevel)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Returns the fraction of vertices strictly below the sea level, in [0, 1].
        /// </summary>
        /// <param name="heightmap">Heightmap to inspect.</param>
        /// <param name="water">Water settings defining the sea level.</param>
        /// <returns>The underwater fraction in [0, 1].</returns>
        /// <exception cref="ArgumentNullException">Thrown when heightmap or water is null.</exception>
        public static double UnderwaterFraction(Heightmap heightmap, WaterSettings water)
        {
            if (heightmap == null)
            {
                throw new ArgumentNullException(nameof(heightmap), "Heightmap must not be null.");
            }
            if (water == null)
            {
                throw new ArgumentNullException(nameof(water), "WaterSettings must not be null.");
            }

            int total = heightmap.Width * heightmap.Height;
            if (total == 0)
            {
                return 0.0;
            }

            return (double)CountUnderwaterVertices(heightmap, water) / total;
        }
    }
}