using MedievalCityBuilder.Roads;
using Xunit;

namespace MedievalCityBuilder.Tests
{
    public class RoadGraphTests
    {
        [Fact]
        public void AddNode_IdsAndCount()
        {
            var g = new RoadGraph();
            var id1 = g.AddNode(0, 0);
            var id2 = g.AddNode(1, 1);
            var id3 = g.AddNode(2, 2);
            Assert.Equal(1, id1.Value);
            Assert.Equal(2, id2.Value);
            Assert.Equal(3, id3.Value);
            Assert.Equal(3, g.NodeCount);
        }

        [Fact]
        public void AddNode_InvalidCoordinates()
        {
            var g = new RoadGraph();
            Assert.Throws<ArgumentOutOfRangeException>(() => g.AddNode(double.NaN, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => g.AddNode(0, double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => g.AddNode(double.PositiveInfinity, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => g.AddNode(0, double.NegativeInfinity));
        }

        [Fact]
        public void NodeIds_NeverReused()
        {
            var g = new RoadGraph();
            var id1 = g.AddNode(0, 0);
            var id2 = g.AddNode(1, 1);
            var n2 = g.GetNode(id2);
            // remove segment first if any
            g.RemoveNode(id2);
            var id3 = g.AddNode(2, 2);
            Assert.Equal(3, id3.Value);
        }

        [Fact]
        public void SegmentIds_NeverReused()
        {
            var g = new RoadGraph();
            var n1 = g.AddNode(0, 0);
            var n2 = g.AddNode(1, 1);
            var n3 = g.AddNode(2, 2);
            var s1 = g.AddSegment(n1, n2);
            var s2 = g.AddSegment(n2, n3);
            g.RemoveSegment(s2);
            var s3 = g.AddSegment(n1, n3);
            Assert.Equal(3, s3.Value);
        }

        [Fact]
        public void AddSegment_Valid()
        {
            var g = new RoadGraph();
            var a = g.AddNode(0, 0);
            var b = g.AddNode(1, 1);
            var s = g.AddSegment(a, b);
            Assert.Equal(1, s.Value);
            Assert.Equal(1, g.SegmentCount);
        }

        [Fact]
        public void AddSegment_SelfLoop_Throws()
        {
            var g = new RoadGraph();
            var a = g.AddNode(0, 0);
            Assert.Throws<ArgumentException>(() => g.AddSegment(a, a));
        }

        [Fact]
        public void AddSegment_UnknownNode_Throws()
        {
            var g = new RoadGraph();
            var a = g.AddNode(0, 0);
            var unknown = new RoadNodeId(999);
            Assert.Throws<ArgumentException>(() => g.AddSegment(a, unknown));
            Assert.Throws<ArgumentException>(() => g.AddSegment(unknown, a));
        }

        [Fact]
        public void AddSegment_Duplicate_Throws()
        {
            var g = new RoadGraph();
            var a = g.AddNode(0, 0);
            var b = g.AddNode(1, 1);
            g.AddSegment(a, b);
            Assert.Throws<ArgumentException>(() => g.AddSegment(a, b));
            Assert.Throws<ArgumentException>(() => g.AddSegment(b, a));
        }

        [Fact]
        public void AddSegment_NormalizedStorage()
        {
            var g = new RoadGraph();
            var a = g.AddNode(0, 0); // id 1
            var b = g.AddNode(1, 1); // id 2
            var s = g.AddSegment(b, a);
            var seg = g.GetSegment(s);
            Assert.True(seg.A.Value < seg.B.Value);
            Assert.Equal(a.Value, seg.A.Value);
            Assert.Equal(b.Value, seg.B.Value);
        }

        [Fact]
        public void GetNeighbors_Ordered()
        {
            var g = new RoadGraph();
            var n1 = g.AddNode(0, 0);
            var n2 = g.AddNode(1, 0);
            var n3 = g.AddNode(2, 0);
            var n4 = g.AddNode(3, 0);
            // add segments in scrambled order
            g.AddSegment(n3, n1); // id1
            g.AddSegment(n2, n4); // id2
            g.AddSegment(n1, n2); // id3
            var neighbors = g.GetNeighbors(n1);
            Assert.Equal(2, neighbors.Count);
            Assert.Equal(n2.Value, neighbors[0].Value);
            Assert.Equal(n3.Value, neighbors[1].Value);
        }

        [Fact]
        public void GetSegmentsAt_Ordered()
        {
            var g = new RoadGraph();
            var n1 = g.AddNode(0, 0);
            var n2 = g.AddNode(1, 0);
            var n3 = g.AddNode(2, 0);
            var s1 = g.AddSegment(n3, n1);
            var s2 = g.AddSegment(n1, n2);
            var segs = g.GetSegmentsAt(n1);
            Assert.Equal(2, segs.Count);
            Assert.Equal(s1.Value, segs[0].Id.Value);
            Assert.Equal(s2.Value, segs[1].Id.Value);
        }

        [Fact]
        public void GetNeighbors_UnknownNode_Throws()
        {
            var g = new RoadGraph();
            Assert.Throws<ArgumentException>(() => g.GetNeighbors(new RoadNodeId(1)));
        }

        [Fact]
        public void RemoveNode_WithSegment_Throws()
        {
            var g = new RoadGraph();
            var a = g.AddNode(0, 0);
            var b = g.AddNode(1, 1);
            g.AddSegment(a, b);
            Assert.Throws<InvalidOperationException>(() => g.RemoveNode(a));
        }

        [Fact]
        public void RemoveNode_AfterRemovingSegment_Succeeds()
        {
            var g = new RoadGraph();
            var a = g.AddNode(0, 0);
            var b = g.AddNode(1, 1);
            var s = g.AddSegment(a, b);
            g.RemoveSegment(s);
            g.RemoveNode(a);
            Assert.Equal(1, g.NodeCount);
        }

        [Fact]
        public void RemoveNode_Unknown_Throws()
        {
            var g = new RoadGraph();
            Assert.Throws<ArgumentException>(() => g.RemoveNode(new RoadNodeId(1)));
        }

        [Fact]
        public void RemoveSegment_Unknown_Throws()
        {
            var g = new RoadGraph();
            Assert.Throws<ArgumentException>(() => g.RemoveSegment(new RoadSegmentId(1)));
        }

        [Fact]
        public void GetSegmentLength_Triangle()
        {
            var g = new RoadGraph();
            var a = g.AddNode(0, 0);
            var b = g.AddNode(3, 4);
            var s = g.AddSegment(a, b);
            Assert.Equal(5.0, g.GetSegmentLength(s));
        }

        [Fact]
        public void GetSegmentLength_Zero()
        {
            var g = new RoadGraph();
            var a = g.AddNode(1, 1);
            var b = g.AddNode(1, 1);
            var s = g.AddSegment(a, b);
            Assert.Equal(0.0, g.GetSegmentLength(s));
        }

        [Fact]
        public void Connectivity_ChainAndIsolated()
        {
            var g = new RoadGraph();
            var a = g.AddNode(0, 0);
            var b = g.AddNode(1, 0);
            var c = g.AddNode(2, 0);
            var d = g.AddNode(10, 10);
            g.AddSegment(a, b);
            g.AddSegment(b, c);
            Assert.True(g.AreConnected(a, c));
            Assert.False(g.AreConnected(a, d));
            Assert.True(g.AreConnected(a, a));
            Assert.Equal(2, g.CountConnectedComponents());
        }

        [Fact]
        public void Connectivity_ConnectComponents()
        {
            var g = new RoadGraph();
            var a = g.AddNode(0, 0);
            var b = g.AddNode(1, 0);
            var c = g.AddNode(2, 0);
            var d = g.AddNode(10, 10);
            g.AddSegment(a, b);
            g.AddSegment(b, c);
            g.AddSegment(c, d);
            Assert.Equal(1, g.CountConnectedComponents());
            Assert.True(g.AreConnected(a, d));
        }

        [Fact]
        public void Connectivity_RemoveSegment_Breaks()
        {
            var g = new RoadGraph();
            var a = g.AddNode(0, 0);
            var b = g.AddNode(1, 0);
            var c = g.AddNode(2, 0);
            var s1 = g.AddSegment(a, b);
            var s2 = g.AddSegment(b, c);
            Assert.True(g.AreConnected(a, c));
            g.RemoveSegment(s2);
            Assert.False(g.AreConnected(a, c));
        }

        [Fact]
        public void CountConnectedComponents_Empty()
        {
            var g = new RoadGraph();
            Assert.Equal(0, g.CountConnectedComponents());
        }

        [Fact]
        public void AreConnected_UnknownNode_Throws()
        {
            var g = new RoadGraph();
            var a = g.AddNode(0, 0);
            Assert.Throws<ArgumentException>(() => g.AreConnected(a, new RoadNodeId(999)));
        }

        [Fact]
        public void Determinism_SameSequence()
        {
            RoadGraph Build()
            {
                var g = new RoadGraph();
                var n1 = g.AddNode(0, 0);
                var n2 = g.AddNode(1, 1);
                var n3 = g.AddNode(2, 2);
                g.AddSegment(n1, n2);
                g.AddSegment(n2, n3);
                return g;
            }

            var g1 = Build();
            var g2 = Build();

            Assert.Equal(g1.NodeCount, g2.NodeCount);
            Assert.Equal(g1.SegmentCount, g2.SegmentCount);
            foreach (var (n1, n2) in g1.Nodes.Zip(g2.Nodes))
            {
                Assert.Equal(n1.Id.Value, n2.Id.Value);
                Assert.Equal(n1.X, n2.X);
                Assert.Equal(n1.Z, n2.Z);
            }
            foreach (var (s1, s2) in g1.Segments.Zip(g2.Segments))
            {
                Assert.Equal(s1.Id.Value, s2.Id.Value);
                Assert.Equal(s1.A.Value, s2.A.Value);
                Assert.Equal(s1.B.Value, s2.B.Value);
            }
        }

        [Fact]
        public void Scale_Chain5000()
        {
            var g = new RoadGraph();
            var first = g.AddNode(0, 0);
            RoadNodeId prev = first;
            for (int i = 1; i < 5000; i++)
            {
                var cur = g.AddNode(i, 0);
                g.AddSegment(prev, cur);
                prev = cur;
            }
            var last = prev;
            Assert.True(g.AreConnected(first, last));
            Assert.Equal(5000, g.NodeCount);
            Assert.Equal(4999, g.SegmentCount);
        }

        [Fact]
        public void TryGet_ExistingAndMissing()
        {
            var g = new RoadGraph();
            var id = g.AddNode(0, 0);
            Assert.True(g.TryGetNode(id, out var node));
            Assert.Equal(id.Value, node.Id.Value);
            Assert.False(g.TryGetNode(new RoadNodeId(999), out _));
        }

        [Fact]
        public void Adjacency_StarGraph_ScrambledOrder()
        {
            var g = new RoadGraph();
            var hub = g.AddNode(0, 0);
            var spokeIds = new List<RoadNodeId>();
            var segmentIds = new List<RoadSegmentId>();

            // Add 1000 spokes in scrambled order
            var indices = Enumerable.Range(1, 1000).OrderBy(_ => Guid.NewGuid()).ToList();
            foreach (var i in indices)
            {
                var spoke = g.AddNode(i, 0);
                spokeIds.Add(spoke);
                segmentIds.Add(g.AddSegment(hub, spoke));
            }

            // Hub has 1000 neighbors in ascending node id order (spokes have IDs 2..1001)
            var hubNeighbors = g.GetNeighbors(hub);
            Assert.Equal(1000, hubNeighbors.Count);
            for (int i = 0; i < 1000; i++)
            {
                Assert.Equal(i + 2, hubNeighbors[i].Value);
            }

            // Hub has 1000 segments in ascending segment id order
            var hubSegments = g.GetSegmentsAt(hub);
            Assert.Equal(1000, hubSegments.Count);
            for (int i = 0; i < 1000; i++)
            {
                Assert.Equal(i + 1, hubSegments[i].Id.Value);
            }

            // Each spoke has exactly one neighbor (the hub)
            foreach (var spoke in spokeIds)
            {
                var neighbors = g.GetNeighbors(spoke);
                Assert.Single(neighbors);
                Assert.Equal(hub.Value, neighbors[0].Value);
            }
        }

        [Fact]
        public void Adjacency_RemoveSegment_UpdatesNeighborsAndSegmentsAt()
        {
            var g = new RoadGraph();
            var a = g.AddNode(0, 0);
            var b = g.AddNode(1, 0);
            var c = g.AddNode(2, 0);
            var s1 = g.AddSegment(a, b);
            var s2 = g.AddSegment(b, c);

            // Before removal
            Assert.Contains(g.GetSegmentsAt(a), seg => seg.Id.Value == s1.Value);
            Assert.Contains(g.GetSegmentsAt(b), seg => seg.Id.Value == s1.Value);
            Assert.Contains(g.GetSegmentsAt(b), seg => seg.Id.Value == s2.Value);
            Assert.Contains(g.GetSegmentsAt(c), seg => seg.Id.Value == s2.Value);
            Assert.Contains(g.GetNeighbors(a).Select(n => n.Value), v => v == b.Value);
            Assert.Contains(g.GetNeighbors(b).Select(n => n.Value), v => v == a.Value);
            Assert.Contains(g.GetNeighbors(b).Select(n => n.Value), v => v == c.Value);
            Assert.Contains(g.GetNeighbors(c).Select(n => n.Value), v => v == b.Value);
            Assert.True(g.AreConnected(a, c));

            // Remove s1 (a-b)
            g.RemoveSegment(s1);

            // After removal: a and b no longer have s1
            Assert.DoesNotContain(g.GetSegmentsAt(a), seg => seg.Id.Value == s1.Value);
            Assert.DoesNotContain(g.GetSegmentsAt(b), seg => seg.Id.Value == s1.Value);
            Assert.DoesNotContain(g.GetNeighbors(a).Select(n => n.Value), v => v == b.Value);
            Assert.DoesNotContain(g.GetNeighbors(b).Select(n => n.Value), v => v == a.Value);

            // b and c still have s2
            Assert.Contains(g.GetSegmentsAt(b), seg => seg.Id.Value == s2.Value);
            Assert.Contains(g.GetSegmentsAt(c), seg => seg.Id.Value == s2.Value);
            Assert.Contains(g.GetNeighbors(b).Select(n => n.Value), v => v == c.Value);
            Assert.Contains(g.GetNeighbors(c).Select(n => n.Value), v => v == b.Value);

            // a and c are no longer connected (s1 was the only link)
            Assert.False(g.AreConnected(a, c));
            // b and c still connected
            Assert.True(g.AreConnected(b, c));
        }

        [Fact]
        public void Adjacency_RemoveNode_HubWithSpokes()
        {
            var g = new RoadGraph();
            var hub = g.AddNode(0, 0);
            var spokeIds = new List<RoadNodeId>();
            var segmentIds = new List<RoadSegmentId>();

            for (int i = 1; i <= 1000; i++)
            {
                var spoke = g.AddNode(i, 0);
                spokeIds.Add(spoke);
                segmentIds.Add(g.AddSegment(hub, spoke));
            }

            // RemoveNode(hub) throws while any spoke segment exists
            Assert.Throws<InvalidOperationException>(() => g.RemoveNode(hub));

            // Remove all 1000 segments
            foreach (var segId in segmentIds)
            {
                g.RemoveSegment(segId);
            }

            // Now RemoveNode(hub) succeeds
            g.RemoveNode(hub);

            // 1000 spoke nodes remain, each isolated
            Assert.Equal(1000, g.NodeCount);
            Assert.Equal(1000, g.CountConnectedComponents());
        }

        [Fact]
        public void Adjacency_Chain20000_Connected()
        {
            var g = new RoadGraph();
            var first = g.AddNode(0, 0);
            RoadNodeId prev = first;
            for (int i = 1; i < 20000; i++)
            {
                var cur = g.AddNode(i, 0);
                g.AddSegment(prev, cur);
                prev = cur;
            }
            var last = prev;

            Assert.True(g.AreConnected(first, last));
            Assert.Equal(1, g.CountConnectedComponents());

            // Remove middle segment (between 9999 and 10000)
            var node9999 = new RoadNodeId(9999);
            var node10000 = new RoadNodeId(10000);
            var segsAt9999 = g.GetSegmentsAt(node9999);
            var segToRemove = segsAt9999.First(s => (s.A.Value == 9999 && s.B.Value == 10000) || (s.A.Value == 10000 && s.B.Value == 9999));
            g.RemoveSegment(segToRemove.Id);

            Assert.False(g.AreConnected(first, last));
            Assert.Equal(2, g.CountConnectedComponents());
        }

        [Fact]
        public void Adjacency_CyclePlusPair_Components()
        {
            var g = new RoadGraph();
            // Triangle: 1-2-3-1
            var n1 = g.AddNode(0, 0);
            var n2 = g.AddNode(1, 0);
            var n3 = g.AddNode(0, 1);
            g.AddSegment(n1, n2);
            g.AddSegment(n2, n3);
            g.AddSegment(n3, n1);

            // Separate pair: 4-5
            var n4 = g.AddNode(10, 10);
            var n5 = g.AddNode(11, 10);
            g.AddSegment(n4, n5);

            Assert.Equal(2, g.CountConnectedComponents());
            Assert.True(g.AreConnected(n1, n2));
            Assert.True(g.AreConnected(n2, n3));
            Assert.True(g.AreConnected(n3, n1));
            Assert.True(g.AreConnected(n4, n5));
            Assert.False(g.AreConnected(n1, n4));
        }
    }
}
