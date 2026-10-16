using System;
using Xunit;
using MedievalCityBuilder.World;

namespace MedievalCityBuilder.Tests
{
    public class TerrainSurfaceTests
    {
        [Fact]
        public void Constructor_NullHeightmap_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TerrainSurface(null!, 1.0, 1.0, 0.0, 0.0));
        }

        [Fact]
        public void Constructor_CellSizeZero_ThrowsArgumentOutOfRangeException()
        {
            var heightmap = new Heightmap(5, 5);
            Assert.Throws<ArgumentOutOfRangeException>(() => new TerrainSurface(heightmap, 0.0, 1.0, 0.0, 0.0));
        }

        [Fact]
        public void Constructor_CellSizeNegative_ThrowsArgumentOutOfRangeException()
        {
            var heightmap = new Heightmap(5, 5);
            Assert.Throws<ArgumentOutOfRangeException>(() => new TerrainSurface(heightmap, -1.0, 1.0, 0.0, 0.0));
        }

        [Fact]
        public void Constructor_HeightScaleNegative_ThrowsArgumentOutOfRangeException()
        {
            var heightmap = new Heightmap(5, 5);
            Assert.Throws<ArgumentOutOfRangeException>(() => new TerrainSurface(heightmap, 1.0, -1.0, 0.0, 0.0));
        }

        [Fact]
        public void Constructor_HeightScaleZero_Allowed()
        {
            var heightmap = new Heightmap(5, 5);
            var surface = new TerrainSurface(heightmap, 1.0, 0.0, 0.0, 0.0);
            Assert.NotNull(surface);
        }

        [Fact]
        public void Constructor_OriginXNaN_ThrowsArgumentOutOfRangeException()
        {
            var heightmap = new Heightmap(5, 5);
            Assert.Throws<ArgumentOutOfRangeException>(() => new TerrainSurface(heightmap, 1.0, 1.0, double.NaN, 0.0));
        }

        [Fact]
        public void Constructor_OriginXInfinity_ThrowsArgumentOutOfRangeException()
        {
            var heightmap = new Heightmap(5, 5);
            Assert.Throws<ArgumentOutOfRangeException>(() => new TerrainSurface(heightmap, 1.0, 1.0, double.PositiveInfinity, 0.0));
        }

        [Fact]
        public void Constructor_OriginZNaN_ThrowsArgumentOutOfRangeException()
        {
            var heightmap = new Heightmap(5, 5);
            Assert.Throws<ArgumentOutOfRangeException>(() => new TerrainSurface(heightmap, 1.0, 1.0, 0.0, double.NaN));
        }

        [Fact]
        public void Constructor_OriginZInfinity_ThrowsArgumentOutOfRangeException()
        {
            var heightmap = new Heightmap(5, 5);
            Assert.Throws<ArgumentOutOfRangeException>(() => new TerrainSurface(heightmap, 1.0, 1.0, 0.0, double.NegativeInfinity));
        }

        [Fact]
        public void Centered_5x5_CellSize2_ExtentsAreMinus4To4()
        {
            var heightmap = new Heightmap(5, 5);
            var surface = TerrainSurface.Centered(heightmap, 2.0, 1.0);

            Assert.Equal(-4.0, surface.MinX);
            Assert.Equal(4.0, surface.MaxX);
            Assert.Equal(-4.0, surface.MinZ);
            Assert.Equal(4.0, surface.MaxZ);
        }

        [Fact]
        public void Centered_129x129_CellSize1_ExtentsAreMinus64To64()
        {
            var heightmap = new Heightmap(129, 129);
            var surface = TerrainSurface.Centered(heightmap, 1.0, 1.0);

            Assert.Equal(-64.0, surface.MinX);
            Assert.Equal(64.0, surface.MaxX);
            Assert.Equal(-64.0, surface.MinZ);
            Assert.Equal(64.0, surface.MaxZ);
        }

        [Fact]
        public void Contains_Inside_ReturnsTrue()
        {
            var heightmap = new Heightmap(5, 5);
            var surface = TerrainSurface.Centered(heightmap, 2.0, 1.0);

            Assert.True(surface.Contains(0.0, 0.0));
            Assert.True(surface.Contains(-2.0, 2.0));
        }

        [Fact]
        public void Contains_OnEdges_ReturnsTrue()
        {
            var heightmap = new Heightmap(5, 5);
            var surface = TerrainSurface.Centered(heightmap, 2.0, 1.0);

            // MinX edge
            Assert.True(surface.Contains(surface.MinX, 0.0));
            // MaxX edge
            Assert.True(surface.Contains(surface.MaxX, 0.0));
            // MinZ edge
            Assert.True(surface.Contains(0.0, surface.MinZ));
            // MaxZ edge
            Assert.True(surface.Contains(0.0, surface.MaxZ));
        }

        [Fact]
        public void Contains_JustOutsideEdges_ReturnsFalse()
        {
            var heightmap = new Heightmap(5, 5);
            var surface = TerrainSurface.Centered(heightmap, 2.0, 1.0);

            Assert.False(surface.Contains(surface.MinX - 0.001, 0.0));
            Assert.False(surface.Contains(surface.MaxX + 0.001, 0.0));
            Assert.False(surface.Contains(0.0, surface.MinZ - 0.001));
            Assert.False(surface.Contains(0.0, surface.MaxZ + 0.001));
        }

        [Fact]
        public void Contains_NaN_ReturnsFalse()
        {
            var heightmap = new Heightmap(5, 5);
            var surface = TerrainSurface.Centered(heightmap, 2.0, 1.0);

            Assert.False(surface.Contains(double.NaN, 0.0));
            Assert.False(surface.Contains(0.0, double.NaN));
            Assert.False(surface.Contains(double.NaN, double.NaN));
        }

        [Fact]
        public void SampleNormalized_AtVertices_EqualsStoredValue()
        {
            var heightmap = new Heightmap(5, 5);
            // Set vertices to multiples of 0.25 for exact binary representation
            for (int z = 0; z < 5; z++)
            {
                for (int x = 0; x < 5; x++)
                {
                    heightmap.Set(x, z, (float)(0.25 * (x + z)));
                }
            }

            var surface = TerrainSurface.Centered(heightmap, 2.0, 1.0);

            for (int z = 0; z < 5; z++)
            {
                for (int x = 0; x < 5; x++)
                {
                    double worldX = surface.MinX + x * 2.0;
                    double worldZ = surface.MinZ + z * 2.0;
                    double expected = 0.25 * (x + z);
                    double actual = surface.SampleNormalized(worldX, worldZ);
                    Assert.Equal(expected, actual);
                }
            }
        }

        [Fact]
        public void SampleNormalized_AtMidpointBetweenTwoVertices_EqualsAverage()
        {
            var heightmap = new Heightmap(3, 3);
            heightmap.Set(0, 0, 0.0f);
            heightmap.Set(1, 0, 1.0f);
            heightmap.Set(2, 0, 0.0f);
            heightmap.Set(0, 1, 0.0f);
            heightmap.Set(1, 1, 0.0f);
            heightmap.Set(2, 1, 0.0f);
            heightmap.Set(0, 2, 0.0f);
            heightmap.Set(1, 2, 0.0f);
            heightmap.Set(2, 2, 0.0f);

            var surface = TerrainSurface.Centered(heightmap, 2.0, 1.0);

            // Midpoint between (0,0) and (1,0) -> world X = -2+1 = -1, Z = -2
            double result = surface.SampleNormalized(-1.0, -2.0);
            Assert.Equal(0.5, result);
        }

        [Fact]
        public void SampleNormalized_AtCellCenter_EqualsAverageOfFourCorners()
        {
            var heightmap = new Heightmap(3, 3);
            heightmap.Set(0, 0, 0.0f);
            heightmap.Set(1, 0, 1.0f);
            heightmap.Set(2, 0, 0.0f);
            heightmap.Set(0, 1, 1.0f);
            heightmap.Set(1, 1, 0.0f);
            heightmap.Set(2, 1, 1.0f);
            heightmap.Set(0, 2, 0.0f);
            heightmap.Set(1, 2, 1.0f);
            heightmap.Set(2, 2, 0.0f);

            var surface = TerrainSurface.Centered(heightmap, 2.0, 1.0);

            // Cell center between vertices (0,0), (1,0), (0,1), (1,1) in heightmap coords
            // is at u=0.5, v=0.5 -> world (-1, -1)
            // Average of (0.0 + 1.0 + 1.0 + 0.0) / 4 = 0.5
            double result = surface.SampleNormalized(-1.0, -1.0);
            Assert.Equal(0.5, result);
        }

        [Fact]
        public void SampleNormalized_OnMaxEdge_EqualsStoredEdgeVertex()
        {
            var heightmap = new Heightmap(3, 3);
            heightmap.Set(0, 0, 0.0f);
            heightmap.Set(1, 0, 0.25f);
            heightmap.Set(2, 0, 0.5f);
            heightmap.Set(0, 1, 0.0f);
            heightmap.Set(1, 1, 0.25f);
            heightmap.Set(2, 1, 0.5f);
            heightmap.Set(0, 2, 0.0f);
            heightmap.Set(1, 2, 0.25f);
            heightmap.Set(2, 2, 0.5f);

            var surface = TerrainSurface.Centered(heightmap, 2.0, 1.0);

            // MaxX edge at various Z values should match the vertex at x=2
            Assert.Equal(0.5, surface.SampleNormalized(surface.MaxX, surface.MinZ));
            Assert.Equal(0.5, surface.SampleNormalized(surface.MaxX, 0.0));
            Assert.Equal(0.5, surface.SampleNormalized(surface.MaxX, surface.MaxZ));

            // MaxZ edge at various X values should match the vertex at z=2
            Assert.Equal(0.0, surface.SampleNormalized(surface.MinX, surface.MaxZ));
            Assert.Equal(0.25, surface.SampleNormalized(0.0, surface.MaxZ));
            Assert.Equal(0.5, surface.SampleNormalized(surface.MaxX, surface.MaxZ));
        }

        [Fact]
        public void SampleNormalized_AtMaxCorner_EqualsStoredCornerVertex()
        {
            var heightmap = new Heightmap(3, 3);
            heightmap.Set(2, 2, 0.75f);

            var surface = TerrainSurface.Centered(heightmap, 2.0, 1.0);

            double result = surface.SampleNormalized(surface.MaxX, surface.MaxZ);
            Assert.Equal(0.75, result);
        }

        [Fact]
        public void SampleNormalized_Outside_ThrowsArgumentOutOfRangeException()
        {
            var heightmap = new Heightmap(5, 5);
            var surface = TerrainSurface.Centered(heightmap, 2.0, 1.0);

            Assert.Throws<ArgumentOutOfRangeException>(() => surface.SampleNormalized(surface.MaxX + 0.001, 0.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => surface.SampleNormalized(surface.MinX - 0.001, 0.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => surface.SampleNormalized(0.0, surface.MaxZ + 0.001));
            Assert.Throws<ArgumentOutOfRangeException>(() => surface.SampleNormalized(0.0, surface.MinZ - 0.001));
        }

        [Fact]
        public void SampleNormalized_NaN_ThrowsArgumentOutOfRangeException()
        {
            var heightmap = new Heightmap(5, 5);
            var surface = TerrainSurface.Centered(heightmap, 2.0, 1.0);

            Assert.Throws<ArgumentOutOfRangeException>(() => surface.SampleNormalized(double.NaN, 0.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => surface.SampleNormalized(0.0, double.NaN));
        }

        [Fact]
        public void SampleHeight_EqualsSampleNormalizedTimesHeightScale()
        {
            var heightmap = new Heightmap(3, 3);
            heightmap.Set(0, 0, 0.0f);
            heightmap.Set(1, 0, 0.5f);
            heightmap.Set(2, 0, 1.0f);
            heightmap.Set(0, 1, 0.0f);
            heightmap.Set(1, 1, 0.5f);
            heightmap.Set(2, 1, 1.0f);
            heightmap.Set(0, 2, 0.0f);
            heightmap.Set(1, 2, 0.5f);
            heightmap.Set(2, 2, 1.0f);

            var surface = TerrainSurface.Centered(heightmap, 2.0, 10.0);

            double norm = surface.SampleNormalized(0.0, 0.0);
            double height = surface.SampleHeight(0.0, 0.0);
            Assert.Equal(norm * 10.0, height);
        }

        [Fact]
        public void HeightmapRamp_SampleNormalizedAtWorldX_GivesExpectedValues()
        {
            // 5x5 heightmap, value = 0.25 * x for x in 0..4
            var heightmap = new Heightmap(5, 5);
            for (int z = 0; z < 5; z++)
            {
                for (int x = 0; x < 5; x++)
                {
                    heightmap.Set(x, z, (float)(0.25 * x));
                }
            }

            // cellSize 2, Centered -> origin at -4, extents -4..4
            var surface = TerrainSurface.Centered(heightmap, 2.0, 1.0);

            Assert.Equal(0.0, surface.SampleNormalized(-4.0, 0.0));
            Assert.Equal(0.25, surface.SampleNormalized(-2.0, 0.0));
            Assert.Equal(0.5, surface.SampleNormalized(0.0, 0.0));
            Assert.Equal(0.75, surface.SampleNormalized(2.0, 0.0));
            Assert.Equal(1.0, surface.SampleNormalized(4.0, 0.0));
        }
    }
}