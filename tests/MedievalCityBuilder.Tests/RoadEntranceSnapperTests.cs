using System;
using System.Collections.Generic;
using Xunit;
using MedievalCityBuilder.Roads;

namespace MedievalCityBuilder.Tests
{
    public class RoadEntranceSnapperTests
    {
        private static readonly BuildingEntrancePoint EntranceA = new(10.0, 10.0, "A");
        private static readonly BuildingEntrancePoint EntranceB = new(20.0, 20.0, "B");
        private static readonly BuildingEntrancePoint EntranceC = new(15.0, 15.0, "C");

        [Fact]
        public void SnapToEntrance_SnapWithinRadius_ReturnsSnappedPoint()
        {
            var entrances = new List<BuildingEntrancePoint> { EntranceA };

            var result = RoadEntranceSnapper.SnapToEntrance(10.5, 10.5, entrances, 1.0);

            Assert.True(result.IsSnapped);
            Assert.Equal(EntranceA.X, result.X);
            Assert.Equal(EntranceA.Z, result.Z);
            Assert.True(result.SnappedEntrance.HasValue);
            Assert.Equal(EntranceA, result.SnappedEntrance.Value);
        }

        [Fact]
        public void SnapToEntrance_OutsideRadius_ReturnsOriginalPoint()
        {
            var entrances = new List<BuildingEntrancePoint> { EntranceA };

            var result = RoadEntranceSnapper.SnapToEntrance(20.0, 20.0, entrances, 1.0);

            Assert.False(result.IsSnapped);
            Assert.Equal(20.0, result.X);
            Assert.Equal(20.0, result.Z);
            Assert.False(result.SnappedEntrance.HasValue);
        }

        [Fact]
        public void SnapToEntrance_NearestEntranceWins()
        {
            var entrances = new List<BuildingEntrancePoint> { EntranceA, EntranceB };
            // Candidate is closer to A (10,10) than B (20,20)
            // Distance to A: sqrt((12-10)^2 + (12-10)^2) = ~2.8
            // Distance to B: sqrt((20-12)^2 + (20-12)^2) = ~11.3
            var result = RoadEntranceSnapper.SnapToEntrance(12.0, 12.0, entrances, 5.0);

            Assert.True(result.IsSnapped);
            Assert.Equal(EntranceA.X, result.X);
            Assert.Equal(EntranceA.Z, result.Z);
            Assert.Equal(EntranceA, result.SnappedEntrance.Value);
        }

        [Fact]
        public void SnapToEntrance_TieBehavior_FirstInListWins()
        {
            // Entrance at (10, 10) and (12, 10) - both are distance 1.0 from (11, 10)
            var entrance1 = new BuildingEntrancePoint(10.0, 10.0, "first");
            var entrance2 = new BuildingEntrancePoint(12.0, 10.0, "second");
            var entrances = new List<BuildingEntrancePoint> { entrance1, entrance2 };

            // Candidate at (11, 10) - exactly 1 unit from both
            var result = RoadEntranceSnapper.SnapToEntrance(11.0, 10.0, entrances, 2.0);

            Assert.True(result.IsSnapped);
            Assert.Equal(entrance1, result.SnappedEntrance.Value);
        }

        [Fact]
        public void SnapToEntrance_RadiusZero_DisablesSnapping()
        {
            var entrances = new List<BuildingEntrancePoint> { EntranceA };

            var result = RoadEntranceSnapper.SnapToEntrance(10.5, 10.5, entrances, 0.0);

            Assert.False(result.IsSnapped);
            Assert.Equal(10.5, result.X);
            Assert.Equal(10.5, result.Z);
        }

        [Fact]
        public void SnapToEntrance_NullEntrances_DisablesSnapping()
        {
            var result = RoadEntranceSnapper.SnapToEntrance(10.5, 10.5, null, 1.0);

            Assert.False(result.IsSnapped);
            Assert.Equal(10.5, result.X);
            Assert.Equal(10.5, result.Z);
        }

