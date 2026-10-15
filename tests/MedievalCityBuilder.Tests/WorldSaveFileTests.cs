using System;
using System.IO;
using System.Text;
using Xunit;
using MedievalCityBuilder.Persistence;
using MedievalCityBuilder.World;

namespace MedievalCityBuilder.Tests
{
    public class WorldSaveFileTests : IDisposable
    {
        private readonly string _testDir;
        private bool _disposed = false;

        public WorldSaveFileTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "mcb_tests_" + Guid.NewGuid());
            Directory.CreateDirectory(_testDir);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    if (Directory.Exists(_testDir))
                    {
                        Directory.Delete(_testDir, true);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
                _disposed = true;
            }
        }

        private string GetTestPath(string filename)
        {
            return Path.Combine(_testDir, filename);
        }

        [Fact]
        public void SaveLoad_RoundTrip_Seed12345_EqualsOriginal()
        {
            var data = new WorldSaveData(
                new WorldGenerationSettings(12345UL, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Small),
                TerrainSettings.Default,
                WaterSettings.Default
            );

            string path = GetTestPath("world1.json");
            WorldSaveFile.Save(path, data);
            var loaded = WorldSaveFile.Load(path);

            Assert.Equal(data, loaded);
        }

        [Fact]
        public void SaveLoad_RoundTrip_SeedUlongMaxValue_EqualsOriginal()
        {
            var data = new WorldSaveData(
                new WorldGenerationSettings(ulong.MaxValue, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Large),
                TerrainSettings.Default,
                WaterSettings.Default
            );

            string path = GetTestPath("world2.json");
            WorldSaveFile.Save(path, data);
            var loaded = WorldSaveFile.Load(path);

            Assert.Equal(data, loaded);
        }

        [Fact]
        public void Save_CreatesMissingDirectory()
        {
            var data = new WorldSaveData(
                new WorldGenerationSettings(12345UL, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Small),
                TerrainSettings.Default,
                WaterSettings.Default
            );

            string subDir = Path.Combine(_testDir, "subdir");
            string path = Path.Combine(subDir, "world.json");

            WorldSaveFile.Save(path, data);

            Assert.True(File.Exists(path));
        }

        [Fact]
        public void Save_TwiceSamePath_OverwritesAndNoTmpRemains()
        {
            var data1 = new WorldSaveData(
                new WorldGenerationSettings(111UL, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Small),
                TerrainSettings.Default,
                WaterSettings.Default
            );
            var data2 = new WorldSaveData(
                new WorldGenerationSettings(222UL, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Medium),
                TerrainSettings.Default,
                WaterSettings.Default
            );

            string path = GetTestPath("overwrite.json");

            WorldSaveFile.Save(path, data1);
            WorldSaveFile.Save(path, data2);

            var loaded = WorldSaveFile.Load(path);
            Assert.Equal(data2, loaded);

            // No .tmp file should remain
            string tmpPath = path + ".tmp";
            Assert.False(File.Exists(tmpPath), "Temporary file should not remain after successful save");
        }

        [Fact]
        public void Save_WrittenFileHasNoUtf8Bom()
        {
            var data = new WorldSaveData(
                new WorldGenerationSettings(12345UL, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Small),
                TerrainSettings.Default,
                WaterSettings.Default
            );

            string path = GetTestPath("nobom.json");
            WorldSaveFile.Save(path, data);

            byte[] bytes = File.ReadAllBytes(path);
            Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF,
                "File should not start with UTF-8 BOM (EF BB BF)");
        }

        [Fact]
        public void Save_FailedSave_PreservesOriginalFile()
        {
            var originalData = new WorldSaveData(
                new WorldGenerationSettings(999UL, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Small),
                TerrainSettings.Default,
                WaterSettings.Default
            );

            string path = GetTestPath("preserve.json");
            WorldSaveFile.Save(path, originalData);

            // Create a directory at the .tmp location to force save failure
            string tmpPath = path + ".tmp";
            Directory.CreateDirectory(tmpPath);

            var newData = new WorldSaveData(
                new WorldGenerationSettings(888UL, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Small),
                TerrainSettings.Default,
                WaterSettings.Default
            );

            // This should throw because we can't write to the .tmp (it's a directory)
            // Windows throws UnauthorizedAccessException when a file is opened on a directory path
            var ex = Assert.ThrowsAny<Exception>(() => WorldSaveFile.Save(path, newData));
            Assert.True(ex is IOException || ex is UnauthorizedAccessException,
                "Expected IOException or UnauthorizedAccessException, got " + ex.GetType().Name);
            Assert.False(ex is SaveFormatException || ex is ArgumentException,
                "Should not throw SaveFormatException or ArgumentException");

            // Original file should still exist and be loadable with original data
            Assert.True(File.Exists(path), "Original file should still exist");
            var loaded = WorldSaveFile.Load(path);
            Assert.Equal(originalData, loaded);
        }

        [Fact]
        public void Load_MissingFile_ThrowsFileNotFoundException()
        {
            string path = GetTestPath("missing.json");
            Assert.Throws<FileNotFoundException>(() => WorldSaveFile.Load(path));
        }

        [Fact]
        public void Load_InvalidJson_ThrowsSaveFormatException()
        {
            string path = GetTestPath("invalid.json");
            File.WriteAllText(path, "not json");

            Assert.Throws<SaveFormatException>(() => WorldSaveFile.Load(path));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void Save_NullEmptyWhitespacePath_ThrowsArgumentException(string? badPath)
        {
            var data = new WorldSaveData(
                new WorldGenerationSettings(12345UL, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Small),
                TerrainSettings.Default,
                WaterSettings.Default
            );

            Assert.Throws<ArgumentException>(() => WorldSaveFile.Save(badPath, data));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void Load_NullEmptyWhitespacePath_ThrowsArgumentException(string? badPath)
        {
            Assert.Throws<ArgumentException>(() => WorldSaveFile.Load(badPath));
        }

        [Fact]
        public void Reproducibility_SaveLoadRegenerate_HeightmapMatchesExactly()
        {
            var generation = new WorldGenerationSettings(12345UL, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Small);
            var terrain = TerrainSettings.Default;
            int width = 64;
            int height = 64;

            // Generate original heightmap
            var originalHeightmap = TerrainGenerator.Generate(generation, terrain, width, height);

            // Save and load
            var savedata = new WorldSaveData(generation, terrain, WaterSettings.Default);
            string path = GetTestPath("repro.json");
            WorldSaveFile.Save(path, savedata);
            var loaded = WorldSaveFile.Load(path);

            // Regenerate from loaded data
            var loadedHeightmap = TerrainGenerator.Generate(loaded.Generation, loaded.Terrain, width, height);

            // Compare every cell
            Assert.Equal(originalHeightmap.Width, loadedHeightmap.Width);
            Assert.Equal(originalHeightmap.Height, loadedHeightmap.Height);

            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    Assert.Equal(originalHeightmap.Get(x, z), loadedHeightmap.Get(x, z));
                }
            }
        }
    }
}