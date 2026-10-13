using System;
using System.Collections.Generic;
using Xunit;
using MedievalCityBuilder.World;

namespace MedievalCityBuilder.Tests
{
    public class TerrainGeneratorTests
    {
        private static WorldGenerationSettings CreateSettings(ulong seed, int generatorVersion = WorldGenerationSettings.CurrentGeneratorVersion, WorldSize size = WorldSize.Small)
        {
            return new WorldGenerationSettings(seed, generatorVersion, size);
        }

        private static TerrainSettings CreateTerrainSettings(
            int octaves = 4,
            double baseFeatureSizeCells = 64.0,
            double persistence = 0.5,
            double lacunarity = 2.0)
        {
            return new TerrainSettings(octaves, baseFeatureSizeCells, persistence, lacunarity);
        }

        [Fact]
        public void ValueNoise_OctaveSeed_RepeatableAndDistinct()
        {
            ulong seed = 12345;
            
            // Test repeatability
            for (int octave = 0; octave < 8; octave++)
            {
                ulong s1 = ValueNoise.OctaveSeed(seed, octave);
                ulong s2 = ValueNoise.OctaveSeed(seed, octave);
                Assert.Equal(s1, s2);
            }

            // Test 8 distinct values for octaves 0..7
            var seeds = new HashSet<ulong>();
            for (int octave = 0; octave < 8; octave++)
            {
                ulong s = ValueNoise.OctaveSeed(seed, octave);
                Assert.True(seeds.Add(s), $"Octave {octave} produced duplicate seed");
            }
            Assert.Equal(8, seeds.Count);

            // Test negative octave throws
            Assert.Throws<ArgumentOutOfRangeException>(() => ValueNoise.OctaveSeed(seed, -1));
        }

        [Fact]
        public void ValueNoise_OctaveSeeds_AreUncorrelated()
        {
            // For every noise seed in 0..15 (both even and odd),
            // and for ix and iz in 0..15:
            // LatticeValue(OctaveSeed(s,1), ix, iz) != LatticeValue(OctaveSeed(s,0), ix + d, iz)
            // for each d in {-1, 0, 1}
            for (ulong s = 0; s <= 15; s++)
            {
                ulong seed0 = ValueNoise.OctaveSeed(s, 0);
                ulong seed1 = ValueNoise.OctaveSeed(s, 1);

                for (int ix = 0; ix <= 15; ix++)
                {
                    for (int iz = 0; iz <= 15; iz++)
                    {
                        double v1 = ValueNoise.LatticeValue(seed1, ix, iz);

                        for (int d = -1; d <= 1; d++)
                        {
                            double v0 = ValueNoise.LatticeValue(seed0, ix + d, iz);
                            Assert.NotEqual(v1, v0);
                        }
                    }
                }
            }
        }

        [Fact]
        public void SameSettingsAndSeed_GiveIdenticalHeightmap()
        {
            var world = CreateSettings(12345);
            var terrain = CreateTerrainSettings();

            var hm1 = TerrainGenerator.Generate(world, terrain, 64, 64);
            var hm2 = TerrainGenerator.Generate(world, terrain, 64, 64);

            Assert.Equal(hm1.Width, hm2.Width);
            Assert.Equal(hm1.Height, hm2.Height);

            var data1 = hm1.GetData();
            var data2 = hm2.GetData();

            Assert.Equal(data1.Length, data2.Length);
            for (int i = 0; i < data1.Length; i++)
            {
                Assert.Equal(data1[i], data2[i]);
            }
        }

        [Fact]
        public void DifferentSeeds_GiveDifferentHeightmaps()
        {
            var world1 = CreateSettings(12345);
            var world2 = CreateSettings(54321);
            var terrain = CreateTerrainSettings();

            var hm1 = TerrainGenerator.Generate(world1, terrain, 64, 64);
            var hm2 = TerrainGenerator.Generate(world2, terrain, 64, 64);

            var data1 = hm1.GetData();
            var data2 = hm2.GetData();

            bool allSame = true;
            for (int i = 0; i < data1.Length; i++)
            {
                if (data1[i] != data2[i])
                {
                    allSame = false;
                    break;
                }
            }

            Assert.False(allSame, "Different seeds should produce different heightmaps");
        }

