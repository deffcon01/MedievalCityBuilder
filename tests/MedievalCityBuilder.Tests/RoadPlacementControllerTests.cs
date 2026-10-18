using System;
using System.Collections.Generic;
using Xunit;
using MedievalCityBuilder.World;
using MedievalCityBuilder.Roads;

namespace MedievalCityBuilder.Tests
{
    public class RoadPlacementControllerTests
    {
        private sealed class FakeBudget : IRoadBudget
        {
            public long Gold { get; set; }
            public long Stone { get; set; }
            public List<RoadCost> SuccessfulSpends { get; } = new();
            public int CanAffordCalls { get; private set; }
            public int TrySpendCalls { get; private set; }

            public FakeBudget(long gold, long stone)
            {
                Gold = gold;
                Stone = stone;
            }

            public bool CanAfford(RoadCost cost)
            {
                CanAffordCalls++;
                return Gold >= cost.Gold && Stone >= cost.Stone;
            }

            public bool TrySpend(RoadCost cost)
            {
                TrySpendCalls++;
                if (CanAfford(cost))
                {
                    // Subtract CanAffordCalls extra increment because CanAfford is called inside CanAfford
                    CanAffordCalls--; 
                    Gold -= cost.Gold;
                    Stone -= cost.Stone;
                    SuccessfulSpends.Add(cost);
                    return true;
                }
                return false;
            }
        }

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

