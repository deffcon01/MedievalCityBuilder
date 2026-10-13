using System;
using System.Collections.Generic;
using Xunit;
using MedievalCityBuilder.World;

namespace MedievalCityBuilder.Tests
{
    public class TerrainMeshDataTests
    {
        private static Heightmap CreateFlatHeightmap(int width, int height, float value = 0.5f)
        {
            var hm = new Heightmap(width, height);
            var data = hm.GetDataSpan();
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = value;
            }
            return hm;
        }

        private static Heightmap CreateSlopedHeightmap(int width, int height)
        {
            var hm = new Heightmap(width, height);
            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Height increases with x: h = x / (width - 1)
                    hm.Set(x, z, (float)x / (width - 1));
                }
            }
            return hm;
        }

        private static Heightmap CreateGeneratedHeightmap(ulong seed = 12345, int size = 64)
        {
            var world = new WorldGenerationSettings(seed, WorldGenerationSettings.CurrentGeneratorVersion, WorldSize.Small);
            var terrain = TerrainSettings.Default;
            // Override size to match the test size
            return TerrainGenerator.Generate(world, terrain, size, size);
        }

        [Fact]
        public void VertexCount_EqualsWidthTimesHeight()
        {
            var hm = CreateFlatHeightmap(5, 4);
            var mesh = TerrainMeshData.Build(hm, 1.0, 10.0);

            Assert.Equal(5 * 4, mesh.VertexCount);
        }

        [Fact]
        public void TriangleCount_EqualsCellsTimesTwo()
        {
            var hm = CreateFlatHeightmap(5, 4);
            var mesh = TerrainMeshData.Build(hm, 1.0, 10.0);

            Assert.Equal((5 - 1) * (4 - 1) * 2, mesh.TriangleCount);
        }

        [Fact]
        public void Indices_AreWithinVertexRange()
        {
            var hm = CreateFlatHeightmap(5, 4);
            var mesh = TerrainMeshData.Build(hm, 1.0, 10.0);

            foreach (int idx in mesh.Indices)
            {
                Assert.True(idx >= 0 && idx < mesh.VertexCount, $"Index {idx} out of range [0, {mesh.VertexCount})");
            }
        }

        [Fact]
        public void Positions_MatchHeightmapCellSizeAndHeightScale()
        {
            var hm = new Heightmap(3, 3);
            hm.Set(0, 0, 0.0f);
            hm.Set(1, 0, 0.5f);
            hm.Set(2, 0, 1.0f);
            hm.Set(0, 1, 0.2f);
            hm.Set(1, 1, 0.7f);
            hm.Set(2, 1, 0.4f);
            hm.Set(0, 2, 0.9f);
            hm.Set(1, 2, 0.1f);
            hm.Set(2, 2, 0.6f);

            double cellSize = 2.0;
            double heightScale = 5.0;
            var mesh = TerrainMeshData.Build(hm, cellSize, heightScale);

            // Check several vertices
            // Vertex (0,0): x=0*2=0, y=0*5=0, z=0*2=0
            Assert.Equal(0.0f, mesh.Positions[0], 6);
            Assert.Equal(0.0f, mesh.Positions[1], 6);
            Assert.Equal(0.0f, mesh.Positions[2], 6);

            // Vertex (1,0): x=1*2=2, y=0.5*5=2.5, z=0*2=0
            Assert.Equal(2.0f, mesh.Positions[3], 6);
            Assert.Equal(2.5f, mesh.Positions[4], 6);
            Assert.Equal(0.0f, mesh.Positions[5], 6);

            // Vertex (2,2): x=2*2=4, y=0.6*5=3.0, z=2*2=4
            int vi = (2 * 3 + 2) * 3;
            Assert.Equal(4.0f, mesh.Positions[vi], 6);
            Assert.Equal(3.0f, mesh.Positions[vi + 1], 6);
            Assert.Equal(4.0f, mesh.Positions[vi + 2], 6);
        }

        [Fact]
        public void FlatHeightmap_GivesUpwardNormals()
        {
            var hm = CreateFlatHeightmap(5, 4, 0.5f);
            var mesh = TerrainMeshData.Build(hm, 1.0, 10.0);

            foreach (int i in mesh.Normals)
            {
                // All normals should be (0, 1, 0)
                // We check in chunks of 3
            }

            for (int vi = 0; vi < mesh.VertexCount; vi++)
            {
                int ni = vi * 3;
                Assert.Equal(0.0f, mesh.Normals[ni], 6);     // nx
                Assert.Equal(1.0f, mesh.Normals[ni + 1], 6); // ny
                Assert.Equal(0.0f, mesh.Normals[ni + 2], 6); // nz
            }
        }

        [Fact]
        public void FlatHeightmap_TrianglesHaveNegativeYCrossProduct()
        {
            var hm = CreateFlatHeightmap(5, 4, 0.5f);
            var mesh = TerrainMeshData.Build(hm, 1.0, 10.0);

            for (int ti = 0; ti < mesh.TriangleCount; ti++)
            {
                int idx = ti * 3;
                int a = mesh.Indices[idx];
                int b = mesh.Indices[idx + 1];
                int c = mesh.Indices[idx + 2];

                // (b - a) x (c - a)
                float ax = mesh.Positions[a * 3];
                float ay = mesh.Positions[a * 3 + 1];
                float az = mesh.Positions[a * 3 + 2];

                float bx = mesh.Positions[b * 3] - ax;
                float by = mesh.Positions[b * 3 + 1] - ay;
                float bz = mesh.Positions[b * 3 + 2] - az;

                float cx = mesh.Positions[c * 3] - ax;
                float cy = mesh.Positions[c * 3 + 1] - ay;
                float cz = mesh.Positions[c * 3 + 2] - az;

                // Cross product Y component: bz * cx - bx * cz
                float crossY = bz * cx - bx * cz;

                Assert.True(crossY < 0, $"Triangle {ti} has crossY = {crossY}, expected < 0");
            }
        }

        [Fact]
        public void SlopedHeightmap_NormalsHaveNegativeXAndPositiveY()
        {
            var hm = CreateSlopedHeightmap(5, 5);
            var mesh = TerrainMeshData.Build(hm, 1.0, 10.0);

            for (int vi = 0; vi < mesh.VertexCount; vi++)
            {
                int ni = vi * 3;
                float nx = mesh.Normals[ni];
                float ny = mesh.Normals[ni + 1];
                float nz = mesh.Normals[ni + 2];

                // Normal should have negative X (since height increases with x, slope goes up in +x direction)
                Assert.True(nx < 0, $"Vertex {vi}: nx = {nx}, expected < 0");

                // Normal should have positive Y (pointing up)
                Assert.True(ny > 0, $"Vertex {vi}: ny = {ny}, expected > 0");

                // Length should be 1
                float len = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
                Assert.True(Math.Abs(len - 1.0f) < 1e-6f, $"Vertex {vi}: normal length = {len}, expected 1.0");
            }
        }

        [Fact]
        public void Build_IsDeterministic()
        {
            var hm = CreateGeneratedHeightmap(12345, 16);
            var mesh1 = TerrainMeshData.Build(hm, 1.5, 20.0);
            var mesh2 = TerrainMeshData.Build(hm, 1.5, 20.0);

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

        [Fact]
        public void NullHeightmap_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => TerrainMeshData.Build(null!, 1.0, 10.0));
        }

        [Fact]
        public void ZeroCellSize_ThrowsArgumentOutOfRangeException()
        {
            var hm = CreateFlatHeightmap(4, 4);
            Assert.Throws<ArgumentOutOfRangeException>(() => TerrainMeshData.Build(hm, 0.0, 10.0));
        }

        [Fact]
        public void NegativeCellSize_ThrowsArgumentOutOfRangeException()
        {
            var hm = CreateFlatHeightmap(4, 4);
            Assert.Throws<ArgumentOutOfRangeException>(() => TerrainMeshData.Build(hm, -1.0, 10.0));
        }

        [Fact]
        public void NegativeHeightScale_ThrowsArgumentOutOfRangeException()
        {
            var hm = CreateFlatHeightmap(4, 4);
            Assert.Throws<ArgumentOutOfRangeException>(() => TerrainMeshData.Build(hm, 1.0, -1.0));
        }

        [Fact]
        public void ZeroHeightScale_Works()
        {
            var hm = CreateFlatHeightmap(4, 4);
            var mesh = TerrainMeshData.Build(hm, 1.0, 0.0);
            // All Y positions should be 0
            for (int vi = 0; vi < mesh.VertexCount; vi++)
            {
                Assert.Equal(0.0f, mesh.Positions[vi * 3 + 1], 6);
            }
        }

        [Fact]
        public void GeneratedHeightmap_AllNormalsUnitLength()
        {
            var hm = CreateGeneratedHeightmap(12345, 64);
            var mesh = TerrainMeshData.Build(hm, 1.0, 10.0);

            for (int vi = 0; vi < mesh.VertexCount; vi++)
            {
                int ni = vi * 3;
                float nx = mesh.Normals[ni];
                float ny = mesh.Normals[ni + 1];
                float nz = mesh.Normals[ni + 2];

                float len = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
                Assert.True(Math.Abs(len - 1.0f) < 1e-6f, $"Vertex {vi}: normal length = {len}, expected 1.0");
            }
        }

        [Fact]
        public void GeneratedHeightmap_AllTrianglesHaveNegativeYCrossProduct()
        {
            var hm = CreateGeneratedHeightmap(12345, 64);
            var mesh = TerrainMeshData.Build(hm, 1.0, 10.0);

            for (int ti = 0; ti < mesh.TriangleCount; ti++)
            {
                int idx = ti * 3;
                int a = mesh.Indices[idx];
                int b = mesh.Indices[idx + 1];
                int c = mesh.Indices[idx + 2];

                float ax = mesh.Positions[a * 3];
                float ay = mesh.Positions[a * 3 + 1];
                float az = mesh.Positions[a * 3 + 2];

                float bx = mesh.Positions[b * 3] - ax;
                float by = mesh.Positions[b * 3 + 1] - ay;
                float bz = mesh.Positions[b * 3 + 2] - az;

                float cx = mesh.Positions[c * 3] - ax;
                float cy = mesh.Positions[c * 3 + 1] - ay;
                float cz = mesh.Positions[c * 3 + 2] - az;

                float crossY = bz * cx - bx * cz;

                Assert.True(crossY < 0, $"Triangle {ti} has crossY = {crossY}, expected < 0");
            }
        }

        [Fact]
        public void SmallHeightmap_5x4_HasCorrectCounts()
        {
            var hm = CreateFlatHeightmap(5, 4);
            var mesh = TerrainMeshData.Build(hm, 1.0, 10.0);

            Assert.Equal(20, mesh.VertexCount);
            Assert.Equal(24, mesh.TriangleCount); // (5-1)*(4-1)*2 = 4*3*2 = 24
        }

        [Fact]
        public void MediumHeightmap_16x16_HasCorrectCounts()
        {
            var hm = CreateFlatHeightmap(16, 16);
            var mesh = TerrainMeshData.Build(hm, 1.0, 10.0);

            Assert.Equal(256, mesh.VertexCount);
            Assert.Equal(450, mesh.TriangleCount); // (16-1)*(16-1)*2 = 15*15*2 = 450
        }
    }
}