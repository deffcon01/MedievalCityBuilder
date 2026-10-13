using System;
using Xunit;
using MedievalCityBuilder.World;

namespace MedievalCityBuilder.Tests
{
    public class DeterministicRandomTests
    {
        [Fact]
        public void SplitMix64_Seed0_FirstOutputIsReferenceValue()
        {
            // Published reference value for SplitMix64 with seed 0
            var rng = new DeterministicRandom(0);
            ulong first = rng.NextUInt64();
            Assert.Equal(0xE220A8397B1DCDAFUL, first);
        }

        [Fact]
        public void SameSeed_ProducesIdenticalSequence()
        {
            var rng1 = new DeterministicRandom(12345);
            var rng2 = new DeterministicRandom(12345);

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(rng1.NextUInt64(), rng2.NextUInt64());
            }
        }

        [Fact]
        public void DifferentSeeds_ProduceDifferentSequences()
        {
            var rng1 = new DeterministicRandom(1);
            var rng2 = new DeterministicRandom(2);

            bool allSame = true;
            for (int i = 0; i < 100; i++)
            {
                if (rng1.NextUInt64() != rng2.NextUInt64())
                {
                    allSame = false;
                    break;
                }
            }
            Assert.False(allSame);
        }

        [Fact]
        public void NextDouble_InRange()
        {
            var rng = new DeterministicRandom(42);

            for (int i = 0; i < 10000; i++)
            {
                double d = rng.NextDouble();
                Assert.True(d >= 0.0 && d < 1.0, $"Value {d} out of range [0, 1)");
            }
        }

        [Fact]
        public void NextInt_InBounds()
        {
            var rng = new DeterministicRandom(42);

            for (int i = 0; i < 10000; i++)
            {
                int v = rng.NextInt(0, 10);
                Assert.True(v >= 0 && v < 10);
            }
        }

        [Fact]
        public void NextInt_CanProduceMinAndMaxMinusOne()
        {
            var rng = new DeterministicRandom(42);

            bool sawMin = false;
            bool sawMaxMinusOne = false;

            // Run enough times to likely hit both boundaries
            for (int i = 0; i < 10000; i++)
            {
                int v = rng.NextInt(5, 10); // [5, 10) => 5,6,7,8,9
                if (v == 5) sawMin = true;
                if (v == 9) sawMaxMinusOne = true;
            }

            Assert.True(sawMin, "Never produced min value");
            Assert.True(sawMaxMinusOne, "Never produced max-1 value");
        }

        [Fact]
        public void NextInt_ThrowsOnInvalidRange_MaxEqualsMin()
        {
            var rng = new DeterministicRandom(42);
            Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(5, 5));
        }

        [Fact]
        public void NextInt_ThrowsOnInvalidRange_MaxLessThanMin()
        {
            var rng = new DeterministicRandom(42);
            Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(10, 5));
        }

        [Fact]
        public void NextInt_RangeOfOne_ReturnsMin()
        {
            var rng = new DeterministicRandom(42);
            int v = rng.NextInt(7, 8); // [7, 8) => only 7
            Assert.Equal(7, v);
        }
    }
}