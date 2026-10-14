using System;
using Xunit;
using MedievalCityBuilder.World;

namespace MedievalCityBuilder.Tests
{
    public class WaterTests
    {
        private static Heightmap CreateHandBuiltHeightmap4x4()
        {
            // 4x4 heightmap with known values for testing SeaLevel = 0.5
            // Row 0: 0.1, 0.3, 0.5, 0.7
            // Row 1: 0.2, 0.4, 0.6, 0.8
            // Row 2: 0.3, 0.5, 0.7, 0.9
            // Row 3: 0.4, 0.6, 0.8, 1.0
            var hm = new Heightmap(4, 4);
            float[,] values = new float[,]
            {
                { 0.1f, 0.3f, 0.5f, 0.7f },
                { 0.2f, 0.4f, 0.6f, 0.8f },
                { 0.3f, 0.5f, 0.7f, 0.9f },
                { 0.4f, 0.6f, 0.8f, 1.0f }
            };

            for (int z = 0; z < 4; z++)
            {
                for (int x = 0; x < 4; x++)
                {
                    hm.Set(x, z, values[z, x]);
                }
            }

            return hm;
        }

        // ---- WaterSettings tests ----

        [Fact]
        public void WaterSettings_NaN_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaterSettings(double.NaN));
        }

        [Fact]
        public void WaterSettings_Negative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaterSettings(-0.01));
        }

        [Fact]
        public void WaterSettings_GreaterThanOne_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaterSettings(1.01));
        }

        [Fact]
        public void WaterSettings_Zero_Accepted()
        {
            var ws = new WaterSettings(0.0);
            Assert.Equal(0.0, ws.SeaLevel);
        }

        [Fact]
        public void WaterSettings_One_Accepted()
        {
            var ws = new WaterSettings(1.0);
            Assert.Equal(1.0, ws.SeaLevel);
        }

        [Fact]
        public void WaterSettings_EqualRecords_AreEqual()
        {
            var ws1 = new WaterSettings(0.35);
            var ws2 = new WaterSettings(0.35);
            Assert.Equal(ws1, ws2);
        }

        [Fact]
        public void WaterSettings_DefaultSeaLevel_Is035()
        {
            Assert.Equal(0.35, WaterSettings.Default.SeaLevel);
        }

        // ---- WaterAnalysis tests ----

        [Fact]
        public void IsUnderwater_StrictlyLessThanSeaLevel()
        {
            var hm = CreateHandBuiltHeightmap4x4();
            var water = new WaterSettings(0.5);

            // Vertex at (2, 0) has value 0.5 - exactly at sea level, NOT underwater
            Assert.False(WaterAnalysis.IsUnderwater(hm, 2, 0, water));

            // Vertex at (0, 0) has value 0.1 - below sea level, IS underwater
            Assert.True(WaterAnalysis.IsUnderwater(hm, 0, 0, water));

            // Vertex at (3, 3) has value 1.0 - above sea level, NOT underwater
            Assert.False(WaterAnalysis.IsUnderwater(hm, 3, 3, water));
        }

        [Fact]
        public void CountUnderwaterVertices_HandBuilt4x4_SeaLevel05()
        {
            var hm = CreateHandBuiltHeightmap4x4();
            var water = new WaterSettings(0.5);

            // Values < 0.5: 0.1, 0.2, 0.3, 0.3, 0.4, 0.4 = 6 values
            // (0,0)=0.1, (0,1)=0.2, (0,2)=0.3, (1,0)=0.3, (1,1)=0.4, (0,3)=0.4
            int count = WaterAnalysis.CountUnderwaterVertices(hm, water);
            Assert.Equal(6, count);
        }

        [Fact]
        public void UnderwaterFraction_HandBuilt4x4_SeaLevel05()
        {
            var hm = CreateHandBuiltHeightmap4x4();
            var water = new WaterSettings(0.5);

            // 6 underwater out of 16 vertices = 0.375
            double fraction = WaterAnalysis.UnderwaterFraction(hm, water);
            Assert.Equal(6.0 / 16.0, fraction, 9);
        }

        [Fact]
        public void SeaLevelZero_GivesZeroUnderwater()
        {
            var hm = CreateHandBuiltHeightmap4x4();
            var water = new WaterSettings(0.0);

            int count = WaterAnalysis.CountUnderwaterVertices(hm, water);
            double fraction = WaterAnalysis.UnderwaterFraction(hm, water);

            Assert.Equal(0, count);
            Assert.Equal(0.0, fraction);
        }

        [Fact]
        public void SeaLevelOne_AllVerticesBelowOneUnderwater()
        {
            var hm = CreateHandBuiltHeightmap4x4();
            var water = new WaterSettings(1.0);

            // Vertex at (3, 3) has value 1.0 - exactly at sea level, NOT underwater (strict <)
            // So 15 out of 16 vertices are underwater
            int count = WaterAnalysis.CountUnderwaterVertices(hm, water);
            double fraction = WaterAnalysis.UnderwaterFraction(hm, water);

            Assert.Equal(15, count);
            Assert.Equal(15.0 / 16.0, fraction, 9);
        }

        [Fact]
        public void FractionEqualsCountDividedByWidthTimesHeight()
        {
            var hm = CreateHandBuiltHeightmap4x4();
            var water = new WaterSettings(0.5);

            int count = WaterAnalysis.CountUnderwaterVertices(hm, water);
            double fraction = WaterAnalysis.UnderwaterFraction(hm, water);

            int total = hm.Width * hm.Height;
            Assert.Equal((double)count / total, fraction);
        }

        [Fact]
        public void IsUnderwater_NullHeightmap_Throws()
        {
            var water = new WaterSettings(0.5);
            Assert.Throws<ArgumentNullException>(() => WaterAnalysis.IsUnderwater(null!, 0, 0, water));
        }

        [Fact]
        public void IsUnderwater_NullWater_Throws()
        {
            var hm = CreateHandBuiltHeightmap4x4();
            Assert.Throws<ArgumentNullException>(() => WaterAnalysis.IsUnderwater(hm, 0, 0, null!));
        }

        [Fact]
        public void CountUnderwaterVertices_NullHeightmap_Throws()
        {
            var water = new WaterSettings(0.5);
            Assert.Throws<ArgumentNullException>(() => WaterAnalysis.CountUnderwaterVertices(null!, water));
        }

        [Fact]
        public void CountUnderwaterVertices_NullWater_Throws()
        {
            var hm = CreateHandBuiltHeightmap4x4();
            Assert.Throws<ArgumentNullException>(() => WaterAnalysis.CountUnderwaterVertices(hm, null!));
        }

        [Fact]
        public void UnderwaterFraction_NullHeightmap_Throws()
        {
            var water = new WaterSettings(0.5);
            Assert.Throws<ArgumentNullException>(() => WaterAnalysis.UnderwaterFraction(null!, water));
        }

        [Fact]
        public void UnderwaterFraction_NullWater_Throws()
        {
            var hm = CreateHandBuiltHeightmap4x4();
            Assert.Throws<ArgumentNullException>(() => WaterAnalysis.UnderwaterFraction(hm, null!));
        }

        [Fact]
        public void GeneratedHeightmap_SeaLevel035_FractionInRangeAndDeterministic()
        {
            var world = new WorldGenerationSettings(12345, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Small);
            var terrain = TerrainSettings.Default;
            var hm = TerrainGenerator.Generate(world, terrain, 64, 64);

            var water = new WaterSettings(0.35);

            double fraction1 = WaterAnalysis.UnderwaterFraction(hm, water);
            double fraction2 = WaterAnalysis.UnderwaterFraction(hm, water);

            // Fraction must be in [0, 1]
            Assert.True(fraction1 >= 0.0 && fraction1 <= 1.0, $"Fraction {fraction1} out of range [0, 1]");

            // Must be identical across two calls (deterministic)
            Assert.Equal(fraction1, fraction2);
        }
    }
}