        [Fact]
        public void AllValuesWithinRange_AndNotConstant()
        {
            var world = CreateSettings(12345);
            var terrain = CreateTerrainSettings();

            var hm = TerrainGenerator.Generate(world, terrain, 64, 64);
            var data = hm.GetData();

            float min = float.MaxValue;
            float max = float.MinValue;
            float first = data[0];
            bool notConstant = false;

            foreach (float v in data)
            {
                Assert.True(v >= 0.0f && v <= 1.0f, $"Value {v} out of range [0, 1]");
                if (v < min) min = v;
                if (v > max) max = v;
                if (v != first) notConstant = true;
            }

            Assert.True(notConstant, "Heightmap should not be constant (at least two distinct values)");
        }

        [Fact]
        public void CellHeightDoesNotDependOnMapSize()
        {
            var world = CreateSettings(12345);
            var terrain = CreateTerrainSettings();

            var hm64 = TerrainGenerator.Generate(world, terrain, 64, 64);
            var hm96 = TerrainGenerator.Generate(world, terrain, 96, 96);

            // Check specific cells are exactly equal
            Assert.Equal(hm64.Get(0, 0), hm96.Get(0, 0));
            Assert.Equal(hm64.Get(10, 20), hm96.Get(10, 20));
            Assert.Equal(hm64.Get(63, 63), hm96.Get(63, 63));
        }

        [Fact]
        public void ChangingTerrainSettingsField_ChangesOutput()
        {
            var world = CreateSettings(12345);
            var terrainBase = CreateTerrainSettings();

            var hmBase = TerrainGenerator.Generate(world, terrainBase, 64, 64);
            var dataBase = hmBase.GetData();

            // Test changing Octaves
            var terrainOctaves = CreateTerrainSettings(octaves: terrainBase.Octaves + 1);
            var hmOctaves = TerrainGenerator.Generate(world, terrainOctaves, 64, 64);
            AssertNotEqual(dataBase, hmOctaves.GetData(), "Changing Octaves should change output");

            // Test changing BaseFeatureSizeCells
            var terrainFeatureSize = CreateTerrainSettings(baseFeatureSizeCells: terrainBase.BaseFeatureSizeCells * 2);
            var hmFeatureSize = TerrainGenerator.Generate(world, terrainFeatureSize, 64, 64);
            AssertNotEqual(dataBase, hmFeatureSize.GetData(), "Changing BaseFeatureSizeCells should change output");

            // Test changing Persistence
            var terrainPersistence = CreateTerrainSettings(persistence: terrainBase.Persistence * 2);
            var hmPersistence = TerrainGenerator.Generate(world, terrainPersistence, 64, 64);
            AssertNotEqual(dataBase, hmPersistence.GetData(), "Changing Persistence should change output");

            // Test changing Lacunarity
            var terrainLacunarity = CreateTerrainSettings(lacunarity: terrainBase.Lacunarity * 2);
            var hmLacunarity = TerrainGenerator.Generate(world, terrainLacunarity, 64, 64);
            AssertNotEqual(dataBase, hmLacunarity.GetData(), "Changing Lacunarity should change output");
        }

