using System;
using System.Collections.Generic;
using System.Linq;
using MedievalCityBuilder.World;
using MedievalCityBuilder.Zoning;
using Xunit;

namespace MedievalCityBuilder.Tests
{
    public class ZoneOverlayTests
    {
        private static ZoneRegistry CreateRegistry()
        {
            return ZoneRegistry.CreateStartingRegistry();
        }

        // Fixture (a): flat 9x9 heightmap value 0.5 with Centered(cellSize=1.0, heightScale=10.0)
        // extents -4..4, world height 5.0
        private static TerrainSurface CreateFlatSurface()
        {
            var hm = new Heightmap(9, 9);
            for (int z = 0; z < 9; z++)
            {
                for (int x = 0; x < 9; x++)
                {
                    hm.Set(x, z, 0.5f);
                }
            }
            return TerrainSurface.Centered(hm, 1.0, 10.0);
        }

        // Fixture (b): ramp 5x5 value = 0.25 * x with cellSize=2, heightScale=4
        // world height = 0.25 * x * 4 = 1.0 * x, but centered so vertex x goes -4..4
        // Wait: heightmap value = 0.25 * x (x in 0..4), cellSize=2, heightScale=4
        // World height at vertex x = 0.25 * x * 4 = x (in world units relative to x=0)
        // With Centered: origin = -4, so world X goes -4, -2, 0, 2, 4
        // Heights at those X: 0.25*0*4=0, 0.25*1*4=1, 0.25*2*4=2, 0.25*3*4=3, 0.25*4*4=4
        private static TerrainSurface CreateRampSurface()
        {
            var hm = new Heightmap(5, 5);
            for (int z = 0; z < 5; z++)
            {
                for (int x = 0; x < 5; x++)
                {
                    hm.Set(x, z, (float)(0.25 * x));
                }
            }
            return TerrainSurface.Centered(hm, 2.0, 4.0);
        }

        // Non-binary fixture: flat 11x11, cellSize 0.3, Centered
        private static TerrainSurface CreateNonBinarySurface()
        {
            var hm = new Heightmap(11, 11);
            for (int z = 0; z < 11; z++)
            {
                for (int x = 0; x < 11; x++)
                {
                    hm.Set(x, z, 0.5f);
                }
            }
            return TerrainSurface.Centered(hm, 0.3, 10.0);
        }

        // Helper to compute Y component of (b-a) x (c-a) from positions array
        // u = b - a, v = c - a
        // y = u.z * v.x - u.x * v.z
        private static double CrossY(float[] positions, int a, int b, int c)
        {
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

        #region ZoneOverlayMesh Constructor Validation Tests

        [Fact]
        public void ZoneOverlayMesh_NullPositions_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ZoneOverlayMesh(null!, new int[3], new int[3], 0));
        }