        private static Heightmap CreateUnderwaterStripHeightmap(int width, int height, float dryValue, float wetValue, int wetStartZ, int wetEndZ)
        {
            var hm = new Heightmap(width, height);
            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (z >= wetStartZ && z <= wetEndZ)
                    {
                        hm.Set(x, z, wetValue);
                    }
                    else
                    {
                        hm.Set(x, z, dryValue);
                    }
                }
            }
            return hm;
        }

        [Fact]
        public void ToolSettings_ValidationAndDefaults()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RoadToolSettings(-0.1, true));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RoadToolSettings(double.NaN, true));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RoadToolSettings(double.PositiveInfinity, true));

            var def = RoadToolSettings.Default;
            Assert.Equal(1.5, def.SnapRadius);
            Assert.True(def.ContinueChain);
        }

        [Fact]
        public void Constructor_ArgumentNullChecks()
        {
            var graph = new RoadGraph();
            var hm = CreateFlatHeightmap(33, 33, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            var placement = RoadPlacementSettings.Default;
            var cost = RoadCostSettings.Default;
            var tool = RoadToolSettings.Default;

            Assert.Throws<ArgumentNullException>(() => new RoadPlacementController(null!, surface, water, placement, cost, tool, null));
            Assert.Throws<ArgumentNullException>(() => new RoadPlacementController(graph, null!, water, placement, cost, tool, null));
            Assert.Throws<ArgumentNullException>(() => new RoadPlacementController(graph, surface, null!, placement, cost, tool, null));
            Assert.Throws<ArgumentNullException>(() => new RoadPlacementController(graph, surface, water, null!, cost, tool, null));
            Assert.Throws<ArgumentNullException>(() => new RoadPlacementController(graph, surface, water, placement, null!, tool, null));
            Assert.Throws<ArgumentNullException>(() => new RoadPlacementController(graph, surface, water, placement, cost, null!, null));
            
            // Null budget is allowed (roads free)
            var controller = new RoadPlacementController(graph, surface, water, placement, cost, tool, null);
            Assert.NotNull(controller);
        }

        [Fact]
        public void FirstClick_ValidTerrain_SuccessZeroCostPendingStart()
        {
            var graph = new RoadGraph();
            var hm = CreateFlatHeightmap(33, 33, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            var controller = new RoadPlacementController(graph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, null);

            Assert.False(controller.HasPendingStart);
            Assert.Throws<InvalidOperationException>(() => controller.PendingStartX);
            Assert.Throws<InvalidOperationException>(() => controller.PendingStartZ);
            Assert.Null(controller.PendingStartNode);

            var result = controller.Click(0.0, 0.0);
            Assert.True(result.Success);
            Assert.Equal(RoadToolFailure.None, result.Failure);
            Assert.Equal(RoadCost.Zero, result.Cost);
            Assert.True(controller.HasPendingStart);
            Assert.Equal(0.0, controller.PendingStartX);
            Assert.Equal(0.0, controller.PendingStartZ);
            Assert.Null(controller.PendingStartNode);
            Assert.Equal(0, graph.NodeCount);
        }

        [Fact]
        public void FirstClick_OutOfBounds_FailTerrain()
        {
            var graph = new RoadGraph();
            var hm = CreateFlatHeightmap(33, 33, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            var controller = new RoadPlacementController(graph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, null);

            var result = controller.Click(999.0, 999.0);
            Assert.False(result.Success);
            Assert.Equal(RoadToolFailure.Terrain, result.Failure);
            Assert.Equal(RoadPlacementFailure.OutOfBounds, result.TerrainFailure);
            Assert.False(controller.HasPendingStart);
        }

        [Fact]
        public void FirstClick_Underwater_FailTerrain()
        {
            var graph = new RoadGraph();
            var hm = CreateFlatHeightmap(33, 33, 0.1f); // 0.1 < sea level 0.35
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            var controller = new RoadPlacementController(graph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, null);

            var result = controller.Click(0.0, 0.0);
            Assert.False(result.Success);
            Assert.Equal(RoadToolFailure.Terrain, result.Failure);
            Assert.Equal(RoadPlacementFailure.Underwater, result.TerrainFailure);
            Assert.False(controller.HasPendingStart);
        }

        [Fact]
        public void TwoClicks_SuccessNodesSegmentsCost()
        {
            var graph = new RoadGraph();
            var hm = CreateFlatHeightmap(33, 33, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            var controller = new RoadPlacementController(graph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, null);

            var res1 = controller.Click(0.0, 0.0);
            Assert.True(res1.Success);

            var res2 = controller.Click(5.0, 0.0);
            Assert.True(res2.Success);
            Assert.Equal(2, graph.NodeCount);
            Assert.Equal(1, graph.SegmentCount);

            var expectedCost = RoadCostCalculator.GetSegmentCost(RoadCostSettings.Default, 5.0);
            Assert.Equal(expectedCost, res2.Cost);
            Assert.NotNull(res2.StartNode);
            Assert.NotNull(res2.EndNode);
            Assert.NotNull(res2.Segment);

            // graph contains ids
            var nodeA = graph.GetNode(res2.StartNode!.Value);
            var nodeB = graph.GetNode(res2.EndNode!.Value);
            var segment = graph.GetSegment(res2.Segment!.Value);
            Assert.NotNull(nodeA);
            Assert.NotNull(nodeB);
            Assert.NotNull(segment);

            Assert.True(controller.HasPendingStart);
            Assert.Equal(res2.EndNode, controller.PendingStartNode);
        }

        [Fact]
        public void ContinueChain_False_ClearsPendingStart()
        {
            var graph = new RoadGraph();
            var hm = CreateFlatHeightmap(33, 33, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            var toolSettings = new RoadToolSettings(1.5, false); // ContinueChain = false
            var controller = new RoadPlacementController(graph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, toolSettings, null);

            controller.Click(0.0, 0.0);
            var res2 = controller.Click(5.0, 0.0);
            Assert.True(res2.Success);
            Assert.False(controller.HasPendingStart);
        }

        [Fact]
        public void ChainOfFourClicks_3SegmentsConnected()
        {
            var graph = new RoadGraph();
            var hm = CreateFlatHeightmap(33, 33, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            var controller = new RoadPlacementController(graph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, null);

            controller.Click(0.0, 0.0);
            var r1 = controller.Click(5.0, 0.0);
            var r2 = controller.Click(10.0, 0.0);
            var r3 = controller.Click(15.0, 0.0);

            Assert.True(r1.Success);
            Assert.True(r2.Success);
            Assert.True(r3.Success);
            Assert.Equal(3, graph.SegmentCount);
            Assert.Equal(4, graph.NodeCount);

            // Check segments at end node of segment 1
            var segmentsAtEnd1 = graph.GetSegmentsAt(r1.EndNode!.Value);
            Assert.Contains(segmentsAtEnd1, s => s.Id == r1.Segment!.Value);
            Assert.Contains(segmentsAtEnd1, s => s.Id == r2.Segment!.Value);
        }

        [Fact]
        public void Snapping_Behaviors()
        {
            var graph = new RoadGraph();
            var hm = CreateFlatHeightmap(33, 33, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            var controller = new RoadPlacementController(graph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, null);

            // Add an existing node at (0,0) by placing a node via graph directly
            var existingNodeId = graph.AddNode(0.0, 0.0);

            // Click at (0.5, 0.5) (distance ~0.707 <= radius 1.5) reuses it
            var res1 = controller.Click(0.5, 0.5);
            Assert.True(res1.Success);
            Assert.Equal(existingNodeId, controller.PendingStartNode);
            Assert.Equal(1, graph.NodeCount); // no new node added

            // Click at (3.0, 0.0) (distance 3.0 > radius 1.5 from (0,0) and distance 1.0 < radius 1.5 from (2,0) so test with fresh graph)
            // To test snap isolation cleanly, let's test on fresh controllers or distinct points:
            var freshGraph = new RoadGraph();
            var n0 = freshGraph.AddNode(0.0, 0.0);
            var freshController = new RoadPlacementController(freshGraph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, null);

            // A click at (2,0) with radius 1.5 does not snap
            var res2 = freshController.Click(2.0, 0.0);
            Assert.True(res2.Success);
            Assert.Null(freshController.PendingStartNode); // first click at (2,0) is a free point (not snapped)
            Assert.Equal(1, freshGraph.NodeCount); // only n0 exists so far

            freshController.Cancel();

            // Click exactly 1.5 away from existing node at (0,0) -> click at (1.5, 0.0) -> distance == 1.5 (inclusive) DOES snap
            var res3 = freshController.Click(1.5, 0.0);
            Assert.True(res3.Success);
            Assert.Equal(n0, freshController.PendingStartNode);

            // Two nodes equidistant from click: lowest id wins
            var graph2 = new RoadGraph();
            var n1 = graph2.AddNode(1.0, 0.0); // ID 1
            var n2 = graph2.AddNode(-1.0, 0.0); // ID 2
            var controller2 = new RoadPlacementController(graph2, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, null);
            
            // Click at (0,0) is distance 1.0 from both nodes. Lowest ID (n1 = 1) should win.
            controller2.Click(0.0, 0.0);
            Assert.Equal(n1, controller2.PendingStartNode);

            // SnapRadius 0 only snaps on exact position match
            var toolZero = new RoadToolSettings(0.0, true);
            var controllerZero = new RoadPlacementController(graph2, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, toolZero, null);
            controllerZero.Click(0.1, 0.0); // distance 0.1 != 0
            Assert.Null(controllerZero.PendingStartNode); // did not snap
            controllerZero.Cancel();
            controllerZero.Click(1.0, 0.0); // exact match
            Assert.Equal(n1, controllerZero.PendingStartNode);
        }

        [Fact]
        public void Connection_BuildSegmentAB_StartAtA_ClickNearC_ClickNearB()
        {
            var graph = new RoadGraph();
            var hm = CreateFlatHeightmap(33, 33, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            var controller = new RoadPlacementController(graph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, null);

            // Build A-B
            controller.Click(0.0, 0.0); // A
            var rAB = controller.Click(5.0, 0.0); // B
            var nodeA = rAB.StartNode!.Value;
            var nodeB = rAB.EndNode!.Value;

            controller.Cancel();

            // Start at A by snapping (click at 0.1, 0.1)
            controller.Click(0.1, 0.1);
            Assert.Equal(nodeA, controller.PendingStartNode);

            // Click near new point C (5.0, 5.0)
            var rAC = controller.Click(5.0, 5.0);
            var nodeC = rAC.EndNode!.Value;
            Assert.Equal(3, graph.NodeCount); // A, B, C

            // Click near B (4.9, 0.1 snaps to B)
            var rCB = controller.Click(4.9, 0.1);
            Assert.Equal(nodeB, rCB.EndNode);

            Assert.True(graph.AreConnected(nodeA, nodeB));
            Assert.True(graph.AreConnected(nodeA, nodeC));
            Assert.True(graph.AreConnected(nodeC, nodeB));
            Assert.Equal(3, graph.NodeCount); // no duplicate nodes at A or B
            Assert.Equal(3, graph.SegmentCount); // A-B, A-C, C-B
        }

        [Fact]
        public void SameNode_Fails()
        {
            var graph = new RoadGraph();
            var hm = CreateFlatHeightmap(33, 33, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            var controller = new RoadPlacementController(graph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, null);

            // Create an existing node at (0,0)
            var nodeA = graph.AddNode(0.0, 0.0);

            // Click near (0,0) -> snaps to nodeA, sets pending start to nodeA
            var r1 = controller.Click(0.1, 0.1);
            Assert.True(r1.Success);
            Assert.Equal(nodeA, controller.PendingStartNode);

            // Click same place again (snaps to same node)
            var r2 = controller.Click(0.2, 0.2); // within snap radius 1.5 of (0,0)
            Assert.False(r2.Success);
            Assert.Equal(RoadToolFailure.SameNode, r2.Failure);
            Assert.True(controller.HasPendingStart);
            Assert.Equal(nodeA, controller.PendingStartNode);
            Assert.Equal(1, graph.NodeCount);
            Assert.Equal(0, graph.SegmentCount);
        }

        [Fact]
        public void DuplicateSegment_Fails()
        {
            var graph = new RoadGraph();
            var hm = CreateFlatHeightmap(33, 33, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            var budget = new FakeBudget(100, 100);
            var controller = new RoadPlacementController(graph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, budget);

            var rAB = controller.Click(0.0, 0.0);
            controller.Click(5.0, 0.0); // Segment A-B created

            controller.Cancel();
            long goldBefore = budget.Gold;
            long stoneBefore = budget.Stone;
            int trySpendBefore = budget.TrySpendCalls;

            // Start at A, click near B -> DuplicateSegment
            controller.Click(0.1, 0.1); // snaps to A
            var rDup = controller.Click(5.1, 0.1); // snaps to B

            Assert.False(rDup.Success);
            Assert.Equal(RoadToolFailure.DuplicateSegment, rDup.Failure);
            Assert.Equal(1, graph.SegmentCount);
            Assert.Equal(goldBefore, budget.Gold);
            Assert.Equal(stoneBefore, budget.Stone);
            Assert.Equal(trySpendBefore, budget.TrySpendCalls); // zero TrySpend calls
        }

        [Fact]
        public void TooShort_And_Underwater_Failures()
        {
            var graph = new RoadGraph();
            var hm = CreateFlatHeightmap(33, 33, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            var budget = new FakeBudget(100, 100);
            var controller = new RoadPlacementController(graph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, budget);

            controller.Click(0.0, 0.0);
            // Second click 0.5 units away (min segment length is 1.0 by default)
            var rShort = controller.Click(0.5, 0.0);
            Assert.False(rShort.Success);
            Assert.Equal(RoadToolFailure.Terrain, rShort.Failure);
            Assert.Equal(RoadPlacementFailure.TooShort, rShort.TerrainFailure);
            Assert.Equal(0, graph.SegmentCount);

            controller.Cancel();

            // Underwater second click
            var wetHm = CreateUnderwaterStripHeightmap(33, 33, 0.5f, 0.1f, 15, 20);
            var wetSurface = TerrainSurface.Centered(wetHm, 1.0, 10.0);
            var wetController = new RoadPlacementController(graph, wetSurface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, budget);

            wetController.Click(0.0, 0.0);
            int trySpendBefore = budget.TrySpendCalls;
            long goldBefore = budget.Gold;

            // Underwater strip is at heightmap z=15..20 -> world Z in [-1, 4]. Click at (0, 2) lands in strip.
            var rWet = wetController.Click(0.0, 2.0); // lands in underwater strip
            Assert.False(rWet.Success);
            Assert.Equal(RoadToolFailure.Terrain, rWet.Failure);
            Assert.Equal(RoadPlacementFailure.Underwater, rWet.TerrainFailure);
            Assert.Equal(0, graph.SegmentCount);
            Assert.Equal(goldBefore, budget.Gold);
            Assert.Equal(trySpendBefore, budget.TrySpendCalls); // no spend
        }

        [Fact]
        public void Budget_ChecksAndSpending()
        {
            var graph = new RoadGraph();
            var hm = CreateFlatHeightmap(33, 33, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            
            // Segment cost for length 5: units = 5, gold = 5*2 = 10, stone = 5*1 = 5 (with defaults minGold=2 minStone=1, so max(10,2)=10, max(5,1)=5 -> (10,5))
            var expectedCost = RoadCostCalculator.GetSegmentCost(RoadCostSettings.Default, 5.0);

            // 1. Balance too small
            var smallBudget = new FakeBudget(expectedCost.Gold - 1, expectedCost.Stone);
            var controller = new RoadPlacementController(graph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, smallBudget);

            controller.Click(0.0, 0.0);
            long goldBefore = smallBudget.Gold;
            long stoneBefore = smallBudget.Stone;

            var rFail = controller.Click(5.0, 0.0);
            Assert.False(rFail.Success);
            Assert.Equal(RoadToolFailure.InsufficientFunds, rFail.Failure);
            Assert.Equal(expectedCost, rFail.Cost);
            Assert.Equal(0, graph.SegmentCount);
            Assert.Equal(goldBefore, smallBudget.Gold);
            Assert.Equal(stoneBefore, smallBudget.Stone);
            Assert.True(controller.HasPendingStart); // pending start unchanged

            // 2. Exact balance
            var exactBudget = new FakeBudget(expectedCost.Gold, expectedCost.Stone);
            var controller2 = new RoadPlacementController(graph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, exactBudget);

            controller2.Click(0.0, 0.0);
            var rSuccess = controller2.Click(5.0, 0.0);
            Assert.True(rSuccess.Success);
            Assert.Equal(0L, exactBudget.Gold);
            Assert.Equal(0L, exactBudget.Stone);
            Assert.Single(exactBudget.SuccessfulSpends);
            Assert.Equal(expectedCost, exactBudget.SuccessfulSpends[0]);
        }

        [Fact]
        public void Cancel_Behavior()
        {
            var graph = new RoadGraph();
            var hm = CreateFlatHeightmap(33, 33, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            var budget = new FakeBudget(100, 100);
            var controller = new RoadPlacementController(graph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, budget);

            // Calling Cancel with no pending start does nothing and does not throw
            controller.Cancel();
            Assert.False(controller.HasPendingStart);

            controller.Click(0.0, 0.0);
            Assert.True(controller.HasPendingStart);

            long goldBefore = budget.Gold;
            controller.Cancel();
            Assert.False(controller.HasPendingStart);
            Assert.Equal(0, graph.NodeCount);
            Assert.Equal(0, graph.SegmentCount);
            Assert.Equal(goldBefore, budget.Gold);
        }

        [Fact]
        public void Preview_NeverMutates()
        {
            var graph = new RoadGraph();
            var hm = CreateFlatHeightmap(33, 33, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            var budget = new FakeBudget(10, 10);
            var controller = new RoadPlacementController(graph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, budget);

            int nodeCountBefore = graph.NodeCount;
            int segCountBefore = graph.SegmentCount;
            bool pendingBefore = controller.HasPendingStart;
            long goldBefore = budget.Gold;
            int trySpendBefore = budget.TrySpendCalls;
            int canAffordBefore = budget.CanAffordCalls;

            // Call Preview many times with valid and invalid points
            for (int i = 0; i < 50; i++)
            {
                controller.Preview(i, 0.0);
                controller.Preview(999.0, 999.0);
            }

            Assert.Equal(nodeCountBefore, graph.NodeCount);
            Assert.Equal(segCountBefore, graph.SegmentCount);
            Assert.Equal(pendingBefore, controller.HasPendingStart);
            Assert.Equal(goldBefore, budget.Gold);
            Assert.Equal(trySpendBefore, budget.TrySpendCalls);
        }

        [Fact]
        public void Preview_Click_Agreement()
        {
            var rng = new DeterministicRandom(42);
            var hm = CreateFlatHeightmap(33, 33, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);

            var points = new List<(double X, double Z)>
            {
                (0.0, 0.0), (5.0, 0.0), (10.0, 0.0), (10.0, 5.0),
                (999.0, 999.0), (0.1, 0.1), (2.0, 2.0), (7.0, 7.0),
                (-5.0, -5.0), (3.0, 3.0), (4.0, 4.0), (8.0, 8.0),
                (1.0, 1.0), (12.0, 12.0), (13.0, 13.0), (14.0, 14.0),
                (0.5, 0.5), (6.0, 6.0), (9.0, 9.0), (11.0, 11.0)
            };

            var graph1 = new RoadGraph();
            var controller1 = new RoadPlacementController(graph1, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, null);

            var graph2 = new RoadGraph();
            var controller2 = new RoadPlacementController(graph2, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, null);

            foreach (var pt in points)
            {
                // Preview on controller1
                var preview = controller1.Preview(pt.X, pt.Z);
                // Click on controller2
                var click = controller2.Click(pt.X, pt.Z);

                Assert.Equal(preview.IsValid, click.Success);
                Assert.Equal(preview.Failure, click.Failure);
                Assert.Equal(preview.TerrainFailure, click.TerrainFailure);
                Assert.Equal(preview.Cost, click.Cost);

                // Now click on controller1 to keep them in sync for next iteration
                controller1.Click(pt.X, pt.Z);
            }
        }

        [Fact]
        public void Preview_ReportsLengthAndCostMatchCalculator()
        {
            var graph = new RoadGraph();
            var hm = CreateFlatHeightmap(33, 33, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            var controller = new RoadPlacementController(graph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, null);

            // No pending start: length 0, cost zero
            var p0 = controller.Preview(0.0, 0.0);
            Assert.Equal(0.0, p0.Length);
            Assert.Equal(RoadCost.Zero, p0.Cost);

            controller.Click(0.0, 0.0);
            var p1 = controller.Preview(5.0, 0.0);
            Assert.Equal(5.0, p1.Length);
            Assert.Equal(RoadCostCalculator.GetSegmentCost(RoadCostSettings.Default, 5.0), p1.Cost);
        }

        [Fact]
        public void NaN_And_Infinite_Coordinates_HandledGracefully()
        {
            var graph = new RoadGraph();
            var hm = CreateFlatHeightmap(33, 33, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            var controller = new RoadPlacementController(graph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, null);

            var pPrev = controller.Preview(double.NaN, double.PositiveInfinity);
            Assert.False(pPrev.IsValid);
            Assert.Equal(RoadToolFailure.Terrain, pPrev.Failure);
            Assert.Equal(RoadPlacementFailure.OutOfBounds, pPrev.TerrainFailure);

            var pClick = controller.Click(double.NaN, double.PositiveInfinity);
            Assert.False(pClick.Success);
            Assert.Equal(RoadToolFailure.Terrain, pClick.Failure);
            Assert.Equal(RoadPlacementFailure.OutOfBounds, pClick.TerrainFailure);
        }

        [Fact]
        public void FUZZ_RandomActions_InvariantsMaintained()
        {
            var rng = new DeterministicRandom(4242);
            var hm = CreateFlatHeightmap(64, 64, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);
            var budget = new FakeBudget(500, 500);

            var graph = new RoadGraph();
            var controller = new RoadPlacementController(graph, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, budget);

            long startingGold = budget.Gold;
            long startingStone = budget.Stone;
            int successfulSegmentsCount = 0;
            var distinctNodes = new HashSet<int>();
            int insufficientFundsCount = 0;

            for (int i = 0; i < 2000; i++)
            {
                // Cancel with probability 1 in 10
                if (rng.NextInt(0, 10) == 0)
                {
                    controller.Cancel();
                }

                // Random point in-bounds or slightly out of bounds (-20 to 20)
                double x = rng.NextDouble() * 40.0 - 20.0;
                double z = rng.NextDouble() * 40.0 - 20.0;

                // Also occasionally test Preview before Click
                var preview = controller.Preview(x, z);
                var result = controller.Click(x, z);

                Assert.Equal(preview.IsValid, result.Success);
                Assert.Equal(preview.Failure, result.Failure);
                Assert.Equal(preview.TerrainFailure, result.TerrainFailure);
                Assert.Equal(preview.Cost, result.Cost);

                if (result.Success)
                {
                    if (result.Segment.HasValue)
                    {
                        successfulSegmentsCount++;
                        if (result.StartNode.HasValue) distinctNodes.Add(result.StartNode.Value.Value);
                        if (result.EndNode.HasValue) distinctNodes.Add(result.EndNode.Value.Value);
                    }
                }
                else if (result.Failure == RoadToolFailure.InsufficientFunds)
                {
                    insufficientFundsCount++;
                }

                // Assert invariants after EVERY action
                Assert.Equal(successfulSegmentsCount, graph.SegmentCount);
                Assert.Equal(distinctNodes.Count, graph.NodeCount);

                foreach (var seg in graph.Segments)
                {
                    Assert.True(graph.TryGetNode(seg.A, out _));
                    Assert.True(graph.TryGetNode(seg.B, out _));
                    Assert.True(seg.A.Value < seg.B.Value);
                }

                long totalSpentGold = 0;
                long totalSpentStone = 0;
                foreach (var cost in budget.SuccessfulSpends)
                {
                    totalSpentGold += cost.Gold;
                    totalSpentStone += cost.Stone;
                }

                Assert.Equal(startingGold - totalSpentGold, budget.Gold);
                Assert.Equal(startingStone - totalSpentStone, budget.Stone);
                Assert.True(budget.Gold >= 0);
                Assert.True(budget.Stone >= 0);
            }

            Assert.True(successfulSegmentsCount >= 10, $"Expected at least 10 segments placed, got {successfulSegmentsCount}");
            Assert.True(insufficientFundsCount >= 1, "Expected at least one InsufficientFunds failure");
        }

        [Fact]
        public void Determinism_SameSequence_EqualResults()
        {
            var hm = CreateFlatHeightmap(33, 33, 0.5f);
            var surface = TerrainSurface.Centered(hm, 1.0, 10.0);
            var water = new WaterSettings(0.35);

            var points = new List<(double X, double Z)>();
            var rng = new DeterministicRandom(12345);
            for (int i = 0; i < 50; i++)
            {
                points.Add((rng.NextDouble() * 30.0 - 15.0, rng.NextDouble() * 30.0 - 15.0));
            }

            var g1 = new RoadGraph();
            var c1 = new RoadPlacementController(g1, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, null);
            
            var g2 = new RoadGraph();
            var c2 = new RoadPlacementController(g2, surface, water, RoadPlacementSettings.Default, RoadCostSettings.Default, RoadToolSettings.Default, null);

            for (int i = 0; i < points.Count; i++)
            {
                var pt = points[i];
                var r1 = c1.Click(pt.X, pt.Z);
                var r2 = c2.Click(pt.X, pt.Z);

                Assert.Equal(r1.Success, r2.Success);
                Assert.Equal(r1.Failure, r2.Failure);
                Assert.Equal(r1.TerrainFailure, r2.TerrainFailure);
                Assert.Equal(r1.Cost, r2.Cost);
            }

            Assert.Equal(g1.NodeCount, g2.NodeCount);
            Assert.Equal(g1.SegmentCount, g2.SegmentCount);
        }
    }
}
