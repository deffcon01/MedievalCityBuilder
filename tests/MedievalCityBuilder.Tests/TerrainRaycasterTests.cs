using System;
using System.Collections.Generic;
using Xunit;
using MedievalCityBuilder.World;

namespace MedievalCityBuilder.Tests
{
    public class TerrainRaycasterTests
    {
        private const double Tolerance = 1e-6;
        private const double ToleranceY = 1e-9;

        private static (TerrainSurface FlatSurface, TerrainSurface RampSurface) CreateFixtures()
        {
            // (a) flat 9x9 heightmap value 0.5, TerrainSurface.Centered(hm, 1.0, 10.0)
            // extents -4..4, surface height 5.0
            var flatHeightmap = new Heightmap(9, 9);
            for (int z = 0; z < 9; z++)
            {
                for (int x = 0; x < 9; x++)
                {
                    flatHeightmap.Set(x, z, 0.5f);
                }
            }
            var flatSurface = TerrainSurface.Centered(flatHeightmap, 1.0, 10.0);

            // (b) ramp 5x5 value = 0.25 * x, cellSize 2, heightScale 4, TerrainSurface.Centered
            // world height = 0.5 * (x + 4)
            var rampHeightmap = new Heightmap(5, 5);
            for (int z = 0; z < 5; z++)
            {
                for (int x = 0; x < 5; x++)
                {
                    rampHeightmap.Set(x, z, (float)(0.25 * x));
                }
            }
            var rampSurface = TerrainSurface.Centered(rampHeightmap, 2.0, 4.0);

            return (flatSurface, rampSurface);
        }

        #region Validation Tests

