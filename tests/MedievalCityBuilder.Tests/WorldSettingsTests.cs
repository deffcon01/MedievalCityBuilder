using System;
using System.Collections.Generic;
using Xunit;
using MedievalCityBuilder.World;

namespace MedievalCityBuilder.Tests
{
    public class RandomStreamsTests
    {
        [Fact]
        public void SameSeedAndName_GivesSameSequence()
        {
            var r1 = RandomStreams.Derive(12345, RandomStreams.Terrain);
            var r2 = RandomStreams.Derive(12345, RandomStreams.Terrain);

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(r1.NextUInt64(), r2.NextUInt64());
            }
        }

        [Fact]
        public void DifferentNames_GiveDifferentSequences()
        {
            var r1 = RandomStreams.Derive(12345, RandomStreams.Terrain);
            var r2 = RandomStreams.Derive(12345, RandomStreams.Water);

            bool allSame = true;
            for (int i = 0; i < 100; i++)
            {
                if (r1.NextUInt64() != r2.NextUInt64())
                {
                    allSame = false;
                    break;
                }
            }
            Assert.False(allSame);
        }

        [Fact]
        public void DerivationOrderDoesNotMatter()
        {
            // Derive "water" before "terrain"
            var w1 = RandomStreams.Derive(12345, RandomStreams.Water);
            var t1 = RandomStreams.Derive(12345, RandomStreams.Terrain);

            // Derive "terrain" before "water"
            var t2 = RandomStreams.Derive(12345, RandomStreams.Terrain);
            var w2 = RandomStreams.Derive(12345, RandomStreams.Water);

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(t1.NextUInt64(), t2.NextUInt64());
                Assert.Equal(w1.NextUInt64(), w2.NextUInt64());
            }
        }

        [Fact]
        public void DifferentWorldSeeds_GiveDifferentSequencesForSameName()
        {
            var r1 = RandomStreams.Derive(1, RandomStreams.Terrain);
            var r2 = RandomStreams.Derive(2, RandomStreams.Terrain);

            bool allSame = true;
            for (int i = 0; i < 100; i++)
            {
                if (r1.NextUInt64() != r2.NextUInt64())
                {
                    allSame = false;
                    break;
                }
            }
            Assert.False(allSame);
        }

        [Fact]
        public void AllStreamNames_ProduceValidRng()
        {
            string[] names = {
                RandomStreams.Terrain,
                RandomStreams.Water,
                RandomStreams.Climate,
                RandomStreams.Biome,
                RandomStreams.RenewableResources,
                RandomStreams.FiniteDeposits,
                RandomStreams.StartingLocation,
                RandomStreams.StartingSettlement
            };

            foreach (string name in names)
            {
                var rng = RandomStreams.Derive(42, name);
                // Just verify it produces values
                ulong v = rng.NextUInt64();
                Assert.NotEqual(0UL, v); // Should not be all zeros
            }
        }
    }

    public class WorldGenerationSettingsTests
    {
        [Fact]
        public void GeneratorVersion_Zero_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new WorldGenerationSettings(123, 0, WorldSize.Medium));
        }

        [Fact]
        public void GeneratorVersion_Negative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new WorldGenerationSettings(123, -1, WorldSize.Medium));
        }

        [Fact]
        public void EqualSettings_AreEqual()
        {
            var s1 = new WorldGenerationSettings(123, 1, WorldSize.Medium);
            var s2 = new WorldGenerationSettings(123, 1, WorldSize.Medium);
            Assert.Equal(s1, s2);
        }

        [Fact]
        public void DifferentSeed_MakesUnequal()
        {
            var s1 = new WorldGenerationSettings(123, 1, WorldSize.Medium);
            var s2 = new WorldGenerationSettings(456, 1, WorldSize.Medium);
            Assert.NotEqual(s1, s2);
        }

        [Fact]
        public void DifferentGeneratorVersion_MakesUnequal()
        {
            var s1 = new WorldGenerationSettings(123, 1, WorldSize.Medium);
            var s2 = new WorldGenerationSettings(123, 2, WorldSize.Medium);
            Assert.NotEqual(s1, s2);
        }

        [Fact]
        public void DifferentSize_MakesUnequal()
        {
            var s1 = new WorldGenerationSettings(123, 1, WorldSize.Small);
            var s2 = new WorldGenerationSettings(123, 1, WorldSize.Medium);
            Assert.NotEqual(s1, s2);
        }

        [Fact]
        public void CurrentGeneratorVersion_IsOne()
        {
            Assert.Equal(1, WorldGenerationSettings.CurrentGeneratorVersion);
        }
    }

    public class WorldSizeTableTests
    {
        [Fact]
        public void EveryWorldSize_HasEntry()
        {
            var table = new WorldSizeTable();

            foreach (WorldSize size in Enum.GetValues(typeof(WorldSize)))
            {
                int cells = table.GetCellsPerSide(size);
                Assert.True(cells > 0, $"WorldSize.{size} has non-positive cell count: {cells}");
            }
        }

        [Fact]
        public void Small_LessThan_Medium_LessThan_Large()
        {
            var table = new WorldSizeTable();

            int small = table.GetCellsPerSide(WorldSize.Small);
            int medium = table.GetCellsPerSide(WorldSize.Medium);
            int large = table.GetCellsPerSide(WorldSize.Large);

            Assert.True(small < medium, $"Small ({small}) not < Medium ({medium})");
            Assert.True(medium < large, $"Medium ({medium}) not < Large ({large})");
        }

        [Fact]
        public void CustomTable_AcceptsValidDictionary()
        {
            var custom = new Dictionary<WorldSize, int>
            {
                { WorldSize.Small, 64 },
                { WorldSize.Medium, 128 },
                { WorldSize.Large, 256 }
            };
            var table = new WorldSizeTable(custom);
            Assert.Equal(64, table.GetCellsPerSide(WorldSize.Small));
            Assert.Equal(128, table.GetCellsPerSide(WorldSize.Medium));
            Assert.Equal(256, table.GetCellsPerSide(WorldSize.Large));
        }

        [Fact]
        public void CustomTable_ThrowsOnMissingEntry()
        {
            var custom = new Dictionary<WorldSize, int>
            {
                { WorldSize.Small, 64 },
                { WorldSize.Medium, 128 }
                // Missing Large
            };
            Assert.Throws<ArgumentException>(() => new WorldSizeTable(custom));
        }

        [Fact]
        public void CustomTable_ThrowsOnNull()
        {
            Assert.Throws<ArgumentNullException>(() => new WorldSizeTable(null!));
        }
    }
}