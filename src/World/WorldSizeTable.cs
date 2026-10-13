using System;
using System.Collections.Generic;

namespace MedievalCityBuilder.World
{
    /// <summary>
    /// Maps WorldSize to grid cell count per side.
    /// All values are PLACEHOLDERS pending performance testing (DESIGN.md section 2).
    /// Made an instance with default values so it can be configured later without code changes.
    /// </summary>
    public sealed class WorldSizeTable
    {
        private readonly Dictionary<WorldSize, int> _cellsPerSide;

        /// <summary>
        /// Creates a new WorldSizeTable with default placeholder values.
        /// </summary>
        public WorldSizeTable()
        {
            // PLACEHOLDER: Exact dimensions determined by performance testing (DESIGN.md section 2)
            _cellsPerSide = new Dictionary<WorldSize, int>
            {
                { WorldSize.Small, 128 },    // PLACEHOLDER
                { WorldSize.Medium, 256 },   // PLACEHOLDER
                { WorldSize.Large, 512 }     // PLACEHOLDER
            };
        }

        /// <summary>
        /// Creates a new WorldSizeTable with custom values.
        /// </summary>
        public WorldSizeTable(Dictionary<WorldSize, int> cellsPerSide)
        {
            if (cellsPerSide == null)
            {
                throw new ArgumentNullException(nameof(cellsPerSide));
            }

            // Validate all WorldSize values are present
            foreach (WorldSize size in Enum.GetValues(typeof(WorldSize)))
            {
                if (!cellsPerSide.ContainsKey(size))
                {
                    throw new ArgumentException($"Missing entry for WorldSize.{size}", nameof(cellsPerSide));
                }
            }

            _cellsPerSide = new Dictionary<WorldSize, int>(cellsPerSide);
        }

        /// <summary>
        /// Gets the grid cell count per side for the given world size.
        /// </summary>
        public int GetCellsPerSide(WorldSize size)
        {
            return _cellsPerSide[size];
        }

        /// <summary>
        /// Gets all size entries.
        /// </summary>
        public IReadOnlyDictionary<WorldSize, int> GetAll()
        {
            return _cellsPerSide;
        }
    }
}