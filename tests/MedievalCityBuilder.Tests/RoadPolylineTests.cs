using System;
using Xunit;
using MedievalCityBuilder.World;
using MedievalCityBuilder.Roads;

namespace MedievalCityBuilder.Tests
{
    public class RoadPolylineTests
    {
        // Helper to create a flat heightmap with a specific value
        private static Heightmap CreateFlatHeightmap(int width, int height, float value)
        {
            var hm = new Heightmap(width, height);
            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    hm.Set(x, z, value);
                }
            }
            return hm;
        }

        // Helper to create a ramp heightmap: value = 0.25 * x
        private static Heightmap CreateRampHeightmap(int width, int height)
        {
            var hm = new Heightmap(width, height);
            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    hm.Set(x, z, (float)(0.25 * x));
                }
            }
            return hm;
        }

        // Fixture (a): flat 9x9 value 0.5 with Centered(cellSize=1.0, heightScale=10.0) -> extents -4..4, height 5.0
        private static TerrainSurface CreateFlatSurface()
        {
            var hm = CreateFlatHeightmap(9, 9, 0.5f);
            return TerrainSurface.Centered(hm, 1.0, 10.0);
        }

        // Fixture (b): ramp 5x5 value = 0.25 * x with cellSize=2, heightScale=4 -> world height 0.5 * (x + 4)
        private static TerrainSurface CreateRampSurface()
        {
            var hm = CreateRampHeightmap(5, 5);
            return TerrainSurface.Centered(hm, 2.0, 4.0);
        }

        // Non-binary surface for edge tests: flat 11x11, cellSize 0.1, Centered
        private static TerrainSurface CreateFlatEdgeSurface()
        {
            var hm = CreateFlatHeightmap(11, 11, 0.5f);
            return TerrainSurface.Centered(hm, 0.1, 10.0);
        }

        #region Validation Tests

        [Fact]
        public void Sample_NullSurface_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RoadPolyline.Sample(null!, 0, 0, 1, 1, 1.0, 0));
        }

        [Fact]
        public void Sample_NaNInX1_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadPolyline.Sample(surface, double.NaN, 0, 1, 1, 1.0, 0));
        }

        [Fact]
        public void Sample_NaNInZ1_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadPolyline.Sample(surface, 0, double.NaN, 1, 1, 1.0, 0));
        }

        [Fact]
        public void Sample_NaNInX2_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadPolyline.Sample(surface, 0, 0, double.NaN, 1, 1.0, 0));
        }

        [Fact]
        public void Sample_NaNInZ2_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadPolyline.Sample(surface, 0, 0, 1, double.NaN, 1.0, 0));
        }

        [Fact]
        public void Sample_InfinityInX1_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadPolyline.Sample(surface, double.PositiveInfinity, 0, 1, 1, 1.0, 0));
        }

        [Fact]
        public void Sample_InfinityInZ1_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadPolyline.Sample(surface, 0, double.PositiveInfinity, 1, 1, 1.0, 0));
        }

        [Fact]
        public void Sample_InfinityInX2_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadPolyline.Sample(surface, 0, 0, double.PositiveInfinity, 1, 1.0, 0));
        }

        [Fact]
        public void Sample_InfinityInZ2_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadPolyline.Sample(surface, 0, 0, 1, double.PositiveInfinity, 1.0, 0));
        }

        [Fact]
        public void Sample_SpacingZero_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadPolyline.Sample(surface, 0, 0, 1, 1, 0.0, 0));
        }

        [Fact]
        public void Sample_SpacingNegative_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadPolyline.Sample(surface, 0, 0, 1, 1, -1.0, 0));
        }

        [Fact]
        public void Sample_SpacingNaN_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadPolyline.Sample(surface, 0, 0, 1, 1, double.NaN, 0));
        }

        [Fact]
        public void Sample_SpacingInfinity_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadPolyline.Sample(surface, 0, 0, 1, 1, double.PositiveInfinity, 0));
        }

        [Fact]
        public void Sample_HeightOffsetNaN_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadPolyline.Sample(surface, 0, 0, 1, 1, 1.0, double.NaN));
        }

        [Fact]
        public void Sample_HeightOffsetInfinity_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadPolyline.Sample(surface, 0, 0, 1, 1, 1.0, double.PositiveInfinity));
        }

        [Fact]
        public void Sample_EndpointOutsidePositiveX_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface(); // extents -4..4
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadPolyline.Sample(surface, 5.0, 0, 4.0, 0, 1.0, 0));
        }

        [Fact]
        public void Sample_EndpointOutsideNegativeX_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface(); // extents -4..4
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadPolyline.Sample(surface, -5.0, 0, 4.0, 0, 1.0, 0));
        }

        [Fact]
        public void Sample_EndpointOutsidePositiveZ_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface(); // extents -4..4
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadPolyline.Sample(surface, 0, 5.0, 0, 4.0, 1.0, 0));
        }

        [Fact]
        public void Sample_EndpointOutsideNegativeZ_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface(); // extents -4..4
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadPolyline.Sample(surface, 0, -5.0, 0, 4.0, 1.0, 0));
        }

        #endregion

        #region Zero-Length Segment Tests

        [Fact]
        public void Sample_ZeroLengthSegment_ReturnsExactlyOnePoint()
        {
            var surface = CreateFlatSurface();
            var points = RoadPolyline.Sample(surface, 1.0, 2.0, 1.0, 2.0, 1.0, 0.5);

            Assert.Single(points);
            Assert.Equal(1.0, points[0].X);
            Assert.Equal(2.0, points[0].Z);
            Assert.Equal(5.0 + 0.5, points[0].Y); // flat surface height 5.0 + offset 0.5
        }

        #endregion

        #region Flat Surface Tests

        [Fact]
        public void Sample_FlatFromMinus4To4_Spacing1_Returns9Points()
        {
            var surface = CreateFlatSurface(); // extents -4..4, height 5.0
            var points = RoadPolyline.Sample(surface, -4.0, 0.0, 4.0, 0.0, 1.0, 0.0);

            Assert.Equal(9, points.Count);
            Assert.Equal(-4.0, points[0].X);
            Assert.Equal(4.0, points[8].X);
            Assert.Equal(5.0, points[0].Y);
            Assert.Equal(5.0, points[8].Y);

            for (int i = 0; i < 9; i++)
            {
                Assert.Equal(-4.0 + i, points[i].X, 1e-9);
                Assert.Equal(5.0, points[i].Y, 1e-9);
                Assert.Equal(0.0, points[i].Z, 1e-9);
            }
        }

        [Fact]
        public void Sample_FlatWithOffset_AllYEqualHeightPlusOffset()
        {
            var surface = CreateFlatSurface(); // height 5.0
            var points = RoadPolyline.Sample(surface, -4.0, 0.0, 4.0, 0.0, 1.0, 2.5);

            Assert.Equal(9, points.Count);
            foreach (var p in points)
            {
                Assert.Equal(7.5, p.Y, 1e-9); // 5.0 + 2.5
            }
        }

        [Fact]
        public void Sample_FlatSpacing3_Returns4PointsEqualSpacing()
        {
            var surface = CreateFlatSurface(); // length 8
            var points = RoadPolyline.Sample(surface, -4.0, 0.0, 4.0, 0.0, 3.0, 0.0);

            Assert.Equal(4, points.Count);
            Assert.Equal(-4.0, points[0].X);
            Assert.Equal(4.0, points[3].X);
            Assert.Equal(0.0, points[1].X, 1e-9); // -4 + 8/3 ≈ -1.333, but exact: -4 + 8*(1/3) = -4/3 ≈ -1.333
            Assert.Equal(8.0 / 3.0, points[2].X - points[1].X, 1e-9); // equal spacing
            Assert.Equal(8.0 / 3.0, points[3].X - points[2].X, 1e-9);
        }

        [Fact]
        public void Sample_FlatSpacingLargerThanLength_Returns2Points()
        {
            var surface = CreateFlatSurface(); // length 8
            var points = RoadPolyline.Sample(surface, -4.0, 0.0, 4.0, 0.0, 10.0, 0.0);

            Assert.Equal(2, points.Count);
            Assert.Equal(-4.0, points[0].X);
            Assert.Equal(4.0, points[1].X);
        }

        #endregion

        #region Ramp Surface Tests

        [Fact]
        public void Sample_RampFromMinus4To4_Spacing2_YValuesCorrect()
        {
            var surface = CreateRampSurface(); // world height = 0.5 * (x + 4)
            var points = RoadPolyline.Sample(surface, -4.0, 0.0, 4.0, 0.0, 2.0, 0.0);

            Assert.Equal(5, points.Count);
            // x = -4, -2, 0, 2, 4
            // heights: 0.5*(x+4) = 0, 1, 2, 3, 4
            Assert.Equal(-4.0, points[0].X, 1e-9);
            Assert.Equal(-2.0, points[1].X, 1e-9);
            Assert.Equal(0.0, points[2].X, 1e-9);
            Assert.Equal(2.0, points[3].X, 1e-9);
            Assert.Equal(4.0, points[4].X, 1e-9);

            Assert.Equal(0.0, points[0].Y, 1e-9);
            Assert.Equal(1.0, points[1].Y, 1e-9);
            Assert.Equal(2.0, points[2].Y, 1e-9);
            Assert.Equal(3.0, points[3].Y, 1e-9);
            Assert.Equal(4.0, points[4].Y, 1e-9);
        }

        [Fact]
        public void Sample_RampWithHeightOffset_ShiftsAllYByOffset()
        {
            var surface = CreateRampSurface();
            var points = RoadPolyline.Sample(surface, -4.0, 0.0, 4.0, 0.0, 2.0, 0.25);

            Assert.Equal(5, points.Count);
            Assert.Equal(0.25, points[0].Y, 1e-9);
            Assert.Equal(1.25, points[1].Y, 1e-9);
            Assert.Equal(2.25, points[2].Y, 1e-9);
            Assert.Equal(3.25, points[3].Y, 1e-9);
            Assert.Equal(4.25, points[4].Y, 1e-9);
        }

        #endregion

        #region Endpoint Exactness Tests

        [Fact]
        public void Sample_FirstAndLastPointsEqualEndpoints_FlatDiagonal()
        {
            var surface = CreateFlatSurface();
            var points = RoadPolyline.Sample(surface, -3.0, -3.0, 3.0, 3.0, 1.0, 0.0);

            Assert.True(points.Count >= 2);
            Assert.Equal(-3.0, points[0].X);
            Assert.Equal(-3.0, points[0].Z);
            Assert.Equal(3.0, points[^1].X);
            Assert.Equal(3.0, points[^1].Z);
        }

        [Fact]
        public void Sample_FirstAndLastPointsEqualEndpoints_RampDiagonal()
        {
            var surface = CreateRampSurface();
            var points = RoadPolyline.Sample(surface, -3.0, -3.0, 3.0, 3.0, 1.0, 0.0);

            Assert.True(points.Count >= 2);
            Assert.Equal(-3.0, points[0].X);
            Assert.Equal(-3.0, points[0].Z);
            Assert.Equal(3.0, points[^1].X);
            Assert.Equal(3.0, points[^1].Z);
        }

        #endregion

        #region Reversal Tests

        [Fact]
        public void Sample_Reversal_FlatSurface_SameCountAndReversed()
        {
            var surface = CreateFlatSurface();
            var points1 = RoadPolyline.Sample(surface, -4.0, 0.0, 4.0, 0.0, 1.0, 0.0);
            var points2 = RoadPolyline.Sample(surface, 4.0, 0.0, -4.0, 0.0, 1.0, 0.0);

            Assert.Equal(points1.Count, points2.Count);
            for (int i = 0; i < points1.Count; i++)
            {
                Assert.Equal(points1[i].X, points2[points2.Count - 1 - i].X, 1e-9);
                Assert.Equal(points1[i].Y, points2[points2.Count - 1 - i].Y, 1e-9);
                Assert.Equal(points1[i].Z, points2[points2.Count - 1 - i].Z, 1e-9);
            }
        }

        [Fact]
        public void Sample_Reversal_RampSurface_SameCountAndReversed()
        {
            var surface = CreateRampSurface();
            var points1 = RoadPolyline.Sample(surface, -4.0, 0.0, 4.0, 0.0, 1.0, 0.0);
            var points2 = RoadPolyline.Sample(surface, 4.0, 0.0, -4.0, 0.0, 1.0, 0.0);

            Assert.Equal(points1.Count, points2.Count);
            for (int i = 0; i < points1.Count; i++)
            {
                Assert.Equal(points1[i].X, points2[points2.Count - 1 - i].X, 1e-9);
                Assert.Equal(points1[i].Y, points2[points2.Count - 1 - i].Y, 1e-9);
                Assert.Equal(points1[i].Z, points2[points2.Count - 1 - i].Z, 1e-9);
            }
        }

        [Fact]
        public void Sample_Reversal_DiagonalOnFlat_SameCountAndReversed()
        {
            var surface = CreateFlatSurface();
            var points1 = RoadPolyline.Sample(surface, -3.0, -2.0, 2.0, 3.0, 0.5, 0.0);
            var points2 = RoadPolyline.Sample(surface, 2.0, 3.0, -3.0, -2.0, 0.5, 0.0);

            Assert.Equal(points1.Count, points2.Count);
            for (int i = 0; i < points1.Count; i++)
            {
                Assert.Equal(points1[i].X, points2[points2.Count - 1 - i].X, 1e-9);
                Assert.Equal(points1[i].Y, points2[points2.Count - 1 - i].Y, 1e-9);
                Assert.Equal(points1[i].Z, points2[points2.Count - 1 - i].Z, 1e-9);
            }
        }

        #endregion

        #region Point Validation Tests

        [Fact]
        public void Sample_AllPointsInsideBounds_AndYMatchesSampleHeightPlusOffset()
        {
            var surface = CreateFlatSurface();
            var points = RoadPolyline.Sample(surface, -3.0, -3.0, 3.0, 3.0, 0.5, 0.25);

            foreach (var p in points)
            {
                Assert.True(surface.Contains(p.X, p.Z), $"Point ({p.X}, {p.Z}) is outside bounds");
                double expectedY = surface.SampleHeight(p.X, p.Z) + 0.25;
                Assert.Equal(expectedY, p.Y, 1e-9);
            }
        }

        [Fact]
        public void Sample_AllPointsInsideBounds_AndYMatchesSampleHeightPlusOffset_Ramp()
        {
            var surface = CreateRampSurface();
            var points = RoadPolyline.Sample(surface, -3.0, -3.0, 3.0, 3.0, 0.5, 0.25);

            foreach (var p in points)
            {
                Assert.True(surface.Contains(p.X, p.Z), $"Point ({p.X}, {p.Z}) is outside bounds");
                double expectedY = surface.SampleHeight(p.X, p.Z) + 0.25;
                Assert.Equal(expectedY, p.Y, 1e-9);
            }
        }

        #endregion

        #region Tiny Spacing Cap Test

        [Fact]
        public void Sample_TinySpacing_ReturnsCappedPointsWithoutHanging()
        {
            var surface = CreateFlatSurface();
            // Segment from -2 to 2 (length 4), spacing 1e-9 -> stepsD = 4e9, capped at 100000
            var points = RoadPolyline.Sample(surface, -2.0, 0.0, 2.0, 0.0, 1e-9, 0.0);

            Assert.Equal(100001, points.Count); // steps + 1 = 100000 + 1
            Assert.Equal(-2.0, points[0].X, 1e-9);
            Assert.Equal(2.0, points[^1].X, 1e-9);
        }

        #endregion

        #region Edge Tests

        [Fact]
        public void Sample_SegmentAlongMinXEdge_NoException_AllPointsInside()
        {
            var surface = CreateFlatEdgeSurface(); // 11x11, cellSize 0.1, extents -0.5..0.5
            var points = RoadPolyline.Sample(surface, surface.MinX, surface.MinZ, surface.MinX, surface.MaxZ, 0.05, 0.0);

            Assert.True(points.Count >= 2);
            foreach (var p in points)
            {
                Assert.True(surface.Contains(p.X, p.Z));
            }
        }

        [Fact]
        public void Sample_SegmentAlongMaxXEdge_NoException_AllPointsInside()
        {
            var surface = CreateFlatEdgeSurface();
            var points = RoadPolyline.Sample(surface, surface.MaxX, surface.MinZ, surface.MaxX, surface.MaxZ, 0.05, 0.0);

            Assert.True(points.Count >= 2);
            foreach (var p in points)
            {
                Assert.True(surface.Contains(p.X, p.Z));
            }
        }

        [Fact]
        public void Sample_SegmentAlongMinZEdge_NoException_AllPointsInside()
        {
            var surface = CreateFlatEdgeSurface();
            var points = RoadPolyline.Sample(surface, surface.MinX, surface.MinZ, surface.MaxX, surface.MinZ, 0.05, 0.0);

            Assert.True(points.Count >= 2);
            foreach (var p in points)
            {
                Assert.True(surface.Contains(p.X, p.Z));
            }
        }

        [Fact]
        public void Sample_SegmentAlongMaxZEdge_NoException_AllPointsInside()
        {
            var surface = CreateFlatEdgeSurface();
            var points = RoadPolyline.Sample(surface, surface.MinX, surface.MaxZ, surface.MaxX, surface.MaxZ, 0.05, 0.0);

            Assert.True(points.Count >= 2);
            foreach (var p in points)
            {
                Assert.True(surface.Contains(p.X, p.Z));
            }
        }

        #endregion

        #region Fuzz Test

        [Fact]
        public void Sample_Fuzz_Seed12345_DeterministicAndValid()
        {
            // Generate terrain for seed 12345, GeneratorVersion 1, TerrainSettings.Default, 64x64
            var world = new WorldGenerationSettings(12345, 1, WorldSize.Small); // Small = 128 cells = 129 vertices, but we want 64x64
            // Actually, let's generate a 64x64 heightmap directly
            var terrain = TerrainSettings.Default;
            var heightmap = TerrainGenerator.Generate(world, terrain, 64, 64);
            var surface = TerrainSurface.Centered(heightmap, 1.0, 10.0);

            var rng = new DeterministicRandom(555);
            double spacing = 1.0;
            double heightOffset = 0.2;

            // Run twice and compare
            var results1 = new System.Collections.Generic.List<System.Collections.Generic.List<RoadPolylinePoint>>();
            var results2 = new System.Collections.Generic.List<System.Collections.Generic.List<RoadPolylinePoint>>();

            for (int run = 0; run < 2; run++)
            {
                var currentResults = run == 0 ? results1 : results2;
                var currentRng = new DeterministicRandom(555);

                for (int segment = 0; segment < 500; segment++)
                {
                    // Choose endpoints like validator fuzz: each coordinate is exactly min, exactly max, or uniform inside
                    double x1 = ChooseCoordinate(currentRng, surface.MinX, surface.MaxX);
                    double z1 = ChooseCoordinate(currentRng, surface.MinZ, surface.MaxZ);
                    double x2 = ChooseCoordinate(currentRng, surface.MinX, surface.MaxX);
                    double z2 = ChooseCoordinate(currentRng, surface.MinZ, surface.MaxZ);

                    var points = RoadPolyline.Sample(surface, x1, z1, x2, z2, spacing, heightOffset);
                    currentResults.Add(points);

                    // Assert no throw (if we get here, no exception was thrown)

                    // First and last points equal endpoints
                    Assert.Equal(x1, points[0].X, 1e-9);
                    Assert.Equal(z1, points[0].Z, 1e-9);
                    Assert.Equal(x2, points[^1].X, 1e-9);
                    Assert.Equal(z2, points[^1].Z, 1e-9);

                    // All points inside bounds
                    foreach (var p in points)
                    {
                        Assert.True(surface.Contains(p.X, p.Z), $"Point ({p.X}, {p.Z}) outside bounds");
                        double expectedY = surface.SampleHeight(p.X, p.Z) + heightOffset;
                        Assert.Equal(expectedY, p.Y, 1e-9);
                    }

                    // Consecutive points never more than spacing apart (plus 1e-9)
                    for (int i = 1; i < points.Count; i++)
                    {
                        double dx = points[i].X - points[i - 1].X;
                        double dz = points[i].Z - points[i - 1].Z;
                        double dy = points[i].Y - points[i - 1].Y;
                        double dist = Math.Sqrt(dx * dx + dz * dz + dy * dy);
                        Assert.True(dist <= spacing + 1e-9, $"Consecutive points too far apart: {dist} > {spacing}");
                    }
                }
            }

            // Assert identical results
            Assert.Equal(results1.Count, results2.Count);
            for (int i = 0; i < results1.Count; i++)
            {
                var p1 = results1[i];
                var p2 = results2[i];
                Assert.Equal(p1.Count, p2.Count);
                for (int j = 0; j < p1.Count; j++)
                {
                    Assert.Equal(p1[j].X, p2[j].X, 1e-9);
                    Assert.Equal(p1[j].Y, p2[j].Y, 1e-9);
                    Assert.Equal(p1[j].Z, p2[j].Z, 1e-9);
                }
            }
        }

        private static double ChooseCoordinate(DeterministicRandom rng, double min, double max)
        {
            // 0 = min, 1 = max, 2 = uniform inside
            int choice = rng.NextInt(0, 3);
            if (choice == 0) return min;
            if (choice == 1) return max;
            return min + rng.NextDouble() * (max - min);
        }

        #endregion
    }
}