        private void AssertNotEqual(ReadOnlySpan<float> a, ReadOnlySpan<float> b, string message)
        {
            Assert.Equal(a.Length, b.Length);
            bool allSame = true;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    allSame = false;
                    break;
                }
            }
            Assert.False(allSame, message);
        }

        [Fact]
        public void InvalidTerrainSettings_Throw()
        {
            var world = CreateSettings(12345);

            Assert.Throws<ArgumentOutOfRangeException>(() => CreateTerrainSettings(octaves: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateTerrainSettings(baseFeatureSizeCells: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateTerrainSettings(baseFeatureSizeCells: -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateTerrainSettings(persistence: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateTerrainSettings(persistence: -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateTerrainSettings(lacunarity: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateTerrainSettings(lacunarity: -1));
        }

        [Fact]
        public void Heightmap_RejectsWidthOrHeightLessThan2()
        {
            var world = CreateSettings(12345);
            var terrain = CreateTerrainSettings();

            Assert.Throws<ArgumentOutOfRangeException>(() => TerrainGenerator.Generate(world, terrain, 1, 64));
            Assert.Throws<ArgumentOutOfRangeException>(() => TerrainGenerator.Generate(world, terrain, 64, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => TerrainGenerator.Generate(world, terrain, 0, 64));
            Assert.Throws<ArgumentOutOfRangeException>(() => TerrainGenerator.Generate(world, terrain, 64, 0));
        }

        [Fact]
        public void Heightmap_Get_ThrowsOutOfRange()
        {
            var world = CreateSettings(12345);
            var terrain = CreateTerrainSettings();
            var hm = TerrainGenerator.Generate(world, terrain, 64, 64);

            Assert.Throws<ArgumentOutOfRangeException>(() => hm.Get(-1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => hm.Get(64, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => hm.Get(0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => hm.Get(0, 64));
            Assert.Throws<ArgumentOutOfRangeException>(() => hm.Get(63, 64));
            Assert.Throws<ArgumentOutOfRangeException>(() => hm.Get(64, 63));
        }

        [Fact]
        public void ValueNoise_LatticeValue_InRange_AndRepeatable()
        {
            // Test range
            for (int ix = -100; ix <= 100; ix += 13)
            {
                for (int iz = -100; iz <= 100; iz += 17)
                {
                    double v = ValueNoise.LatticeValue(12345, ix, iz);
                    Assert.True(v >= 0.0 && v < 1.0, $"LatticeValue({ix}, {iz}) = {v} out of range [0, 1)");
                }
            }

            // Test repeatability
            for (int ix = -10; ix <= 10; ix++)
            {
                for (int iz = -10; iz <= 10; iz++)
                {
                    double v1 = ValueNoise.LatticeValue(98765, ix, iz);
                    double v2 = ValueNoise.LatticeValue(98765, ix, iz);
                    Assert.Equal(v1, v2);
                }
            }
        }

        [Fact]
        public void ValueNoise_Sample_AtIntegerCoordinates_EqualsLatticeValue()
        {
            for (int ix = -10; ix <= 10; ix++)
            {
                for (int iz = -10; iz <= 10; iz++)
                {
                    double lattice = ValueNoise.LatticeValue(55555, ix, iz);
                    double sample = ValueNoise.Sample(55555, (double)ix, (double)iz);
                    Assert.Equal(lattice, sample, 15); // High precision since they should be exactly equal
                }
            }
        }

        [Fact]
        public void GoldenTest_Seed12345_GeneratorVersion1_DefaultSettings_64x64()
        {
            // GOLDEN TEST: These values lock the current algorithm.
            // Changing them requires incrementing WorldGenerationSettings.CurrentGeneratorVersion.
            // Obtained by running the generator once with seed 12345, GeneratorVersion 1, TerrainSettings.Default, size 64x64.
            // NOTE: Algorithm changed before any save files existed; CurrentGeneratorVersion stays 1 because no saves or releases exist.
            // OLD values: 0.63614976406097412, 0.50005364418029785, 0.38636791706085205
            // NEW values: 0.26993528008460999, 0.42523997975755096, 0.24660804867744446
            
            var world = CreateSettings(12345, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Small);
            var terrain = TerrainSettings.Default;
            var hm = TerrainGenerator.Generate(world, terrain, 64, 64);

            // Expected values with tolerance 1e-9
            // NOTE: Changing these requires incrementing CurrentGeneratorVersion
            double expected_0_0 = 0.26993528008460999;
            double expected_31_31 = 0.42523997975755096;
            double expected_63_63 = 0.24660804867744446;

            Assert.Equal(expected_0_0, hm.Get(0, 0), 9);
            Assert.Equal(expected_31_31, hm.Get(31, 31), 9);
            Assert.Equal(expected_63_63, hm.Get(63, 63), 9);
        }

        [Fact]
        public void Generate_WithWorldSizeTable_Works()
        {
            var world = CreateSettings(12345, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Small);
            var terrain = CreateTerrainSettings();
            var table = new WorldSizeTable();

            // Small size = 128 cells per side => 129 vertices
            var hm = TerrainGenerator.Generate(world, terrain, table);

            Assert.Equal(129, hm.Width);
            Assert.Equal(129, hm.Height);

            // Verify it matches direct call
            var hmDirect = TerrainGenerator.Generate(world, terrain, 129, 129);
            var data1 = hm.GetData();
            var data2 = hmDirect.GetData();

            for (int i = 0; i < data1.Length; i++)
            {
                Assert.Equal(data1[i], data2[i]);
            }
        }
    }
}