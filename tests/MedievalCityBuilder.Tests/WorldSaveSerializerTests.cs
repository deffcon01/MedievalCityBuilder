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

        [Theory]
        [InlineData("{\"generatorVersion\": 1, \"seed\": \"12345\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}", "formatVersion")]
        [InlineData("{\"formatVersion\": 1, \"seed\": \"12345\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}", "generatorVersion")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}", "seed")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"12345\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}", "worldSize")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"12345\", \"worldSize\": \"Small\", \"water\": {\"seaLevel\": 0.35}}", "terrain")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"12345\", \"worldSize\": \"Small\", \"terrain\": {\"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}", "octaves")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"12345\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}", "baseFeatureSizeCells")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"12345\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}", "persistence")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"12345\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5}, \"water\": {\"seaLevel\": 0.35}}", "lacunarity")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"12345\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}}", "water")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"12345\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {}}", "seaLevel")]
        public void Deserialize_MissingField_ThrowsSaveFormatExceptionWithFieldName(string json, string expectedField)
        {
            var exception = Assert.Throws<SaveFormatException>(() => WorldSaveSerializer.Deserialize(json));
            Assert.Contains(expectedField, exception.Message);
        }

        [Theory]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \" 5\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"5 \", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"+5\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}")]
        [InlineData("{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"18446744073709551616\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}")]
        public void Deserialize_InvalidSeed_ThrowsSaveFormatException(string json)
        {
            Assert.Throws<SaveFormatException>(() => WorldSaveSerializer.Deserialize(json));
        }

        [Fact]
        public void Deserialize_SeedUlongMaxValue_LoadsSuccessfully()
        {
            string json = "{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"18446744073709551615\", \"worldSize\": \"Small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}";
            var data = WorldSaveSerializer.Deserialize(json);
            Assert.Equal(ulong.MaxValue, data.Generation.Seed);
        }

        [Fact]
        public void Deserialize_WorldSizeSmallLowercase_ThrowsSaveFormatException()
        {
            string json = "{\"formatVersion\": 1, \"generatorVersion\": 1, \"seed\": \"12345\", \"worldSize\": \"small\", \"terrain\": {\"octaves\": 4, \"baseFeatureSizeCells\": 64, \"persistence\": 0.5, \"lacunarity\": 2}, \"water\": {\"seaLevel\": 0.35}}";
            Assert.Throws<SaveFormatException>(() => WorldSaveSerializer.Deserialize(json));
        }

        [Fact]
        public void CultureTest_RoundTripWithAwkwardDoubles()
        {
            var original = new WorldSaveData(
                new WorldGenerationSettings(987654321UL, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Medium),
                new TerrainSettings(5, 0.1, 1.0 / 3.0, 1e-9),
                new WaterSettings(0.1)
            );

            var originalCulture = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
                string json = WorldSaveSerializer.Serialize(original);
                
                // Assert no commas (excluding property names/values if any, but the JSON format uses dot for decimals)
                // The issue might be if other things have commas, but in this specific JSON it should be only decimals.
                // Actually, wait, the JSON might have commas between fields.
                // Let's assert that there is no *decimal* comma.
                Assert.DoesNotContain("0,1", json);
                Assert.DoesNotContain("1,0", json);
                Assert.DoesNotContain("0,3", json);
                Assert.Contains("0.1", json);

                var deserialized = WorldSaveSerializer.Deserialize(json);
                Assert.Equal(original, deserialized);
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = originalCulture;
            }
        }
    }
}
