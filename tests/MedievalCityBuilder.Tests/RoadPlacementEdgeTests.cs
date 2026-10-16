using System;
using Xunit;
using MedievalCityBuilder.World;
using MedievalCityBuilder.Roads;

namespace MedievalCityBuilder.Tests
{
    public class RoadPlacementEdgeTests
    {
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

        [Fact]
        public void FlatEdgeSegmentsCellSize0_1ValidNoThrow()
        {
            var heightmap = CreateFlatHeightmap(11, 11, 0.5f);
            var surface = TerrainSurface.Centered(heightmap, 0.1, 10.0);
            var water = new WaterSettings(0.35);
            var settings = RoadPlacementSettings.Default;

            double minX = surface.MinX;
            double maxX = surface.MaxX;
            double minZ = surface.MinZ;
            double maxZ = surface.MaxZ;

            // edges along X
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, minX, minZ, maxX, minZ).IsValid);
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, maxX, minZ, minX, minZ).IsValid);
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, minX, maxZ, maxX, maxZ).IsValid);
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, maxX, maxZ, minX, maxZ).IsValid);
            // edges along Z
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, minX, minZ, minX, maxZ).IsValid);
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, minX, maxZ, minX, minZ).IsValid);
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, maxX, minZ, maxX, maxZ).IsValid);
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, maxX, maxZ, maxX, minZ).IsValid);
            // corner to corner diagonal
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, minX, minZ, maxX, maxZ).IsValid);
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, maxX, maxZ, minX, minZ).IsValid);
        }

        [Fact]
        public void FlatEdgeSegmentsCellSize0_3NonSquareValidNoThrow()
        {
            var heightmap = CreateFlatHeightmap(13, 7, 0.5f);
            var surface = TerrainSurface.Centered(heightmap, 0.3, 10.0);
            var water = new WaterSettings(0.35);
            var settings = RoadPlacementSettings.Default;

            double minX = surface.MinX;
            double maxX = surface.MaxX;
            double minZ = surface.MinZ;
            double maxZ = surface.MaxZ;

            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, minX, minZ, maxX, minZ).IsValid);
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, maxX, minZ, minX, minZ).IsValid);
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, minX, maxZ, maxX, maxZ).IsValid);
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, maxX, maxZ, minX, maxZ).IsValid);
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, minX, minZ, minX, maxZ).IsValid);
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, minX, maxZ, minX, minZ).IsValid);
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, maxX, minZ, maxX, maxZ).IsValid);
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, maxX, maxZ, maxX, minZ).IsValid);
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, minX, minZ, maxX, maxZ).IsValid);
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, maxX, maxZ, minX, minZ).IsValid);
        }

        [Fact]
        public void FlatEdgeSegmentsExplicitConstructorValidNoThrow()
        {
            var heightmap = CreateFlatHeightmap(8, 9, 0.5f);
            var surface = new TerrainSurface(heightmap, 0.7, 10.0, 0.1, -0.3);
            var water = new WaterSettings(0.35);
            var settings = RoadPlacementSettings.Default;

            double minX = surface.MinX;
            double maxX = surface.MaxX;
            double minZ = surface.MinZ;
            double maxZ = surface.MaxZ;

            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, minX, minZ, maxX, minZ).IsValid);
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, maxX, maxZ, minX, minZ).IsValid);
            Assert.True(RoadPlacementValidator.ValidateSegment(surface, water, settings, minX, maxZ, minX, minZ).IsValid);
        }

        [Fact]
        public void FuzzSegmentsFlatSurfaceNoThrowAndSymmetric()
        {
            var rng = new DeterministicRandom(777);
            var heightmap = CreateFlatHeightmap(11, 11, 0.5f);
            var surface = TerrainSurface.Centered(heightmap, 0.1, 10.0);
            var water = new WaterSettings(0.35);
            var settings = RoadPlacementSettings.Default;

            double minX = surface.MinX;
            double maxX = surface.MaxX;
            double minZ = surface.MinZ;
            double maxZ = surface.MaxZ;

            bool hasValid = false;
            for (int i = 0; i < 3000; i++)
            {
                int modeX1 = rng.NextInt(0, 3);
                int modeZ1 = rng.NextInt(0, 3);
                int modeX2 = rng.NextInt(0, 3);
                int modeZ2 = rng.NextInt(0, 3);

                double x1 = modeX1 == 0 ? minX : modeX1 == 1 ? maxX : minX + rng.NextDouble() * (maxX - minX);
                double z1 = modeZ1 == 0 ? minZ : modeZ1 == 1 ? maxZ : minZ + rng.NextDouble() * (maxZ - minZ);
                double x2 = modeX2 == 0 ? minX : modeX2 == 1 ? maxX : minX + rng.NextDouble() * (maxX - minX);
                double z2 = modeZ2 == 0 ? minZ : modeZ2 == 1 ? maxZ : minZ + rng.NextDouble() * (maxZ - minZ);

                var r1 = RoadPlacementValidator.ValidateSegment(surface, water, settings, x1, z1, x2, z2);
                var r2 = RoadPlacementValidator.ValidateSegment(surface, water, settings, x2, z2, x1, z1);

                Assert.Equal(r1.IsValid, r2.IsValid);
                Assert.Equal(r1.Failure, r2.Failure);

                if (r1.IsValid)
                {
                    hasValid = true;
                }
            }

            Assert.True(hasValid);
        }

        [Fact]
        public void FuzzSegmentsTerrainSurfaceNoThrowAndSymmetric()
        {
            var rng = new DeterministicRandom(777);
            var world = new WorldGenerationSettings(12345, 1, WorldSize.Small);
            var terrain = TerrainSettings.Default;
            var heightmap = TerrainGenerator.Generate(world, terrain, 64, 64);
            var surface = TerrainSurface.Centered(heightmap, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            var settings = RoadPlacementSettings.Default;

            double minX = surface.MinX;
            double maxX = surface.MaxX;
            double minZ = surface.MinZ;
            double maxZ = surface.MaxZ;

            bool hasValid = false;
            bool hasInvalid = false;

            for (int i = 0; i < 3000; i++)
            {
                int modeX1 = rng.NextInt(0, 3);
                int modeZ1 = rng.NextInt(0, 3);
                int modeX2 = rng.NextInt(0, 3);
                int modeZ2 = rng.NextInt(0, 3);

                double x1 = modeX1 == 0 ? minX : modeX1 == 1 ? maxX : minX + rng.NextDouble() * (maxX - minX);
                double z1 = modeZ1 == 0 ? minZ : modeZ1 == 1 ? maxZ : minZ + rng.NextDouble() * (maxZ - minZ);
                double x2 = modeX2 == 0 ? minX : modeX2 == 1 ? maxX : minX + rng.NextDouble() * (maxX - minX);
                double z2 = modeZ2 == 0 ? minZ : modeZ2 == 1 ? maxZ : minZ + rng.NextDouble() * (maxZ - minZ);

                var r1 = RoadPlacementValidator.ValidateSegment(surface, water, settings, x1, z1, x2, z2);
                var r2 = RoadPlacementValidator.ValidateSegment(surface, water, settings, x2, z2, x1, z1);

                Assert.Equal(r1.IsValid, r2.IsValid);
                Assert.Equal(r1.Failure, r2.Failure);

                if (r1.IsValid)
                {
                    hasValid = true;
                }
                else
                {
                    hasInvalid = true;
                }
            }

            Assert.True(hasValid);
            Assert.True(hasInvalid);
        }

        [Fact]
        public void TinySpacingValidNoHang()
        {
            var heightmap = CreateFlatHeightmap(11, 11, 0.5f);
            var surface = TerrainSurface.Centered(heightmap, 0.1, 10.0);
            var water = new WaterSettings(0.35);
            var settings = new RoadPlacementSettings(0.5, 1e-9, 1.0);

            double x1 = surface.MinX;
            double z1 = surface.MinZ;
            double x2 = surface.MaxX;
            double z2 = surface.MaxZ;

            var result = RoadPlacementValidator.ValidateSegment(surface, water, settings, x1, z1, x2, z2);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void TerrainSurfaceCenteredNullThrows()
        {
            Assert.Throws<ArgumentNullException>(() => TerrainSurface.Centered(null!, 1.0, 10.0));
        }
    }
}