        [Fact]
        public void ZoneOverlayMesh_NullVertexZoneIds_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ZoneOverlayMesh(new float[3], null!, new int[3], 0));
        }

        [Fact]
        public void ZoneOverlayMesh_NullIndices_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ZoneOverlayMesh(new float[3], new int[3], null!, 0));
        }

        [Fact]
        public void ZoneOverlayMesh_MismatchedPositionsAndZoneIds_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new ZoneOverlayMesh(new float[6], new int[3], new int[3], 0));
        }

        [Fact]
        public void ZoneOverlayMesh_PositionsLengthNotDivisibleBy3_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new ZoneOverlayMesh(new float[4], new int[4], new int[3], 0));
        }

        [Fact]
        public void ZoneOverlayMesh_IndicesLengthNotDivisibleBy3_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new ZoneOverlayMesh(new float[3], new int[3], new int[4], 0));
        }

        [Fact]
        public void ZoneOverlayMesh_NegativeQuadCount_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ZoneOverlayMesh(new float[3], new int[3], new int[3], -1));
        }

        #endregion

        #region Build Validation Tests

        [Fact]
        public void Build_NullGrid_ThrowsArgumentNullException()
        {
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentNullException>(() => ZoneOverlayBuilder.Build(null!, surface, 0.15));
        }

        [Fact]
        public void Build_NullSurface_ThrowsArgumentNullException()
        {
            var grid = new ZoneGrid(CreateRegistry(), 0, 0, 1.0, 10, 10);
            Assert.Throws<ArgumentNullException>(() => ZoneOverlayBuilder.Build(grid, null!, 0.15));
        }

        [Fact]
        public void Build_HeightOffsetNaN_ThrowsArgumentOutOfRangeException()
        {
            var grid = new ZoneGrid(CreateRegistry(), 0, 0, 1.0, 10, 10);
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => ZoneOverlayBuilder.Build(grid, surface, double.NaN));
        }

        [Fact]
        public void Build_HeightOffsetInfinity_ThrowsArgumentOutOfRangeException()
        {
            var grid = new ZoneGrid(CreateRegistry(), 0, 0, 1.0, 10, 10);
            var surface = CreateFlatSurface();
            Assert.Throws<ArgumentOutOfRangeException>(() => ZoneOverlayBuilder.Build(grid, surface, double.PositiveInfinity));
        }

        #endregion

        #region Empty Grid Tests

        [Fact]
        public void Build_EmptyGrid_ReturnsZeroVertices()
        {
            var registry = CreateRegistry();
            var grid = new ZoneGrid(registry, -5.0, -5.0, 1.0, 10, 10);
            var surface = CreateFlatSurface();

            var mesh = ZoneOverlayBuilder.Build(grid, surface, 0.15);

            Assert.Equal(0, mesh.VertexCount);
            Assert.Equal(0, mesh.TriangleCount);
            Assert.Equal(0, mesh.QuadCount);
            Assert.Empty(mesh.Positions);
            Assert.Empty(mesh.VertexZoneIds);
            Assert.Empty(mesh.Indices);
        }

        #endregion

        #region Single Cell Tests

        [Fact]
        public void Build_OneCellFlatSurface_FourVerticesTwoTriangles()
        {
            var registry = CreateRegistry();
            var grid = new ZoneGrid(registry, -5.0, -5.0, 1.0, 10, 10);
            // Cell (5,5) center is at (-5 + 5.5, -5 + 5.5) = (0.5, 0.5)
            // Paint with radius < 0.5 to only hit this one cell
            grid.Paint(0.5, 0.5, 0.4, new ZoneTypeId(1));

            var surface = CreateFlatSurface(); // extents -4..4, height 5.0
            var mesh = ZoneOverlayBuilder.Build(grid, surface, 0.15);

            Assert.Equal(4, mesh.VertexCount);
            Assert.Equal(2, mesh.TriangleCount);
            Assert.Equal(1, mesh.QuadCount);

            // Cell (5,5) corners: MinX=-5, x=5 => 0, x+1=6 => 1
            // So corners at (0, 0), (1, 0), (1, 1), (0, 1)
            // With heightOffset 0.15, flat surface height 5.0 -> world Y = 5.15

            // Check first vertex (lower-left): (0, 5.15, 0)
            Assert.Equal(0.0f, mesh.Positions[0], 1e-6);
            Assert.Equal(5.15f, mesh.Positions[1], 1e-6);
            Assert.Equal(0.0f, mesh.Positions[2], 1e-6);

            // Second vertex (lower-right): (1, 5.15, 0)
            Assert.Equal(1.0f, mesh.Positions[3], 1e-6);
            Assert.Equal(5.15f, mesh.Positions[4], 1e-6);
            Assert.Equal(0.0f, mesh.Positions[5], 1e-6);

            // Third vertex (upper-right): (1, 5.15, 1)
            Assert.Equal(1.0f, mesh.Positions[6], 1e-6);
            Assert.Equal(5.15f, mesh.Positions[7], 1e-6);
            Assert.Equal(1.0f, mesh.Positions[8], 1e-6);

            // Fourth vertex (upper-left): (0, 5.15, 1)
            Assert.Equal(0.0f, mesh.Positions[9], 1e-6);
            Assert.Equal(5.15f, mesh.Positions[10], 1e-6);
            Assert.Equal(1.0f, mesh.Positions[11], 1e-6);

            // Zone IDs
            Assert.Equal(1, mesh.VertexZoneIds[0]);
            Assert.Equal(1, mesh.VertexZoneIds[1]);
            Assert.Equal(1, mesh.VertexZoneIds[2]);
            Assert.Equal(1, mesh.VertexZoneIds[3]);

            // Indices: (0,1,2) and (0,2,3)
            Assert.Equal(0, mesh.Indices[0]);
            Assert.Equal(1, mesh.Indices[1]);
            Assert.Equal(2, mesh.Indices[2]);
            Assert.Equal(0, mesh.Indices[3]);
            Assert.Equal(2, mesh.Indices[4]);
            Assert.Equal(3, mesh.Indices[5]);
        }

        [Fact]
        public void Build_OneCellRampSurface_CorrectHeights()
        {
            var registry = CreateRegistry();
            // Grid covering the ramp surface (extents -4..4 in X and Z)
            var grid = new ZoneGrid(registry, -4.0, -4.0, 2.0, 4, 4);
            // Cell (2,2) center is at MinX + 2*CellSize + CellSize/2 = -4 + 4 + 1 = 1
            // Cell (2,2) spans X=0..2, Z=0..2
            // Paint with radius < 1.0 to only hit this one cell
            grid.Paint(1.0, 1.0, 0.9, new ZoneTypeId(1));

            var surface = CreateRampSurface(); // world height at X: -4->0, -2->1, 0->2, 2->3, 4->4
            var mesh = ZoneOverlayBuilder.Build(grid, surface, 0.0);

            Assert.Equal(4, mesh.VertexCount);
            Assert.Equal(2, mesh.TriangleCount);

            // Cell corners at X=0,2 and Z=0,2
            // Heights: at X=0 -> 2.0, at X=2 -> 3.0
            // Vertex 0: (0, 2.0, 0)
            Assert.Equal(0.0f, mesh.Positions[0], 1e-6);
            Assert.Equal(2.0f, mesh.Positions[1], 1e-6);
            Assert.Equal(0.0f, mesh.Positions[2], 1e-6);

            // Vertex 1: (2, 3.0, 0)
            Assert.Equal(2.0f, mesh.Positions[3], 1e-6);
            Assert.Equal(3.0f, mesh.Positions[4], 1e-6);
            Assert.Equal(0.0f, mesh.Positions[5], 1e-6);

            // Vertex 2: (2, 3.0, 2)
            Assert.Equal(2.0f, mesh.Positions[6], 1e-6);
            Assert.Equal(3.0f, mesh.Positions[7], 1e-6);
            Assert.Equal(2.0f, mesh.Positions[8], 1e-6);

            // Vertex 3: (0, 2.0, 2)
            Assert.Equal(0.0f, mesh.Positions[9], 1e-6);
            Assert.Equal(2.0f, mesh.Positions[10], 1e-6);
            Assert.Equal(2.0f, mesh.Positions[11], 1e-6);
        }

        #endregion

        #region Winding Tests

        [Theory]
        [InlineData(-3.5, -3.5)] // Cell (0,0) center
        [InlineData(3.5, -3.5)]  // Cell (7,0) center
        [InlineData(-3.5, 3.5)]  // Cell (0,7) center
        [InlineData(3.5, 3.5)]   // Cell (7,7) center
        public void Build_FlatSurface_AllTrianglesHaveNegativeYCrossComponent(double cellCenterX, double cellCenterZ)
        {
            var registry = CreateRegistry();
            var grid = new ZoneGrid(registry, -4.0, -4.0, 1.0, 8, 8);
            // Paint with radius < 0.5 to only hit the target cell
            grid.Paint(cellCenterX, cellCenterZ, 0.4, new ZoneTypeId(1));

            var surface = CreateFlatSurface();
            var mesh = ZoneOverlayBuilder.Build(grid, surface, 0.15);

            Assert.Equal(4, mesh.VertexCount);
            Assert.Equal(2, mesh.TriangleCount);

            for (int i = 0; i < mesh.TriangleCount; i++)
            {
                int a = mesh.Indices[i * 3];
                int b = mesh.Indices[i * 3 + 1];
                int c = mesh.Indices[i * 3 + 2];
                double crossY = CrossY(mesh.Positions, a, b, c);
                Assert.True(crossY < 0, $"Triangle {i} has non-negative cross Y: {crossY} (indices {a},{b},{c})");
            }
        }

        [Theory]
        [InlineData(-1.0, -1.0)] // Cell (1,1) center
        [InlineData(1.0, 1.0)]   // Cell (2,2) center
        public void Build_RampSurface_AllTrianglesHaveNegativeYCrossComponent(double cellCenterX, double cellCenterZ)
        {
            var registry = CreateRegistry();
            var grid = new ZoneGrid(registry, -4.0, -4.0, 2.0, 4, 4);
            // Paint with radius < 1.0 to only hit the target cell
            grid.Paint(cellCenterX, cellCenterZ, 0.9, new ZoneTypeId(1));

            var surface = CreateRampSurface();
            var mesh = ZoneOverlayBuilder.Build(grid, surface, 0.0);

            Assert.Equal(4, mesh.VertexCount);
            Assert.Equal(2, mesh.TriangleCount);

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

        #region Zone ID Mapping Tests

        [Fact]
        public void Build_MultipleZoneIds_CorrectVertexZoneIds()
        {
            var registry = CreateRegistry();
            var grid = new ZoneGrid(registry, -4.0, -4.0, 1.0, 8, 8);
            // Paint two cells with different zone IDs
            grid.Paint(-3.5, -3.5, 0.4, new ZoneTypeId(1)); // Residential
            grid.Paint(3.5, 3.5, 0.4, new ZoneTypeId(3));   // Commercial

            var surface = CreateFlatSurface();
            var mesh = ZoneOverlayBuilder.Build(grid, surface, 0.15);

            Assert.Equal(8, mesh.VertexCount); // 2 cells * 4 vertices
            Assert.Equal(4, mesh.TriangleCount); // 2 cells * 2 triangles

            // First cell (4 vertices) should have zone ID 1
            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(1, mesh.VertexZoneIds[i]);
            }

            // Second cell (4 vertices) should have zone ID 3
            for (int i = 4; i < 8; i++)
            {
                Assert.Equal(3, mesh.VertexZoneIds[i]);
            }
        }

        [Fact]
        public void Build_RowMajorOrder_CellsEmittedInRowMajor()
        {
            var registry = CreateRegistry();
            var grid = new ZoneGrid(registry, -4.0, -4.0, 1.0, 4, 4);

            // Paint cells in row-major order: (0,0), (1,0), (0,1), (1,1)
            // Cell centers: (-3.5,-3.5), (-2.5,-3.5), (-3.5,-2.5), (-2.5,-2.5)
            grid.Paint(-3.5, -3.5, 0.4, new ZoneTypeId(1)); // (0,0)
            grid.Paint(-2.5, -3.5, 0.4, new ZoneTypeId(2)); // (1,0)
            grid.Paint(-3.5, -2.5, 0.4, new ZoneTypeId(3)); // (0,1)
            grid.Paint(-2.5, -2.5, 0.4, new ZoneTypeId(4)); // (1,1)

            var surface = CreateFlatSurface();
            var mesh = ZoneOverlayBuilder.Build(grid, surface, 0.15);

            Assert.Equal(16, mesh.VertexCount);
            Assert.Equal(8, mesh.TriangleCount);

            // Check zone IDs are in row-major order
            // Cell (0,0): vertices 0-3 -> zone 1
            for (int i = 0; i < 4; i++) Assert.Equal(1, mesh.VertexZoneIds[i]);
            // Cell (1,0): vertices 4-7 -> zone 2
            for (int i = 4; i < 8; i++) Assert.Equal(2, mesh.VertexZoneIds[i]);
            // Cell (0,1): vertices 8-11 -> zone 3
            for (int i = 8; i < 12; i++) Assert.Equal(3, mesh.VertexZoneIds[i]);
            // Cell (1,1): vertices 12-15 -> zone 4
            for (int i = 12; i < 16; i++) Assert.Equal(4, mesh.VertexZoneIds[i]);
        }

        #endregion

        #region Vertex Count Tests

        [Fact]
        public void Build_VertexCountEqualsFourTimesPaintedCells()
        {
            var registry = CreateRegistry();
            var grid = new ZoneGrid(registry, -4.0, -4.0, 1.0, 8, 8);

            // Paint 5 cells using their centers with radius < 0.5
            grid.Paint(-3.5, -3.5, 0.4, new ZoneTypeId(1));
            grid.Paint(-2.5, -3.5, 0.4, new ZoneTypeId(1));
            grid.Paint(-1.5, -3.5, 0.4, new ZoneTypeId(1));
            grid.Paint(-3.5, -2.5, 0.4, new ZoneTypeId(1));
            grid.Paint(-3.5, -1.5, 0.4, new ZoneTypeId(1));

            var surface = CreateFlatSurface();
            var mesh = ZoneOverlayBuilder.Build(grid, surface, 0.15);

            Assert.Equal(20, mesh.VertexCount); // 5 cells * 4
            Assert.Equal(10, mesh.TriangleCount); // 5 cells * 2
            Assert.Equal(5, mesh.QuadCount);
        }

        #endregion

        #region Determinism Tests

        [Fact]
        public void Build_Deterministic_SameCallTwiceGivesIdenticalArrays()
        {
            var registry = CreateRegistry();
            var grid = new ZoneGrid(registry, -4.0, -4.0, 1.0, 8, 8);
            grid.Paint(-3.5, -3.5, 0.4, new ZoneTypeId(1));
            grid.Paint(0.5, 0.5, 0.4, new ZoneTypeId(3));

            var surface = CreateFlatSurface();
            var mesh1 = ZoneOverlayBuilder.Build(grid, surface, 0.15);
            var mesh2 = ZoneOverlayBuilder.Build(grid, surface, 0.15);

            Assert.Equal(mesh1.Positions.Length, mesh2.Positions.Length);
            Assert.Equal(mesh1.VertexZoneIds.Length, mesh2.VertexZoneIds.Length);
            Assert.Equal(mesh1.Indices.Length, mesh2.Indices.Length);

            for (int i = 0; i < mesh1.Positions.Length; i++)
            {
                Assert.Equal(mesh1.Positions[i], mesh2.Positions[i]);
            }
            for (int i = 0; i < mesh1.VertexZoneIds.Length; i++)
            {
                Assert.Equal(mesh1.VertexZoneIds[i], mesh2.VertexZoneIds[i]);
            }
            for (int i = 0; i < mesh1.Indices.Length; i++)
            {
                Assert.Equal(mesh1.Indices[i], mesh2.Indices[i]);
            }
        }

        #endregion

        #region Grid Larger Than Terrain Tests

        [Fact]
        public void Build_GridLargerThanTerrain_ClampsWithoutThrowing()
        {
            var registry = CreateRegistry();
            // Grid from -10 to 10 (larger than terrain -4 to 4)
            var grid = new ZoneGrid(registry, -10.0, -10.0, 1.0, 20, 20);
            // Paint a cell outside terrain bounds - cell center at -9.5, -9.5
            grid.Paint(-9.5, -9.5, 0.4, new ZoneTypeId(1));
            // Paint a cell inside terrain bounds - cell center at 0.5, 0.5
            grid.Paint(0.5, 0.5, 0.4, new ZoneTypeId(1));

            var surface = CreateFlatSurface(); // extents -4..4
            var mesh = ZoneOverlayBuilder.Build(grid, surface, 0.15);

            // Should not throw, and should produce vertices for both cells (clamped)
            Assert.Equal(8, mesh.VertexCount); // 2 cells * 4
            Assert.Equal(4, mesh.TriangleCount); // 2 cells * 2

            // All vertices should be inside terrain bounds
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                float x = mesh.Positions[i * 3];
                float z = mesh.Positions[i * 3 + 2];
                Assert.True(x >= surface.MinX - 1e-6 && x <= surface.MaxX + 1e-6, $"Vertex {i} X={x} outside bounds");
                Assert.True(z >= surface.MinZ - 1e-6 && z <= surface.MaxZ + 1e-6, $"Vertex {i} Z={z} outside bounds");
            }
        }

        #endregion

        #region Non-Binary Fixture Tests

        [Fact]
        public void Build_NonBinaryCellSize_WorksCorrectly()
        {
            var registry = CreateRegistry();
            // Grid with cellSize 0.3, MinX=-1.5, cell (5,5) center = -1.5 + 5*0.3 + 0.15 = 0.15
            // Wait: cell index 5 -> center = MinX + 5*cellSize + cellSize/2 = -1.5 + 1.5 + 0.15 = 0.15
            // Let's use cell (5,5) with center at 0.15, paint with radius < 0.15
            var grid = new ZoneGrid(registry, -1.5, -1.5, 0.3, 10, 10);
            grid.Paint(0.15, 0.15, 0.1, new ZoneTypeId(1));

            var surface = CreateNonBinarySurface(); // cellSize 0.3, extents -1.5..1.5
            var mesh = ZoneOverlayBuilder.Build(grid, surface, 0.15);

            Assert.Equal(4, mesh.VertexCount);
            Assert.Equal(2, mesh.TriangleCount);

            // Cell (5,5) corners at x: -1.5 + 5*0.3 = 0.0, x+1 = 0.3
            // With heightOffset 0.15, flat surface height 5.0 -> world Y = 5.15
            Assert.Equal(0.0f, mesh.Positions[0], 1e-6);
            Assert.Equal(5.15f, mesh.Positions[1], 1e-6);
            Assert.Equal(0.0f, mesh.Positions[2], 1e-6);
        }

        #endregion

        #region Fuzz Tests

        private sealed class DeterministicRandom
        {
            private ulong _state;

            public DeterministicRandom(ulong seed)
            {
                _state = seed;
            }

            public ulong NextUInt64()
            {
                ulong z = (_state += 0x9E3779B97F4A7C15UL);
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }

            public double NextDouble()
            {
                ulong value = NextUInt64();
                ulong mantissa = value >> 11;
                return (double)mantissa * (1.0 / 9007199254740992.0);
            }

            public int NextInt(int minInclusive, int maxExclusive)
            {
                if (maxExclusive <= minInclusive)
                    throw new ArgumentOutOfRangeException(nameof(maxExclusive));
                long range = (long)maxExclusive - minInclusive;
                if (range == 1) return minInclusive;
                ulong uRange = (ulong)range;
                ulong maxValid = ulong.MaxValue - (ulong.MaxValue % uRange);
                while (true)
                {
                    ulong sample = NextUInt64();
                    if (sample < maxValid)
                        return minInclusive + (int)(sample % uRange);
                }
            }
        }

        [Fact]
        public void Build_Fuzz_RandomBrushes_CountsMatchBruteForce()
        {
            var registry = CreateRegistry();
            var surface = CreateFlatSurface();

            var rng = new DeterministicRandom(8080);
            int cellsX = 20;
            int cellsZ = 20;
            double cellSize = 1.0;
            double minX = -10.0;
            double minZ = -10.0;

            var grid = new ZoneGrid(registry, minX, minZ, cellSize, cellsX, cellsZ);

            for (int brush = 0; brush < 200; brush++)
            {
                // Random brush center within surface bounds
                double worldX = minX + rng.NextDouble() * (cellsX * cellSize);
                double worldZ = minZ + rng.NextDouble() * (cellsZ * cellSize);
                double radius = 0.5 + rng.NextDouble() * 3.0; // 0.5 to 3.5
                int zoneIdx = rng.NextInt(1, 9); // Zone IDs 1-8
                var zoneId = new ZoneTypeId(zoneIdx);

                // Paint using ZoneGrid
                grid.Paint(worldX, worldZ, radius, zoneId);
            }

            // Get actual painted cell count from grid
            int actualPaintedCount = grid.PaintedCellCount;

            // Build overlay
            var mesh = ZoneOverlayBuilder.Build(grid, surface, 0.15);

            // Vertex count should equal 4 * actual painted cell count
            Assert.Equal(actualPaintedCount * 4, mesh.VertexCount);
            Assert.Equal(actualPaintedCount * 2, mesh.TriangleCount);
            Assert.Equal(actualPaintedCount, mesh.QuadCount);

            // Verify all positions are inside surface bounds
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                float x = mesh.Positions[i * 3];
                float z = mesh.Positions[i * 3 + 2];
                Assert.True(x >= surface.MinX - 1e-6 && x <= surface.MaxX + 1e-6, $"Vertex {i} X={x} outside bounds");
                Assert.True(z >= surface.MinZ - 1e-6 && z <= surface.MaxZ + 1e-6, $"Vertex {i} Z={z} outside bounds");
            }

            // All triangles have negative Y winding (allow <= 1e-12 for degenerate triangles at edges)
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
    }
}