        [Fact]
        public void TryRaycast_NullSurface_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => TerrainRaycaster.TryRaycast(
                null!, 0, 0, 0, 0, -1, 0, 10, 0.5, out _));
        }

        [Theory]
        [InlineData(double.NaN, 0, 0)]
        [InlineData(double.PositiveInfinity, 0, 0)]
        [InlineData(double.NegativeInfinity, 0, 0)]
        [InlineData(0, double.NaN, 0)]
        [InlineData(0, double.PositiveInfinity, 0)]
        [InlineData(0, double.NegativeInfinity, 0)]
        [InlineData(0, 0, double.NaN)]
        [InlineData(0, 0, double.PositiveInfinity)]
        [InlineData(0, 0, double.NegativeInfinity)]
        public void TryRaycast_OriginNaNOrInfinite_ThrowsArgumentOutOfRangeException(double ox, double oy, double oz)
        {
            var surface = new Heightmap(5, 5);
            var ts = TerrainSurface.Centered(surface, 1.0, 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => TerrainRaycaster.TryRaycast(
                ts, ox, oy, oz, 0, -1, 0, 10, 0.5, out _));
        }

        [Theory]
        [InlineData(double.NaN, 0, 0)]
        [InlineData(double.PositiveInfinity, 0, 0)]
        [InlineData(double.NegativeInfinity, 0, 0)]
        [InlineData(0, double.NaN, 0)]
        [InlineData(0, double.PositiveInfinity, 0)]
        [InlineData(0, double.NegativeInfinity, 0)]
        [InlineData(0, 0, double.NaN)]
        [InlineData(0, 0, double.PositiveInfinity)]
        [InlineData(0, 0, double.NegativeInfinity)]
        public void TryRaycast_DirectionNaNOrInfinite_ThrowsArgumentOutOfRangeException(double dx, double dy, double dz)
        {
            var surface = new Heightmap(5, 5);
            var ts = TerrainSurface.Centered(surface, 1.0, 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => TerrainRaycaster.TryRaycast(
                ts, 0, 10, 0, dx, dy, dz, 10, 0.5, out _));
        }

        [Fact]
        public void TryRaycast_ZeroDirection_ThrowsArgumentException()
        {
            var surface = new Heightmap(5, 5);
            var ts = TerrainSurface.Centered(surface, 1.0, 1.0);
            Assert.Throws<ArgumentException>(() => TerrainRaycaster.TryRaycast(
                ts, 0, 10, 0, 0, 0, 0, 10, 0.5, out _));
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        public void TryRaycast_InvalidMaxDistance_ThrowsArgumentOutOfRangeException(double maxDistance)
        {
            var surface = new Heightmap(5, 5);
            var ts = TerrainSurface.Centered(surface, 1.0, 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => TerrainRaycaster.TryRaycast(
                ts, 0, 10, 0, 0, -1, 0, maxDistance, 0.5, out _));
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        public void TryRaycast_InvalidStep_ThrowsArgumentOutOfRangeException(double step)
        {
            var surface = new Heightmap(5, 5);
            var ts = TerrainSurface.Centered(surface, 1.0, 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => TerrainRaycaster.TryRaycast(
                ts, 0, 10, 0, 0, -1, 0, 10, step, out _));
        }

        [Fact]
        public void TryRaycast_MaxDistanceOverStepTooLarge_ThrowsArgumentOutOfRangeException()
        {
            var surface = new Heightmap(5, 5);
            var ts = TerrainSurface.Centered(surface, 1.0, 1.0);
            // maxDistance / step > 1,000,000
            Assert.Throws<ArgumentOutOfRangeException>(() => TerrainRaycaster.TryRaycast(
                ts, 0, 10, 0, 0, -1, 0, 1000001, 0.5, out _));
        }

        #endregion

        #region Flat Surface Tests

        [Fact]
        public void TryRaycast_Flat_VerticalDown_Hits()
        {
            var (flatSurface, _) = CreateFixtures();
            // origin (0, 15, 0), direction (0, -1, 0)
            // height is at 5.0, so distance = 15 - 5 = 10
            bool hit = TerrainRaycaster.TryRaycast(flatSurface, 0, 15, 0, 0, -1, 0, 20, 0.5, out var result);
            Assert.True(hit);
            Assert.Equal(0.0, result.X, Tolerance);
            Assert.Equal(5.0, result.Y, ToleranceY);
            Assert.Equal(0.0, result.Z, Tolerance);
            Assert.Equal(10.0, result.Distance, Tolerance);
        }

        [Fact]
        public void TryRaycast_Flat_VerticalDown_ScaledDirection_SameHit()
        {
            var (flatSurface, _) = CreateFixtures();
            // direction (0, -2, 0) gets normalized to (0, -1, 0), same hit
            bool hit = TerrainRaycaster.TryRaycast(flatSurface, 0, 15, 0, 0, -2, 0, 20, 0.5, out var result);
            Assert.True(hit);
            Assert.Equal(0.0, result.X, Tolerance);
            Assert.Equal(5.0, result.Y, ToleranceY);
            Assert.Equal(0.0, result.Z, Tolerance);
            Assert.Equal(10.0, result.Distance, Tolerance);
        }

        [Fact]
        public void TryRaycast_Flat_DiagonalFromLeft_Hits()
        {
            var (flatSurface, _) = CreateFixtures();
            // origin (-4, 9, 0), direction (1, -1, 0)
            // y = 9 - t, x = -4 + t, want y = 5 => t = 4, x = 0
            // x=0 is within bounds [-4, 4]
            bool hit = TerrainRaycaster.TryRaycast(flatSurface, -4, 9, 0, 1, -1, 0, 20, 0.5, out var result);
            Assert.True(hit);
            Assert.Equal(0.0, result.X, Tolerance);
            Assert.Equal(5.0, result.Y, ToleranceY);
            Assert.Equal(0.0, result.Z, Tolerance);
            // distance = t * sqrt(2) = 4 * 1.414... = 5.65685...
            Assert.Equal(4.0 * Math.Sqrt(2), result.Distance, Tolerance);
        }

        [Fact]
        public void TryRaycast_Flat_EntersFromOutside_Hits()
        {
            var (flatSurface, _) = CreateFixtures();
            // origin (-20, 25, 0), direction (1, -1, 0)
            // param: x = -20 + t, y = 25 - t
            // want y = 5 => t = 20, then x = 0 (within bounds)
            bool hit = TerrainRaycaster.TryRaycast(flatSurface, -20, 25, 0, 1, -1, 0, 50, 0.5, out var result);
            Assert.True(hit);
            Assert.Equal(0.0, result.X, Tolerance);
            Assert.Equal(5.0, result.Y, ToleranceY);
            Assert.Equal(0.0, result.Z, Tolerance);
            Assert.Equal(20.0 * Math.Sqrt(2), result.Distance, Tolerance);
        }

        #endregion

        #region Ramp Surface Tests

        [Fact]
        public void TryRaycast_Ramp_AtX1_HitsCorrectY()
        {
            var (_, rampSurface) = CreateFixtures();
            // origin (1, 10, 0), direction (0, -1, 0)
            // world x = 1 -> ramp x = (1 + 4) / 2 = 2.5? Wait...
            // Centered: origin at -4, so x vertex index = (x + 4) / 2
            // At x=1: vertex index = 2.5 -> height = 0.25 * 2.5 = 0.625 -> world height = 0.625 * 4 = 2.5
            bool hit = TerrainRaycaster.TryRaycast(rampSurface, 1, 10, 0, 0, -1, 0, 20, 0.5, out var result);
            Assert.True(hit);
            Assert.Equal(1.0, result.X, Tolerance);
            Assert.Equal(2.5, result.Y, ToleranceY);
            Assert.Equal(0.0, result.Z, Tolerance);
            Assert.Equal(7.5, result.Distance, Tolerance); // 10 - 2.5 = 7.5
        }

        [Fact]
        public void TryRaycast_Ramp_AtXMinus3_HitsCorrectY()
        {
            var (_, rampSurface) = CreateFixtures();
            // origin (-3, 10, 0)
            // world x = -3 -> vertex index = 0.5 -> height = 0.25 * 0.5 = 0.125 -> world height = 0.5
            bool hit = TerrainRaycaster.TryRaycast(rampSurface, -3, 10, 0, 0, -1, 0, 20, 0.5, out var result);
            Assert.True(hit);
            Assert.Equal(-3.0, result.X, Tolerance);
            Assert.Equal(0.5, result.Y, ToleranceY);
            Assert.Equal(0.0, result.Z, Tolerance);
            Assert.Equal(9.5, result.Distance, Tolerance);
        }

        [Fact]
        public void TryRaycast_Ramp_AtX4_HitsCorrectY()
        {
            var (_, rampSurface) = CreateFixtures();
            // origin (4, 10, 0) - max edge
            // world x = 4 -> vertex index = 4 -> height = 0.25 * 4 = 1.0 -> world height = 4.0
            bool hit = TerrainRaycaster.TryRaycast(rampSurface, 4, 10, 0, 0, -1, 0, 20, 0.5, out var result);
            Assert.True(hit);
            Assert.Equal(4.0, result.X, Tolerance);
            Assert.Equal(4.0, result.Y, ToleranceY);
            Assert.Equal(0.0, result.Z, Tolerance);
            Assert.Equal(6.0, result.Distance, Tolerance);
        }

        [Fact]
        public void TryRaycast_Ramp_AtCornerMinus4Minus4_HitsCorrectY()
        {
            var (_, rampSurface) = CreateFixtures();
            // origin (-4, 10, -4) - corner
            // world x = -4 -> vertex index = 0 -> height = 0 -> world height = 0.0
            // world z = -4 -> vertex index = 0
            bool hit = TerrainRaycaster.TryRaycast(rampSurface, -4, 10, -4, 0, -1, 0, 20, 0.5, out var result);
            Assert.True(hit);
            Assert.Equal(-4.0, result.X, Tolerance);
            Assert.Equal(0.0, result.Y, ToleranceY);
            Assert.Equal(-4.0, result.Z, Tolerance);
            Assert.Equal(10.0, result.Distance, Tolerance);
        }

        #endregion

        #region No Hit Tests

        [Fact]
        public void TryRaycast_Flat_HorizontalAbove_NoHit()
        {
            var (flatSurface, _) = CreateFixtures();
            // origin (0, 15, 0), direction (1, 0, 0) - horizontal
            bool hit = TerrainRaycaster.TryRaycast(flatSurface, 0, 15, 0, 1, 0, 0, 20, 0.5, out var result);
            Assert.False(hit);
        }

        [Fact]
        public void TryRaycast_Flat_PointingUp_HitsAtDistanceZero()
        {
            var (flatSurface, _) = CreateFixtures();
            // origin (0, 5, 0), direction (0, 1, 0) - pointing up from surface
            // Ray is exactly on the surface at first sample -> hit at distance 0
            bool hit = TerrainRaycaster.TryRaycast(flatSurface, 0, 5, 0, 0, 1, 0, 20, 0.5, out var result);
            Assert.True(hit);
            Assert.Equal(0.0, result.X, Tolerance);
            Assert.Equal(5.0, result.Y, ToleranceY);
            Assert.Equal(0.0, result.Z, Tolerance);
            Assert.Equal(0.0, result.Distance, Tolerance);
        }

        [Fact]
        public void TryRaycast_Flat_StraightDownOutsideBounds_NoHit()
        {
            var (flatSurface, _) = CreateFixtures();
            // origin (10, 15, 0) - outside bounds (bounds are -4..4)
            bool hit = TerrainRaycaster.TryRaycast(flatSurface, 10, 15, 0, 0, -1, 0, 20, 0.5, out var result);
            Assert.False(hit);
        }

        [Fact]
        public void TryRaycast_Flat_StartBelowSurface_NoHit()
        {
            var (flatSurface, _) = CreateFixtures();
            // origin (0, 0, 0) - below surface (surface at y=5)
            bool hit = TerrainRaycaster.TryRaycast(flatSurface, 0, 0, 0, 0, -1, 0, 20, 0.5, out var result);
            Assert.False(hit);
        }

        [Fact]
        public void TryRaycast_Flat_MaxDistanceTooShort_NoHit()
        {
            var (flatSurface, _) = CreateFixtures();
            // origin (0, 15, 0), maxDistance = 5 (need 10 to reach surface)
            bool hit = TerrainRaycaster.TryRaycast(flatSurface, 0, 15, 0, 0, -1, 0, 5, 0.5, out var result);
            Assert.False(hit);
        }

        [Fact]
        public void TryRaycast_Flat_EntersBoundsBelowSurface_NoHit()
        {
            var (flatSurface, _) = CreateFixtures();
            // origin (-10, 0, 0) - below surface and outside
            // direction (1, 0, 0) - horizontal into bounds but below surface
            bool hit = TerrainRaycaster.TryRaycast(flatSurface, -10, 0, 0, 1, 0, 0, 30, 0.5, out var result);
            Assert.False(hit);
        }

        #endregion

        #region Ray On Surface Test

        [Fact]
        public void TryRaycast_Flat_RayOnSurface_HitsAtDistanceZero()
        {
            var (flatSurface, _) = CreateFixtures();
            // origin (-4, 5, 0), direction (1, 0, 0) - exactly on surface
            bool hit = TerrainRaycaster.TryRaycast(flatSurface, -4, 5, 0, 1, 0, 0, 20, 0.5, out var result);
            Assert.True(hit);
            Assert.Equal(-4.0, result.X, Tolerance);
            Assert.Equal(5.0, result.Y, ToleranceY);
            Assert.Equal(0.0, result.Z, Tolerance);
            Assert.Equal(0.0, result.Distance, Tolerance);
        }

        #endregion

        #region Hit Accuracy Tests

        [Fact]
        public void TryRaycast_HitYEqualsSampleHeight()
        {
            var (flatSurface, _) = CreateFixtures();
            bool hit = TerrainRaycaster.TryRaycast(flatSurface, 0, 15, 0, 0, -1, 0, 20, 0.5, out var result);
            Assert.True(hit);
            double sampledY = flatSurface.SampleHeight(result.X, result.Z);
            Assert.Equal(sampledY, result.Y, ToleranceY);
        }

        [Fact]
        public void TryRaycast_HitPointEqualsOriginPlusDirTimesDistance()
        {
            var (flatSurface, _) = CreateFixtures();
            // Use diagonal ray for non-vertical test
            bool hit = TerrainRaycaster.TryRaycast(flatSurface, -4, 9, 0, 1, -1, 0, 20, 0.5, out var result);
            Assert.True(hit);
            double dirLen = Math.Sqrt(2);
            double expectedX = -4 + (1 / dirLen) * result.Distance;
            double expectedZ = 0 + (0 / dirLen) * result.Distance;
            Assert.Equal(expectedX, result.X, Tolerance);
            Assert.Equal(expectedZ, result.Z, Tolerance);
        }

        #endregion

        #region Determinism Tests

        [Fact]
        public void TryRaycast_Deterministic_SameCallTwice()
        {
            var (flatSurface, _) = CreateFixtures();
            bool hit1 = TerrainRaycaster.TryRaycast(flatSurface, 0, 15, 0, 0, -1, 0, 20, 0.5, out var result1);
            bool hit2 = TerrainRaycaster.TryRaycast(flatSurface, 0, 15, 0, 0, -1, 0, 20, 0.5, out var result2);
            Assert.Equal(hit1, hit2);
            if (hit1)
            {
                Assert.Equal(result1.X, result2.X);
                Assert.Equal(result1.Y, result2.Y);
                Assert.Equal(result1.Z, result2.Z);
                Assert.Equal(result1.Distance, result2.Distance);
            }
        }

        #endregion

        #region Fuzz Test

        [Fact]
        public void TryRaycast_Fuzz_2000Rays_Seed2024()
        {
            var worldSettings = new WorldGenerationSettings(12345, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Small);
            var terrainSettings = TerrainSettings.Default;
            var heightmap = TerrainGenerator.Generate(worldSettings, terrainSettings, 64, 64);
            var surface = TerrainSurface.Centered(heightmap, 1.0, 10.0);

            var rng = new DeterministicRandom(2024);
            const int numRays = 2000;
            var results = new List<(bool hit, TerrainRayHit result, double originX, double originY, double originZ, double dirX, double dirY, double dirZ)>();

            double minX = surface.MinX - 10;
            double maxX = surface.MaxX + 10;
            double minZ = surface.MinZ - 10;
            double maxZ = surface.MaxZ + 10;

            for (int i = 0; i < numRays; i++)
            {
                double originX = rng.NextDouble() * (maxX - minX) + minX;
                double originZ = rng.NextDouble() * (maxZ - minZ) + minZ;
                double originY = 10 + rng.NextDouble() * 30; // [10, 40]

                double dirX = rng.NextDouble() * 2 - 1; // [-1, 1]
                double dirZ = rng.NextDouble() * 2 - 1; // [-1, 1]
                double dirY = -1 + rng.NextDouble() * -0.9; // [-1, -0.1] (always pointing down)

                bool hit = TerrainRaycaster.TryRaycast(surface, originX, originY, originZ, dirX, dirY, dirZ, 200, 0.5, out var result);
                results.Add((hit, result, originX, originY, originZ, dirX, dirY, dirZ));
            }

            // Verify all hits
            int hitCount = 0;
            int missCount = 0;
            foreach (var (hit, result, originX, originY, originZ, dirX, dirY, dirZ) in results)
            {
                if (hit)
                {
                    hitCount++;
                    // Distance is within [0, maxDistance]
                    Assert.True(result.Distance >= 0 && result.Distance <= 200, $"Distance {result.Distance} out of range");

                    // Y equals SampleHeight(X, Z) within 1e-6
                    double sampledY = surface.SampleHeight(result.X, result.Z);
                    Assert.Equal(sampledY, result.Y, Tolerance);

                    // Hit point equals origin + normalized dir * Distance in X and Z within 1e-6
                    double dirLen = Math.Sqrt(dirX * dirX + dirY * dirY + dirZ * dirZ);
                    double expectedX = originX + (dirX / dirLen) * result.Distance;
                    double expectedZ = originZ + (dirZ / dirLen) * result.Distance;
                    Assert.Equal(expectedX, result.X, Tolerance);
                    Assert.Equal(expectedZ, result.Z, Tolerance);
                }
                else
                {
                    missCount++;
                }
            }

            Assert.True(hitCount >= 500, $"Expected at least 500 hits, got {hitCount}");
            Assert.True(missCount >= 100, $"Expected at least 100 misses, got {missCount}");

            // Run again and assert identical hit lists
            var rng2 = new DeterministicRandom(2024);
            for (int i = 0; i < numRays; i++)
            {
                double originX = rng2.NextDouble() * (maxX - minX) + minX;
                double originZ = rng2.NextDouble() * (maxZ - minZ) + minZ;
                double originY = 10 + rng2.NextDouble() * 30;

                double dirX = rng2.NextDouble() * 2 - 1;
                double dirZ = rng2.NextDouble() * 2 - 1;
                double dirY = -1 + rng2.NextDouble() * -0.9;

                bool hit = TerrainRaycaster.TryRaycast(surface, originX, originY, originZ, dirX, dirY, dirZ, 200, 0.5, out var result);

                var (expectedHit, expectedResult, _, _, _, _, _, _) = results[i];
                Assert.Equal(expectedHit, hit);
                if (hit)
                {
                    Assert.Equal(expectedResult.X, result.X);
                    Assert.Equal(expectedResult.Y, result.Y);
                    Assert.Equal(expectedResult.Z, result.Z);
                    Assert.Equal(expectedResult.Distance, result.Distance);
                }
            }
        }

        #endregion
    }
}