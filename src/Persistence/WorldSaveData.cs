using MedievalCityBuilder.World;

namespace MedievalCityBuilder.Persistence
{
    /// <summary>
    /// The version of the on-disk save file layout.
    /// </summary>
    /// <remarks>
    /// The format version must be incremented whenever the file layout changes.
    /// When an older version is encountered, a migration must be added for that version.
    /// </remarks>
    public static class SaveFormat
    {
        /// <summary>
        /// The current save file format version. Must be >= 1.
        /// </summary>
        public const int CurrentFormatVersion = 1;
    }

    /// <summary>
    /// Immutable snapshot of the world state that can be serialized to and deserialized from a JSON string.
    /// </summary>
    /// <param name="Generation">The world generation settings (seed, generator version, size).</param>
    /// <param name="Terrain">The terrain generation settings.</param>
    /// <param name="Water">The water generation settings.</param>
    public record WorldSaveData(
        WorldGenerationSettings Generation,
        TerrainSettings Terrain,
        WaterSettings Water);
}