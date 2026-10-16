using System;
using Xunit;
using MedievalCityBuilder.Roads;

namespace MedievalCityBuilder.Tests
{
    public class RoadCostTests
    {
        [Theory]
        [InlineData(-1, 1, 2, 1, 1, 5)]
        [InlineData(1, -1, 2, 1, 1, 5)]
        [InlineData(1, 1, -1, 1, 1, 5)]
        [InlineData(1, 1, 2, -1, 1, 5)]
        [InlineData(1, 1, 2, 1, -1, 5)]
        [InlineData(1, 1, 2, 1, 1, -1)]
        public void Settings_NegativeField_ThrowsArgumentOutOfRangeException(
            long goldPerUnit,
            long stonePerUnit,
            long minGold,
            long minStone,
            long upkeepStone,
            int usagePercent)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RoadCostSettings(
                goldPerUnit, stonePerUnit, minGold, minStone, upkeepStone, usagePercent));
        }

        [Fact]
        public void Settings_DefaultValues_AreAsSpecified()
        {
            var def = RoadCostSettings.Default;
            Assert.Equal(2, def.GoldPerUnit);
            Assert.Equal(1, def.StonePerUnit);
            Assert.Equal(2, def.MinGold);
            Assert.Equal(1, def.MinStone);
            Assert.Equal(1, def.UpkeepStonePerUnitPerPeriod);
            Assert.Equal(5, def.UsageUpkeepPercent);
        }

        [Fact]
        public void Settings_EqualRecords_AreEqual()
        {
            var a = new RoadCostSettings(2, 1, 2, 1, 1, 5);
            var b = new RoadCostSettings(2, 1, 2, 1, 1, 5);
            Assert.Equal(a, b);
        }

        [Theory]
        [InlineData(0.0, 2, 1)]
        [InlineData(0.2, 2, 1)]
        [InlineData(1.0, 2, 1)]
        [InlineData(1.0001, 4, 2)]
        [InlineData(10.0, 20, 10)]
        [InlineData(10.5, 22, 11)]
        public void GetSegmentCost_DefaultSettings_ReturnsExpected(double length, long expectedGold, long expectedStone)
        {
            var cost = RoadCostCalculator.GetSegmentCost(RoadCostSettings.Default, length);
            Assert.Equal(expectedGold, cost.Gold);
            Assert.Equal(expectedStone, cost.Stone);
        }

        [Fact]
        public void GetSegmentCost_CustomMinimums_RespectsMinimums()
        {
            var settings = new RoadCostSettings(2, 1, 50, 30, 1, 5);
            // length 5 -> units 5 -> cost (10, 5), below min (50, 30) -> returns (50, 30)
            var costLow = RoadCostCalculator.GetSegmentCost(settings, 5.0);
            Assert.Equal(50, costLow.Gold);
            Assert.Equal(30, costLow.Stone);

            // length 30 -> units 30 -> cost (60, 30), exceeds min gold (50) and matches min stone (30) -> returns (60, 30)? Wait: stone for 30 units with StonePerUnit=1 is 30. MinStone is 30, so max(30, 30) = 30. Let's test length 33 -> 33 units -> gold 66, stone 33.
            var costHigh = RoadCostCalculator.GetSegmentCost(settings, 33.0);
            Assert.Equal(66, costHigh.Gold);
            Assert.Equal(33, costHigh.Stone);
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(-0.1)]
        [InlineData(-1.0)]
        [InlineData(1000001.0)]
        public void GetSegmentCost_InvalidLength_ThrowsArgumentOutOfRangeException(double length)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                RoadCostCalculator.GetSegmentCost(RoadCostSettings.Default, length));
        }

        [Fact]
        public void GetSegmentCost_MaxAllowedLength_Accepted()
        {
            var cost = RoadCostCalculator.GetSegmentCost(RoadCostSettings.Default, 1000000.0);
            Assert.True(cost.Gold > 0);
        }

        [Fact]
        public void GetSegmentCost_Overflow_ThrowsOverflowException()
        {
            var settings = new RoadCostSettings(long.MaxValue / 2, 1, 2, 1, 1, 5);
            Assert.Throws<OverflowException>(() =>
                RoadCostCalculator.GetSegmentCost(settings, 5.0));
        }

        [Fact]
        public void GraphOverloads_WorksCorrectly()
        {
            var graph = new RoadGraph();
            var n1 = graph.AddNode(0, 0);
            var n2 = graph.AddNode(3, 4);   // length 5
            var n3 = graph.AddNode(3, 14);  // length between n2 and n3 is 10

            var seg1 = graph.AddSegment(n1, n2);
            var seg2 = graph.AddSegment(n2, n3);

            // GetSegmentCost by id
            var cost1 = RoadCostCalculator.GetSegmentCost(RoadCostSettings.Default, graph, seg1);
            Assert.Equal(10, cost1.Gold); // 5 units * 2
            Assert.Equal(5, cost1.Stone);  // 5 units * 1

            var cost2 = RoadCostCalculator.GetSegmentCost(RoadCostSettings.Default, graph, seg2);
            Assert.Equal(20, cost2.Gold); // 10 units * 2
            Assert.Equal(10, cost2.Stone); // 10 units * 1

            // GetGraphCost equals sum
            var totalGraphCost = RoadCostCalculator.GetGraphCost(RoadCostSettings.Default, graph);
            Assert.Equal(cost1.Gold + cost2.Gold, totalGraphCost.Gold);
            Assert.Equal(cost1.Stone + cost2.Stone, totalGraphCost.Stone);

            // Empty graph gives Zero
            var emptyGraph = new RoadGraph();
            Assert.Equal(RoadCost.Zero, RoadCostCalculator.GetGraphCost(RoadCostSettings.Default, emptyGraph));

            // Unknown segment id throws ArgumentException
            Assert.Throws<ArgumentException>(() =>
                RoadCostCalculator.GetSegmentCost(RoadCostSettings.Default, graph, new RoadSegmentId(999)));

            // Null arguments throw ArgumentNullException
            Assert.Throws<ArgumentNullException>(() =>
                RoadCostCalculator.GetSegmentCost(null!, graph, seg1));
            Assert.Throws<ArgumentNullException>(() =>
                RoadCostCalculator.GetSegmentCost(RoadCostSettings.Default, null!, seg1));
            Assert.Throws<ArgumentNullException>(() =>
                RoadCostCalculator.GetGraphCost(null!, graph));
            Assert.Throws<ArgumentNullException>(() =>
                RoadCostCalculator.GetGraphCost(RoadCostSettings.Default, null!));
        }

        [Fact]
        public void Additivity_ChainOfSegments_GraphCostEqualsSum()
        {
            var graph = new RoadGraph();
            var n0 = graph.AddNode(0, 0);
            var n1 = graph.AddNode(2, 0); // len 2
            var n2 = graph.AddNode(5, 0); // len 3
            var n3 = graph.AddNode(5, 4); // len 4
            var n4 = graph.AddNode(8, 8); // len 5
            var n5 = graph.AddNode(20, 8); // len 12

            graph.AddSegment(n0, n1);
            graph.AddSegment(n1, n2);
            graph.AddSegment(n2, n3);
            graph.AddSegment(n3, n4);
            graph.AddSegment(n4, n5);

            RoadCost manualSum = RoadCost.Zero;
            foreach (var seg in graph.Segments)
            {
                manualSum = manualSum + RoadCostCalculator.GetSegmentCost(RoadCostSettings.Default, graph, seg.Id);
            }

            var graphCost = RoadCostCalculator.GetGraphCost(RoadCostSettings.Default, graph);
            Assert.Equal(manualSum, graphCost);
        }

        [Theory]
        [InlineData(10.0, 0, 10)] // base 10 + 0 = 10
        [InlineData(10.0, 1, 11)] // base 10 + ceil(10*5*1/100) = 10 + ceil(0.5) = 11
        [InlineData(10.0, 4, 12)] // base 10 + ceil(10*5*4/100) = 10 + ceil(2.0) = 12
        [InlineData(3.0, 1, 4)]   // base 3 + ceil(3*5*1/100) = 3 + ceil(0.15) = 4 (rounding up)
        public void GetSegmentUpkeepStone_ExpectedValues(double length, int usage, long expectedUpkeep)
        {
            long upkeep = RoadCostCalculator.GetSegmentUpkeepStone(RoadCostSettings.Default, length, usage);
            Assert.Equal(expectedUpkeep, upkeep);
        }

        [Fact]
        public void GetSegmentUpkeepStone_UpkeepNeverDecreasesWithUsage()
        {
            double length = 7.5;
            long previousUpkeep = RoadCostCalculator.GetSegmentUpkeepStone(RoadCostSettings.Default, length, 0);
            for (int usage = 1; usage <= 50; usage++)
            {
                long currentUpkeep = RoadCostCalculator.GetSegmentUpkeepStone(RoadCostSettings.Default, length, usage);
                Assert.True(currentUpkeep >= previousUpkeep, $"Upkeep at usage {usage} ({currentUpkeep}) decreased compared to usage {usage - 1} ({previousUpkeep}).");
                previousUpkeep = currentUpkeep;
            }
        }

        [Fact]
        public void GetSegmentUpkeepStone_ZeroPercentUsage_UsageIrrelevant()
        {
            var settings = new RoadCostSettings(2, 1, 2, 1, 3, 0); // 0% usage upkeep
            long u0 = RoadCostCalculator.GetSegmentUpkeepStone(settings, 10.0, 0);
            long u100 = RoadCostCalculator.GetSegmentUpkeepStone(settings, 10.0, 100);
            Assert.Equal(30, u0);
            Assert.Equal(30, u100);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void GetSegmentUpkeepStone_NegativeUsage_ThrowsArgumentOutOfRangeException(int negativeUsage)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                RoadCostCalculator.GetSegmentUpkeepStone(RoadCostSettings.Default, 5.0, negativeUsage));
        }

        [Fact]
        public void GetSegmentUpkeepStone_NullSettings_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                RoadCostCalculator.GetSegmentUpkeepStone(null!, 5.0, 1));
        }

        [Fact]
        public void GetGraphUpkeepStone_WorksCorrectly()
        {
            var graph = new RoadGraph();
            var n1 = graph.AddNode(0, 0);
            var n2 = graph.AddNode(3, 4);  // length 5 -> base 5, usage 0 -> 5
            var n3 = graph.AddNode(3, 14); // length 10 -> base 10, usage 2 -> base 10 + ceil(10*5*2/100) = 10 + 1 = 11

            var seg1 = graph.AddSegment(n1, n2);
            var seg2 = graph.AddSegment(n2, n3);

            long total = RoadCostCalculator.GetGraphUpkeepStone(RoadCostSettings.Default, graph, id =>
            {
                if (id == seg1) return 0;
                if (id == seg2) return 2;
                return 0;
            });

            Assert.Equal(5 + 11, total);

            // Empty graph returns 0 and callback is never invoked
            var emptyGraph = new RoadGraph();
            int callbackInvocations = 0;
            long emptyTotal = RoadCostCalculator.GetGraphUpkeepStone(RoadCostSettings.Default, emptyGraph, id =>
            {
                callbackInvocations++;
                return 5;
            });
            Assert.Equal(0, emptyTotal);
            Assert.Equal(0, callbackInvocations);

            // Callback returning negative value throws ArgumentOutOfRangeException
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                RoadCostCalculator.GetGraphUpkeepStone(RoadCostSettings.Default, graph, id => -1));

            // Null callback throws ArgumentNullException
            Assert.Throws<ArgumentNullException>(() =>
                RoadCostCalculator.GetGraphUpkeepStone(RoadCostSettings.Default, graph, null!));

            // Null graph or settings throws ArgumentNullException
            Assert.Throws<ArgumentNullException>(() =>
                RoadCostCalculator.GetGraphUpkeepStone(null!, graph, id => 0));
            Assert.Throws<ArgumentNullException>(() =>
                RoadCostCalculator.GetGraphUpkeepStone(RoadCostSettings.Default, null!, id => 0));
        }

        [Fact]
        public void Determinism_SameCallTwice_ReturnsEqualResults()
        {
            var graph = new RoadGraph();
            var n1 = graph.AddNode(1.5, 2.5);
            var n2 = graph.AddNode(6.5, 14.5);
            var seg = graph.AddSegment(n1, n2);

            var cost1 = RoadCostCalculator.GetSegmentCost(RoadCostSettings.Default, graph, seg);
            var cost2 = RoadCostCalculator.GetSegmentCost(RoadCostSettings.Default, graph, seg);
            Assert.Equal(cost1, cost2);

            var upkeep1 = RoadCostCalculator.GetSegmentUpkeepStone(RoadCostSettings.Default, 7.3, 3);
            var upkeep2 = RoadCostCalculator.GetSegmentUpkeepStone(RoadCostSettings.Default, 7.3, 3);
            Assert.Equal(upkeep1, upkeep2);

            var graphCost1 = RoadCostCalculator.GetGraphCost(RoadCostSettings.Default, graph);
            var graphCost2 = RoadCostCalculator.GetGraphCost(RoadCostSettings.Default, graph);
            Assert.Equal(graphCost1, graphCost2);

            var graphUpkeep1 = RoadCostCalculator.GetGraphUpkeepStone(RoadCostSettings.Default, graph, id => 4);
            var graphUpkeep2 = RoadCostCalculator.GetGraphUpkeepStone(RoadCostSettings.Default, graph, id => 4);
            Assert.Equal(graphUpkeep1, graphUpkeep2);
        }
    }
}