        [Fact]
        public void SnapToEntrance_EmptyEntrances_DisablesSnapping()
        {
            var entrances = new List<BuildingEntrancePoint>();

            var result = RoadEntranceSnapper.SnapToEntrance(10.5, 10.5, entrances, 1.0);

            Assert.False(result.IsSnapped);
            Assert.Equal(10.5, result.X);
            Assert.Equal(10.5, result.Z);
        }

        [Fact]
        public void SnapToEntrance_NaNCoordinates_ReturnsOriginal()
        {
            var entrances = new List<BuildingEntrancePoint> { EntranceA };

            var result = RoadEntranceSnapper.SnapToEntrance(double.NaN, 10.0, entrances, 1.0);

            Assert.False(result.IsSnapped);
            Assert.True(double.IsNaN(result.X));
            Assert.Equal(10.0, result.Z);
        }

        [Fact]
        public void SnapToEntrance_InfinityCoordinates_ReturnsOriginal()
        {
            var entrances = new List<BuildingEntrancePoint> { EntranceA };

            var result = RoadEntranceSnapper.SnapToEntrance(double.PositiveInfinity, 10.0, entrances, 1.0);

            Assert.False(result.IsSnapped);
            Assert.Equal(double.PositiveInfinity, result.X);
            Assert.Equal(10.0, result.Z);
        }

        [Fact]
        public void SnapToEntrance_EntranceWithNaN_IsSkipped()
        {
            var badEntrance = new BuildingEntrancePoint(double.NaN, 10.0, "bad");
            var goodEntrance = new BuildingEntrancePoint(10.0, 10.0, "good");
            var entrances = new List<BuildingEntrancePoint> { badEntrance, goodEntrance };

            var result = RoadEntranceSnapper.SnapToEntrance(10.5, 10.5, entrances, 1.0);

            Assert.True(result.IsSnapped);
            Assert.Equal(goodEntrance, result.SnappedEntrance.Value);
        }

        [Fact]
        public void SnapToEntrance_EntranceExactlyAtRadiusBoundary_IsSnapped()
        {
            var entrance = new BuildingEntrancePoint(10.0, 10.0, "exact");
            var entrances = new List<BuildingEntrancePoint> { entrance };

            // Distance = 1.0, radius = 1.0
            var result = RoadEntranceSnapper.SnapToEntrance(11.0, 10.0, entrances, 1.0);

            Assert.True(result.IsSnapped);
            Assert.Equal(entrance, result.SnappedEntrance.Value);
        }

        [Fact]
        public void SnapToEntrance_EntranceJustBeyondRadiusBoundary_IsNotSnapped()
        {
            var entrance = new BuildingEntrancePoint(10.0, 10.0, "beyond");
            var entrances = new List<BuildingEntrancePoint> { entrance };

            // Distance = 1.001, radius = 1.0
            var result = RoadEntranceSnapper.SnapToEntrance(11.001, 10.0, entrances, 1.0);

            Assert.False(result.IsSnapped);
            Assert.Equal(11.001, result.X);
            Assert.Equal(10.0, result.Z);
        }

        [Fact]
        public void SnapToEntrance_MultipleEntrancesAtSameDistance_FirstInListWins()
        {
            // Three entrances all at distance 1.0 from (11, 10)
            var e1 = new BuildingEntrancePoint(10.0, 10.0, "first");
            var e2 = new BuildingEntrancePoint(12.0, 10.0, "second");
            var e3 = new BuildingEntrancePoint(11.0, 11.0, "third");
            var entrances = new List<BuildingEntrancePoint> { e1, e2, e3 };

            var result = RoadEntranceSnapper.SnapToEntrance(11.0, 10.0, entrances, 2.0);

            Assert.True(result.IsSnapped);
            Assert.Equal(e1, result.SnappedEntrance.Value);
        }
    }
}