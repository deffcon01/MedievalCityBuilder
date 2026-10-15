using System;
using Xunit;
using MedievalCityBuilder.World;
using MedievalCityBuilder.Persistence;

namespace MedievalCityBuilder.Tests
{
    public class WorldSaveSerializerTests
    {
        [Fact]
        public void RoundTrip_Seed12345_DefaultSettings_EqualsOriginal()
        {
            var original = new WorldSaveData(
                new WorldGenerationSettings(12345UL, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Small),
                TerrainSettings.Default,
                WaterSettings.Default
            );

            string json = WorldSaveSerializer.Serialize(original);
            var deserialized = WorldSaveSerializer.Deserialize(json);

            Assert.Equal(original, deserialized);
        }

        [Fact]
        public void RoundTrip_SeedUlongMaxValue_EqualsOriginal()
        {
            var original = new WorldSaveData(
                new WorldGenerationSettings(ulong.MaxValue, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Large),
                TerrainSettings.Default,
                WaterSettings.Default
            );

            string json = WorldSaveSerializer.Serialize(original);
            var deserialized = WorldSaveSerializer.Deserialize(json);

            Assert.Equal(original, deserialized);
        }

        [Fact]
        public void RoundTrip_SeedZero_EqualsOriginal()
        {
            var original = new WorldSaveData(
                new WorldGenerationSettings(0UL, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Medium),
                TerrainSettings.Default,
                WaterSettings.Default
            );

            string json = WorldSaveSerializer.Serialize(original);
            var deserialized = WorldSaveSerializer.Deserialize(json);

            Assert.Equal(original, deserialized);
        }

        [Fact]
        public void RoundTrip_NonDefaultAwkwardDoubles_EqualsOriginal()
        {
            var original = new WorldSaveData(
                new WorldGenerationSettings(987654321UL, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Medium),
                new TerrainSettings(5, 0.1, 1.0 / 3.0, 1e-9),
                new WaterSettings(0.1)
            );

            string json = WorldSaveSerializer.Serialize(original);
            var deserialized = WorldSaveSerializer.Deserialize(json);

            Assert.Equal(original, deserialized);
        }

        [Fact]
        public void Determinism_SerializeTwice_GivesIdenticalStrings()
        {
            var data = new WorldSaveData(
                new WorldGenerationSettings(12345UL, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Small),
                TerrainSettings.Default,
                WaterSettings.Default
            );

            string json1 = WorldSaveSerializer.Serialize(data);
            string json2 = WorldSaveSerializer.Serialize(data);

            Assert.Equal(json1, json2);
        }

        [Fact]
        public void Output_ContainsFormatVersionSeedAndWorldSize()
        {
            var data = new WorldSaveData(
                new WorldGenerationSettings(12345UL, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Medium),
                TerrainSettings.Default,
                WaterSettings.Default
            );

            string json = WorldSaveSerializer.Serialize(data);

            Assert.Contains("formatVersion", json);
            Assert.Contains("\"12345\"", json);
            Assert.Contains("Medium", json);
        }

        [Fact]
        public void Reproducibility_HeightmapMatchesAfterRoundTrip()
        {
            var generation = new WorldGenerationSettings(12345UL, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Small);
            var terrain = TerrainSettings.Default;
            int width = 64;
            int height = 64;

            var originalHeightmap = TerrainGenerator.Generate(generation, terrain, width, height);

            var savedata = new WorldSaveData(generation, terrain, WaterSettings.Default);
            string json = WorldSaveSerializer.Serialize(savedata);
            var loaded = WorldSaveSerializer.Deserialize(json);

            var loadedHeightmap = TerrainGenerator.Generate(loaded.Generation, loaded.Terrain, width, height);

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

        [Theory]
        [InlineData("{\"formatVersion\": 0, \"generatorVersion\": 1, \"seed\": \"12345\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}")]
        [InlineData("{\"formatVersion\": 999, \"generatorVersion\": 1, \"seed\": \"12345\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 99, \"seed\": \"12345\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"abc\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"-1\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"12345\", \"worldSize\": \"Huge\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"12345\", \"worldSize\": \"1\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"12345\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 2.0}}")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"12345\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 0, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("{}")]
        [InlineData("not json")]
        public void Deserialize_InvalidInput_ThrowsSaveFormatException(string json)
        {
            Assert.Throws<SaveFormatException>(() => WorldSaveSerializer.Deserialize(json));
        }

        [Fact]
        public void Deserialize_NullInput_ThrowsSaveFormatException()
        {
            Assert.Throws<SaveFormatException>(() => WorldSaveSerializer.Deserialize(null!));
        }

        [Fact]
        public void Deserialize_UnknownExtraField_IsIgnoredAndLoadsSuccessfully()
        {
            string json = "{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"12345\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}, \"unknownExtraField\": 999}";

            var data = WorldSaveSerializer.Deserialize(json);
            Assert.NotNull(data);
            Assert.Equal(12345UL, data.Generation.Seed);
            Assert.Equal(WorldSize.Small, data.Generation.Size);
        }
    }
}
