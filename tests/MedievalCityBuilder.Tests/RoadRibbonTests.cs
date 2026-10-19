using System;
using Xunit;
using MedievalCityBuilder.World;
using MedievalCityBuilder.Roads;

namespace MedievalCityBuilder.Tests
{
    public class RoadRibbonTests
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

        // Helper to compute Y component of (b-a) x (c-a) from positions array
        // u = b - a, v = c - a
        // y = u.z * v.x - u.x * v.z
        private static double CrossY(float[] positions, int a, int b, int c)
        {
            // a, b, c are vertex indices
            int ai = a * 3;
            int bi = b * 3;
            int ci = c * 3;

            double ux = positions[bi] - positions[ai];
            double uy = positions[bi + 1] - positions[ai + 1];
            double uz = positions[bi + 2] - positions[ai + 2];

            double vx = positions[ci] - positions[ai];
            double vy = positions[ci + 1] - positions[ai + 1];
            double vz = positions[ci + 2] - positions[ai + 2];

            return uz * vx - ux * vz;
        }

        #region RoadRibbonMesh Constructor Validation Tests

        [Fact]
        public void RoadRibbonMesh_NullPositions_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RoadRibbonMesh(null!, new float[3], new int[3]));
        }

        [Fact]
        public void RoadRibbonMesh_NullNormals_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RoadRibbonMesh(new float[3], null!, new int[3]));
        }

        [Fact]
        public void RoadRibbonMesh_NullIndices_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RoadRibbonMesh(new float[3], new float[3], null!));
        }

        [Fact]
        public void RoadRibbonMesh_MismatchedPositionsAndNormalsLength_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new RoadRibbonMesh(new float[6], new float[3], new int[3]));
        }

        [Fact]
        public void RoadRibbonMesh_PositionsLengthNotDivisibleBy3_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new RoadRibbonMesh(new float[4], new float[4], new int[3]));
        }

        [Fact]
        public void RoadRibbonMesh_IndicesLengthNotDivisibleBy3_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new RoadRibbonMesh(new float[3], new float[3], new int[4]));
        }

        #endregion

        #region Build Validation Tests

        [Fact]
        public void Build_NullSurface_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RoadRibbonBuilder.Build(null!, 0, 0, 1, 1, 2.0, 1.0, 0.0));
        }

        [Fact]
        public void Build_NaNInX1_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, double.NaN, 0, 1, 1, 2.0, 1.0, 0.0));
        }

        [Fact]
        public void Build_NaNInZ1_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 0, double.NaN, 1, 1, 2.0, 1.0, 0.0));
        }

        [Fact]
        public void Build_NaNInX2_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 0, 0, double.NaN, 1, 2.0, 1.0, 0.0));
        }

        [Fact]
        public void Build_NaNInZ2_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 0, 0, 1, double.NaN, 2.0, 1.0, 0.0));
        }

        [Fact]
        public void Build_InfinityInX1_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, double.PositiveInfinity, 0, 1, 1, 2.0, 1.0, 0.0));
        }

        [Fact]
        public void Build_InfinityInZ1_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 0, double.PositiveInfinity, 1, 1, 2.0, 1.0, 0.0));
        }

        [Fact]
        public void Build_InfinityInX2_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 0, 0, double.PositiveInfinity, 1, 2.0, 1.0, 0.0));
        }

        [Fact]
        public void Build_InfinityInZ2_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 0, 0, 1, double.PositiveInfinity, 2.0, 1.0, 0.0));
        }

        [Fact]
        public void Build_WidthZero_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 0, 0, 1, 1, 0.0, 1.0, 0.0));
        }

        [Fact]
        public void Build_WidthNegative_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 0, 0, 1, 1, -1.0, 1.0, 0.0));
        }

        [Fact]
        public void Build_WidthNaN_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 0, 0, 1, 1, double.NaN, 1.0, 0.0));
        }

        [Fact]
        public void Build_WidthInfinity_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 0, 0, 1, 1, double.PositiveInfinity, 1.0, 0.0));
        }

        [Fact]
        public void Build_SpacingZero_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 0, 0, 1, 1, 2.0, 0.0, 0.0));
        }

        [Fact]
        public void Build_SpacingNegative_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 0, 0, 1, 1, 2.0, -1.0, 0.0));
        }

        [Fact]
        public void Build_SpacingNaN_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 0, 0, 1, 1, 2.0, double.NaN, 0.0));
        }

        [Fact]
        public void Build_SpacingInfinity_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 0, 0, 1, 1, 2.0, double.PositiveInfinity, 0.0));
        }

        [Fact]
        public void Build_HeightOffsetNaN_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 0, 0, 1, 1, 2.0, 1.0, double.NaN));
        }

        [Fact]
        public void Build_HeightOffsetInfinity_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 0, 0, 1, 1, 2.0, 1.0, double.PositiveInfinity));
        }

        [Fact]
        public void Build_EndpointOutsidePositiveX_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface(); // extents -4..4
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 5.0, 0, 4.0, 0, 2.0, 1.0, 0.0));
        }

        [Fact]
        public void Build_EndpointOutsideNegativeX_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface(); // extents -4..4
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, -5.0, 0, 4.0, 0, 2.0, 1.0, 0.0));
        }

        [Fact]
        public void Build_EndpointOutsidePositiveZ_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface(); // extents -4..4
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 0, 5.0, 0, 4.0, 2.0, 1.0, 0.0));
        }

        [Fact]
        public void Build_EndpointOutsideNegativeZ_ThrowsArgumentOutOfRangeException()
        {
            var surface = CreateFlatSurface(); // extents -4..4
            Assert.Throws<ArgumentOutOfRangeException>(() => RoadRibbonBuilder.Build(surface, 0, -5.0, 0, 4.0, 2.0, 1.0, 0.0));
        }

        [Fact]
        public void Build_ZeroLengthSegment_ThrowsArgumentException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentException>(() => RoadRibbonBuilder.Build(surface, 1.0, 2.0, 1.0, 2.0, 2.0, 1.0, 0.0));
        }

        #endregion

        #region Flat Surface Tests

        [Fact]
        public void Build_FlatFromMinus3To3_Width2_Spacing1_HeightOffset015_CorrectVerticesAndTriangles()
        {
            var surface = CreateFlatSurface(); // extents -4..4, height 5.0
            var mesh = RoadRibbonBuilder.Build(surface, -3.0, 0.0, 3.0, 0.0, 2.0, 1.0, 0.15);

            // Length = 6, spacing = 1 -> steps = ceil(6/1) = 6, points = 7
            Assert.Equal(14, mesh.VertexCount); // 7 * 2
            Assert.Equal(12, mesh.TriangleCount); // 2 * (7 - 1) = 12

            // Vertex 0: left at (-3, 0) -> x = -3 + (-dz/len)*half = -3 + 0 = -3, z = 0 + (dx/len)*half = 0 + 1*1 = 1
            // Wait: dx = 6, dz = 0, len = 6, px = -0/6 = 0, pz = 6/6 = 1, half = 1
            // Left: lx = cx + px*half = cx + 0 = cx, lz = cz + pz*half = cz + 1
            // Right: rx = cx - px*half = cx - 0 = cx, rz = cz - pz*half = cz - 1
            // At x = -3: left = (-3, 5.15, 1), right = (-3, 5.15, -1)
            Assert.Equal(-3.0, mesh.Positions[0], 1e-6);
            Assert.Equal(5.15, mesh.Positions[1], 1e-6);
            Assert.Equal(1.0, mesh.Positions[2], 1e-6);

            Assert.Equal(-3.0, mesh.Positions[3], 1e-6);
            Assert.Equal(5.15, mesh.Positions[4], 1e-6);
            Assert.Equal(-1.0, mesh.Positions[5], 1e-6);

            // Vertex 12 (index 6*2 = 12): left at (3, 0) -> (3, 5.15, 1)
            // Vertex 13: right at (3, 0) -> (3, 5.15, -1)
            Assert.Equal(3.0, mesh.Positions[36], 1e-6);
            Assert.Equal(5.15, mesh.Positions[37], 1e-6);
            Assert.Equal(1.0, mesh.Positions[38], 1e-6);

            Assert.Equal(3.0, mesh.Positions[39], 1e-6);
            Assert.Equal(5.15, mesh.Positions[40], 1e-6);
            Assert.Equal(-1.0, mesh.Positions[41], 1e-6);

            // Every Y equals 5.15
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                Assert.Equal(5.15, mesh.Positions[i * 3 + 1], 1e-6);
            }

            // Every normal is (0, 1, 0)
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                Assert.Equal(0.0f, mesh.Normals[i * 3], 1e-6);
                Assert.Equal(1.0f, mesh.Normals[i * 3 + 1], 1e-6);
                Assert.Equal(0.0f, mesh.Normals[i * 3 + 2], 1e-6);
            }

            // Every index is within [0, VertexCount)
            foreach (int idx in mesh.Indices)
            {
                Assert.True(idx >= 0 && idx < mesh.VertexCount, $"Index {idx} out of bounds [0, {mesh.VertexCount})");
            }
        }

        #endregion

        #region Winding Tests

        [Theory]
        [InlineData(-3.0, 0.0, 3.0, 0.0)]   // (-3,0)->(3,0)
        [InlineData(3.0, 0.0, -3.0, 0.0)]   // (3,0)->(-3,0)
        [InlineData(0.0, -3.0, 0.0, 3.0)]   // (0,-3)->(0,3)
        [InlineData(0.0, 3.0, 0.0, -3.0)]   // (0,3)->(0,-3)
        [InlineData(-3.0, -3.0, 3.0, 3.0)]  // (-3,-3)->(3,3)
        [InlineData(3.0, 3.0, -3.0, -3.0)]  // (3,3)->(-3,-3)
        [InlineData(-3.0, 3.0, 3.0, -3.0)]  // (-3,3)->(3,-3)
        public void Build_FlatSegment_AllTrianglesHaveNegativeYCrossComponent(double x1, double z1, double x2, double z2)
        {
            var surface = CreateFlatSurface();
            var mesh = RoadRibbonBuilder.Build(surface, x1, z1, x2, z2, 2.0, 1.0, 0.0);

            for (int i = 0; i < mesh.TriangleCount; i++)
            {
                int a = mesh.Indices[i * 3];
                int b = mesh.Indices[i * 3 + 1];
                int c = mesh.Indices[i * 3 + 2];
                double crossY = CrossY(mesh.Positions, a, b, c);
                Assert.True(crossY < 0, $"Triangle {i} has non-negative cross Y: {crossY} (indices {a},{b},{c})");
            }
        }

        #endregion

        #region Ramp Surface Tests

        [Fact]
        public void Build_RampSegmentAlongZAtX0_LeftAndRightVerticesHaveCorrectHeights()
        {
            var surface = CreateRampSurface(); // world height = 0.5 * (x + 4), extents -4..4
            // Segment along Z at x = 0: (0, -3) -> (0, 3)
            var mesh = RoadRibbonBuilder.Build(surface, 0.0, -3.0, 0.0, 3.0, 2.0, 1.0, 0.0);

            // dx = 0, dz = 6, len = 6
            // px = -dz/len = -1, pz = dx/len = 0
            // half = 1
            // Left: lx = cx + px*half = cx - 1, lz = cz + 0 = cz
            // Right: rx = cx - px*half = cx + 1, rz = cz
            // At x = 0: left x = -1, right x = +1
            // World height at x = -1: 0.5 * (-1 + 4) = 1.5
            // World height at x = +1: 0.5 * (1 + 4) = 2.5
            // Points at z = -3, -2, -1, 0, 1, 2, 3 (7 points, spacing 1)

            int pointCount = 7;
            Assert.Equal(14, mesh.VertexCount);
            Assert.Equal(12, mesh.TriangleCount);

            for (int i = 0; i < pointCount; i++)
            {
                int leftIdx = i * 2;
                int rightIdx = i * 2 + 1;

                // Left vertex (even index)
                Assert.Equal(-1.0, mesh.Positions[leftIdx * 3], 1e-9); // x = -1
                Assert.Equal(1.5, mesh.Positions[leftIdx * 3 + 1], 1e-9); // Y = 1.5

                // Right vertex (odd index)
                Assert.Equal(1.0, mesh.Positions[rightIdx * 3], 1e-9); // x = +1
                Assert.Equal(2.5, mesh.Positions[rightIdx * 3 + 1], 1e-9); // Y = 2.5
            }
        }

        #endregion

        #region Reversal Tests

        [Fact]
        public void Build_Reversal_SameVertexCountTriangleCountAndNegativeWinding()
        {
            var surface = CreateFlatSurface();
            var mesh1 = RoadRibbonBuilder.Build(surface, -3.0, 0.0, 3.0, 0.0, 2.0, 1.0, 0.0);
            var mesh2 = RoadRibbonBuilder.Build(surface, 3.0, 0.0, -3.0, 0.0, 2.0, 1.0, 0.0);

            Assert.Equal(mesh1.VertexCount, mesh2.VertexCount);
            Assert.Equal(mesh1.TriangleCount, mesh2.TriangleCount);

            // Both have all-negative winding
            for (int i = 0; i < mesh1.TriangleCount; i++)
            {
                int a = mesh1.Indices[i * 3];
                int b = mesh1.Indices[i * 3 + 1];
                int c = mesh1.Indices[i * 3 + 2];
                double crossY = CrossY(mesh1.Positions, a, b, c);
                Assert.True(crossY < 0, $"Mesh1 triangle {i} has non-negative cross Y: {crossY}");
            }

            for (int i = 0; i < mesh2.TriangleCount; i++)
            {
                int a = mesh2.Indices[i * 3];
                int b = mesh2.Indices[i * 3 + 1];
                int c = mesh2.Indices[i * 3 + 2];
                double crossY = CrossY(mesh2.Positions, a, b, c);
                Assert.True(crossY < 0, $"Mesh2 triangle {i} has non-negative cross Y: {crossY}");
            }
        }

        #endregion

        #region Larger Spacing Tests

        [Fact]
        public void Build_SpacingLargerThanLength_Returns2Points_4Vertices_2Triangles()
        {
            var surface = CreateFlatSurface();
            // Length 6, spacing 10 -> steps = ceil(6/10) = 1, points = 2
            var mesh = RoadRibbonBuilder.Build(surface, -3.0, 0.0, 3.0, 0.0, 2.0, 10.0, 0.0);

            Assert.Equal(4, mesh.VertexCount); // 2 * 2
            Assert.Equal(2, mesh.TriangleCount); // 2 * (2 - 1) = 2
        }

        #endregion

        #region Edge Clamping Tests

        [Fact]
        public void Build_SegmentAlongMaxZEdge_Clamped_NoException_ValidVerticesAndWinding()
        {
            var surface = CreateFlatSurface(); // extents -4..4
            // Segment along max Z edge: (-3, 4) -> (3, 4), width 2
            var mesh = RoadRibbonBuilder.Build(surface, -3.0, 4.0, 3.0, 4.0, 2.0, 1.0, 0.0);

            // No exception thrown (test passes if we get here)

            // Every vertex lies inside [MinX, MaxX] x [MinZ, MaxZ]
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                double x = mesh.Positions[i * 3];
                double z = mesh.Positions[i * 3 + 2];
                Assert.True(x >= surface.MinX - 1e-9 && x <= surface.MaxX + 1e-9, $"Vertex {i} X={x} outside bounds [{surface.MinX}, {surface.MaxX}]");
                Assert.True(z >= surface.MinZ - 1e-9 && z <= surface.MaxZ + 1e-9, $"Vertex {i} Z={z} outside bounds [{surface.MinZ}, {surface.MaxZ}]");
                Assert.True(double.IsFinite(mesh.Positions[i * 3 + 1]), $"Vertex {i} Y is not finite");
            }

            // Every triangle has Y cross component <= 1e-12 (degenerate triangles allowed)
            for (int i = 0; i < mesh.TriangleCount; i++)
            {
                int a = mesh.Indices[i * 3];
                int b = mesh.Indices[i * 3 + 1];
                int c = mesh.Indices[i * 3 + 2];
                double crossY = CrossY(mesh.Positions, a, b, c);
                Assert.True(crossY <= 1e-12, $"Triangle {i} has positive cross Y: {crossY}");
            }
        }

        [Fact]
        public void Build_SegmentAlongMinXEdge_NonBinarySurface_Clamped_NoException_ValidVerticesAndWinding()
        {
            var surface = CreateFlatEdgeSurface(); // 11x11, cellSize 0.1, Centered -> extents -0.5..0.5
            // Segment along x = MinX from z = -0.4 to z = 0.4, width 0.2
            var mesh = RoadRibbonBuilder.Build(surface, surface.MinX, -0.4, surface.MinX, 0.4, 0.2, 0.1, 0.0);

            // No exception thrown

            // Every vertex lies inside bounds
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                double x = mesh.Positions[i * 3];
                double z = mesh.Positions[i * 3 + 2];
                Assert.True(x >= surface.MinX - 1e-9 && x <= surface.MaxX + 1e-9, $"Vertex {i} X={x} outside bounds [{surface.MinX}, {surface.MaxX}]");
                Assert.True(z >= surface.MinZ - 1e-9 && z <= surface.MaxZ + 1e-9, $"Vertex {i} Z={z} outside bounds [{surface.MinZ}, {surface.MaxZ}]");
                Assert.True(double.IsFinite(mesh.Positions[i * 3 + 1]), $"Vertex {i} Y is not finite");
            }

            // Every triangle has Y cross component <= 1e-12
            for (int i = 0; i < mesh.TriangleCount; i++)
            {
                int a = mesh.Indices[i * 3];
                int b = mesh.Indices[i * 3 + 1];
                int c = mesh.Indices[i * 3 + 2];
                double crossY = CrossY(mesh.Positions, a, b, c);
                Assert.True(crossY <= 1e-12, $"Triangle {i} has positive cross Y: {crossY}");
            }
        }

        #endregion

        #region Wide Width Test

        [Fact]
        public void Build_VeryWideWidth_BuildsWithoutException_AllVerticesClamped()
        {
            var surface = CreateFlatSurface(); // extents -4..4
            // Width 1000 (much wider than map which is 8x8)
            var mesh = RoadRibbonBuilder.Build(surface, 0.0, 0.0, 3.0, 0.0, 1000.0, 1.0, 0.0);

            // No exception thrown

            // All vertices clamped inside bounds
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                double x = mesh.Positions[i * 3];
                double z = mesh.Positions[i * 3 + 2];
                Assert.True(x >= surface.MinX - 1e-9 && x <= surface.MaxX + 1e-9, $"Vertex {i} X={x} outside bounds [{surface.MinX}, {surface.MaxX}]");
                Assert.True(z >= surface.MinZ - 1e-9 && z <= surface.MaxZ + 1e-9, $"Vertex {i} Z={z} outside bounds [{surface.MinZ}, {surface.MaxZ}]");
                Assert.True(double.IsFinite(mesh.Positions[i * 3 + 1]), $"Vertex {i} Y is not finite");
            }
        }

        #endregion

        #region Determinism Test

        [Fact]
        public void Build_Deterministic_SameCallTwiceGivesIdenticalArrays()
        {
            var surface = CreateFlatSurface();
            var mesh1 = RoadRibbonBuilder.Build(surface, -3.0, 0.0, 3.0, 0.0, 2.0, 1.0, 0.15);
            var mesh2 = RoadRibbonBuilder.Build(surface, -3.0, 0.0, 3.0, 0.0, 2.0, 1.0, 0.15);

            Assert.Equal(mesh1.Positions.Length, mesh2.Positions.Length);
            Assert.Equal(mesh1.Normals.Length, mesh2.Normals.Length);
            Assert.Equal(mesh1.Indices.Length, mesh2.Indices.Length);

            for (int i = 0; i < mesh1.Positions.Length; i++)
            {
                Assert.Equal(mesh1.Positions[i], mesh2.Positions[i]);
            }
            for (int i = 0; i < mesh1.Normals.Length; i++)
            {
                Assert.Equal(mesh1.Normals[i], mesh2.Normals[i]);
            }
            for (int i = 0; i < mesh1.Indices.Length; i++)
            {
                Assert.Equal(mesh1.Indices[i], mesh2.Indices[i]);
            }
        }

        #endregion

        #region Fuzz Tests

        // DeterministicRandom copied from the codebase pattern
        private sealed class DeterministicRandom
        {
            private uint _state;

            public DeterministicRandom(uint seed)
            {
                _state = seed;
            }

            public uint NextUInt()
            {
                _state = _state * 1664525u + 1013904223u;
                return _state;
            }

            public double NextDouble()
            {
                return (double)NextUInt() / uint.MaxValue;
            }

            public int NextInt(int minInclusive, int maxExclusive)
            {
                return minInclusive + (int)(NextUInt() % (uint)(maxExclusive - minInclusive));
            }
        }

        private static TerrainSurface CreateFuzzSurface()
        {
            // Generate terrain from seed 12345, GeneratorVersion 1, TerrainSettings.Default, 64x64
            // We need to match the TerrainGenerator exactly
            var world = new WorldGenerationSettings(12345, 1, WorldSize.Small);
            var terrain = TerrainSettings.Default;
            var heightmap = TerrainGenerator.Generate(world, terrain, 64, 64);
            return TerrainSurface.Centered(heightmap, 1.0, 10.0);
        }

        private static double ChooseCoordinate(DeterministicRandom rng, double min, double max)
        {
            // 0 = min, 1 = max, 2 = uniform inside
            int choice = rng.NextInt(0, 3);
            if (choice == 0) return min;
            if (choice == 1) return max;
            return min + rng.NextDouble() * (max - min);
        }

        [Fact]
        public void Build_Fuzz_InteriorSegments_Seed8080_DeterministicAndValid()
        {
            var surface = CreateFuzzSurface();
            var rng = new DeterministicRandom(8080);
            double width = 1.6;
            double spacing = 1.0;
            double heightOffset = 0.15;

            var results1 = new System.Collections.Generic.List<RoadRibbonMesh>();
            var results2 = new System.Collections.Generic.List<RoadRibbonMesh>();

            for (int run = 0; run < 2; run++)
            {
                var currentResults = run == 0 ? results1 : results2;
                var currentRng = new DeterministicRandom(8080);

                for (int segment = 0; segment < 500; segment++)
                {
                    // INTERIOR: both endpoints uniform in [MinX+3, MaxX-3] x [MinZ+3, MaxZ-3]
                    double x1 = surface.MinX + 3.0 + currentRng.NextDouble() * (surface.MaxX - surface.MinX - 6.0);
                    double z1 = surface.MinZ + 3.0 + currentRng.NextDouble() * (surface.MaxZ - surface.MinZ - 6.0);
                    double x2 = surface.MinX + 3.0 + currentRng.NextDouble() * (surface.MaxX - surface.MinX - 6.0);
                    double z2 = surface.MinZ + 3.0 + currentRng.NextDouble() * (surface.MaxZ - surface.MinZ - 6.0);

                    double dx = x2 - x1;
                    double dz = z2 - z1;
                    double length = Math.Sqrt(dx * dx + dz * dz);

                    if (length < 1.0)
                    {
                        segment--;
                        continue;
                    }

                    var mesh = RoadRibbonBuilder.Build(surface, x1, z1, x2, z2, width, spacing, heightOffset);
                    currentResults.Add(mesh);

                    // Assert no throw (if we get here, no exception was thrown)

                    // VertexCount == 2 * points
                    int steps = Math.Max(1, (int)Math.Ceiling(length / spacing));
                    int points = steps + 1;
                    Assert.Equal(2 * points, mesh.VertexCount);

                    // TriangleCount == 2 * (points - 1)
                    Assert.Equal(2 * (points - 1), mesh.TriangleCount);

                    // All indices valid
                    foreach (int idx in mesh.Indices)
                    {
                        Assert.True(idx >= 0 && idx < mesh.VertexCount, $"Index {idx} out of bounds [0, {mesh.VertexCount})");
                    }

                    // All positions inside bounds and finite
                    for (int i = 0; i < mesh.VertexCount; i++)
                    {
                        double x = mesh.Positions[i * 3];
                        double y = mesh.Positions[i * 3 + 1];
                        double z = mesh.Positions[i * 3 + 2];

                        Assert.True(double.IsFinite(x) && double.IsFinite(y) && double.IsFinite(z), $"Vertex {i} has non-finite position");
                        Assert.True(x >= surface.MinX - 1e-9 && x <= surface.MaxX + 1e-9, $"Vertex {i} X={x} outside bounds");
                        Assert.True(z >= surface.MinZ - 1e-9 && z <= surface.MaxZ + 1e-9, $"Vertex {i} Z={z} outside bounds");
                    }

                    // All triangles STRICTLY negative winding
                    for (int i = 0; i < mesh.TriangleCount; i++)
                    {
                        int a = mesh.Indices[i * 3];
                        int b = mesh.Indices[i * 3 + 1];
                        int c = mesh.Indices[i * 3 + 2];
                        double crossY = CrossY(mesh.Positions, a, b, c);
                        Assert.True(crossY < 0, $"Triangle {i} has non-negative cross Y: {crossY}");
                    }

                    // Every vertex Y equals surface.SampleHeight(x, z) + 0.15 within 1e-6
                    // Use the float-rounded vertex x and z
                    for (int i = 0; i < mesh.VertexCount; i++)
                    {
                        float vx = mesh.Positions[i * 3];
                        float vz = mesh.Positions[i * 3 + 2];
                        float vy = mesh.Positions[i * 3 + 1];
                        double expectedY = surface.SampleHeight(vx, vz) + heightOffset;
                        Assert.Equal(expectedY, vy, 1e-6);
                    }
                }
            }

            // Assert identical arrays
            Assert.Equal(results1.Count, results2.Count);
            for (int i = 0; i < results1.Count; i++)
            {
                var m1 = results1[i];
                var m2 = results2[i];

                Assert.Equal(m1.Positions.Length, m2.Positions.Length);
                Assert.Equal(m1.Normals.Length, m2.Normals.Length);
                Assert.Equal(m1.Indices.Length, m2.Indices.Length);

                for (int j = 0; j < m1.Positions.Length; j++)
                {
                    Assert.Equal(m1.Positions[j], m2.Positions[j]);
                }
                for (int j = 0; j < m1.Normals.Length; j++)
                {
                    Assert.Equal(m1.Normals[j], m2.Normals[j]);
                }
                for (int j = 0; j < m1.Indices.Length; j++)
                {
                    Assert.Equal(m1.Indices[j], m2.Indices[j]);
                }
            }
        }

        [Fact]
        public void Build_Fuzz_EdgeSegments_Seed8080_DeterministicAndValid()
        {
            var surface = CreateFuzzSurface();
            var rng = new DeterministicRandom(8080);
            double width = 1.6;
            double spacing = 1.0;
            double heightOffset = 0.15;

            var results1 = new System.Collections.Generic.List<RoadRibbonMesh>();
            var results2 = new System.Collections.Generic.List<RoadRibbonMesh>();

            for (int run = 0; run < 2; run++)
            {
                var currentResults = run == 0 ? results1 : results2;
                var currentRng = new DeterministicRandom(8080);

                for (int segment = 0; segment < 500; segment++)
                {
                    // EDGE: endpoint coordinates chosen as exactly min, exactly max, or uniform inside
                    double x1 = ChooseCoordinate(currentRng, surface.MinX, surface.MaxX);
                    double z1 = ChooseCoordinate(currentRng, surface.MinZ, surface.MaxZ);
                    double x2 = ChooseCoordinate(currentRng, surface.MinX, surface.MaxX);
                    double z2 = ChooseCoordinate(currentRng, surface.MinZ, surface.MaxZ);

                    double dx = x2 - x1;
                    double dz = z2 - z1;
                    double length = Math.Sqrt(dx * dx + dz * dz);

                    if (length < 1.0)
                    {
                        segment--;
                        continue;
                    }

                    var mesh = RoadRibbonBuilder.Build(surface, x1, z1, x2, z2, width, spacing, heightOffset);
                    currentResults.Add(mesh);

                    // Assert no throw

                    // Valid indices
                    foreach (int idx in mesh.Indices)
                    {
                        Assert.True(idx >= 0 && idx < mesh.VertexCount, $"Index {idx} out of bounds [0, {mesh.VertexCount})");
                    }

                    // Positions inside bounds and finite
                    for (int i = 0; i < mesh.VertexCount; i++)
                    {
                        double x = mesh.Positions[i * 3];
                        double y = mesh.Positions[i * 3 + 1];
                        double z = mesh.Positions[i * 3 + 2];

                        Assert.True(double.IsFinite(x) && double.IsFinite(y) && double.IsFinite(z), $"Vertex {i} has non-finite position");
                        Assert.True(x >= surface.MinX - 1e-9 && x <= surface.MaxX + 1e-9, $"Vertex {i} X={x} outside bounds");
                        Assert.True(z >= surface.MinZ - 1e-9 && z <= surface.MaxZ + 1e-9, $"Vertex {i} Z={z} outside bounds");
                    }

                    // Winding <= 1e-12 for every triangle
                    for (int i = 0; i < mesh.TriangleCount; i++)
                    {
                        int a = mesh.Indices[i * 3];
                        int b = mesh.Indices[i * 3 + 1];
                        int c = mesh.Indices[i * 3 + 2];
                        double crossY = CrossY(mesh.Positions, a, b, c);
                        Assert.True(crossY <= 1e-12, $"Triangle {i} has positive cross Y: {crossY}");
                    }
                }
            }

            // Assert identical arrays
            Assert.Equal(results1.Count, results2.Count);
            for (int i = 0; i < results1.Count; i++)
            {
                var m1 = results1[i];
                var m2 = results2[i];

                Assert.Equal(m1.Positions.Length, m2.Positions.Length);
                Assert.Equal(m1.Normals.Length, m2.Normals.Length);
                Assert.Equal(m1.Indices.Length, m2.Indices.Length);

                for (int j = 0; j < m1.Positions.Length; j++)
                {
                    Assert.Equal(m1.Positions[j], m2.Positions[j]);
                }
                for (int j = 0; j < m1.Normals.Length; j++)
                {
                    Assert.Equal(m1.Normals[j], m2.Normals[j]);
                }
                for (int j = 0; j < m1.Indices.Length; j++)
                {
                    Assert.Equal(m1.Indices[j], m2.Indices[j]);
                }
            }
        }

        #endregion
    }
}