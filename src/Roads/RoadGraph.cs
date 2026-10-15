using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MedievalCityBuilder.Roads
{
    /// <summary>
    /// In-memory road graph with deterministic ordering.
    /// This graph is in-memory only; saving it will extend WorldSaveData later and require a save format version increment.
    /// </summary>
    public sealed class RoadGraph
    {
        private readonly SortedDictionary<int, RoadNode> _nodes = new();
        private readonly SortedDictionary<int, RoadSegment> _segments = new();
        private readonly Dictionary<(int A, int B), RoadSegmentId> _segmentLookup = new();
        private int _nextNodeId = 1;
        private int _nextSegmentId = 1;

        /// <summary>
        /// Adds a road node at the given world coordinates.
        /// </summary>
        public RoadNodeId AddNode(double x, double z)
        {
            if (double.IsNaN(x) || double.IsInfinity(x) || double.IsNaN(z) || double.IsInfinity(z))
                throw new ArgumentOutOfRangeException(nameof(x), "Coordinate must be a finite number.");

            var id = new RoadNodeId(_nextNodeId++);
            var node = new RoadNode(id, x, z);
            _nodes[id.Value] = node;
            return id;
        }

        /// <summary>
        /// Adds a road segment between two existing nodes.
        /// </summary>
        public RoadSegmentId AddSegment(RoadNodeId a, RoadNodeId b)
        {
            if (!_nodes.ContainsKey(a.Value) || !_nodes.ContainsKey(b.Value))
                throw new ArgumentException("One or both nodes do not exist.");

            if (a.Value == b.Value)
                throw new ArgumentException("Cannot create a segment from a node to itself.");

            int first = Math.Min(a.Value, b.Value);
            int second = Math.Max(a.Value, b.Value);
            var normalized = (first, second);

            if (_segmentLookup.ContainsKey(normalized))
                throw new ArgumentException("A segment between these nodes already exists.");

            var id = new RoadSegmentId(_nextSegmentId++);
            var nodeA = new RoadNodeId(first);
            var nodeB = new RoadNodeId(second);
            var segment = new RoadSegment(id, nodeA, nodeB);
            _segments[id.Value] = segment;
            _segmentLookup[normalized] = id;
            return id;
        }

        /// <summary>
        /// Removes a segment by id.
        /// </summary>
        public void RemoveSegment(RoadSegmentId id)
        {
            if (!_segments.TryGetValue(id.Value, out var segment))
                throw new ArgumentException("Segment does not exist.");

            var normalized = (Math.Min(segment.A.Value, segment.B.Value), Math.Max(segment.A.Value, segment.B.Value));
            _segmentLookup.Remove(normalized);
            _segments.Remove(id.Value);
        }

        /// <summary>
        /// Removes a node if it has no incident segments.
        /// </summary>
        public void RemoveNode(RoadNodeId id)
        {
            if (!_nodes.ContainsKey(id.Value))
                throw new ArgumentException("Node does not exist.");

            foreach (var seg in _segments.Values)
            {
                if (seg.A.Value == id.Value || seg.B.Value == id.Value)
                    throw new InvalidOperationException("Node is still connected to one or more segments.");
            }

            _nodes.Remove(id.Value);
        }

        public int NodeCount => _nodes.Count;
        public int SegmentCount => _segments.Count;

        public IReadOnlyList<RoadNode> Nodes => _nodes.Values.ToList();
        public IReadOnlyList<RoadSegment> Segments => _segments.Values.ToList();

        public RoadNode GetNode(RoadNodeId id)
        {
            if (!_nodes.TryGetValue(id.Value, out var node))
                throw new ArgumentException("Node does not exist.");
            return node;
        }

        public bool TryGetNode(RoadNodeId id, [MaybeNullWhen(false)] out RoadNode node)
        {
            return _nodes.TryGetValue(id.Value, out node);
        }

        public RoadSegment GetSegment(RoadSegmentId id)
        {
            if (!_segments.TryGetValue(id.Value, out var seg))
                throw new ArgumentException("Segment does not exist.");
            return seg;
        }

        public bool TryGetSegment(RoadSegmentId id, [MaybeNullWhen(false)] out RoadSegment segment)
        {
            return _segments.TryGetValue(id.Value, out segment);
        }

        public IReadOnlyList<RoadSegment> GetSegmentsAt(RoadNodeId node)
        {
            if (!_nodes.ContainsKey(node.Value))
                throw new ArgumentException("Node does not exist.");

            var list = new List<RoadSegment>();
            foreach (var seg in _segments.Values)
            {
                if (seg.A.Value == node.Value || seg.B.Value == node.Value)
                    list.Add(seg);
            }
            return list;
        }

        public IReadOnlyList<RoadNodeId> GetNeighbors(RoadNodeId node)
        {
            if (!_nodes.ContainsKey(node.Value))
                throw new ArgumentException("Node does not exist.");

            var neighbors = new HashSet<int>();
            foreach (var seg in _segments.Values)
            {
                if (seg.A.Value == node.Value)
                    neighbors.Add(seg.B.Value);
                else if (seg.B.Value == node.Value)
                    neighbors.Add(seg.A.Value);
            }

            var ordered = neighbors.Select(v => new RoadNodeId(v)).OrderBy(id => id.Value).ToList();
            return ordered;
        }

        public double GetSegmentLength(RoadSegmentId id)
        {
            var seg = GetSegment(id);
            var a = GetNode(seg.A);
            var b = GetNode(seg.B);
            double dx = a.X - b.X;
            double dz = a.Z - b.Z;
            return Math.Sqrt(dx * dx + dz * dz);
        }

        public bool AreConnected(RoadNodeId a, RoadNodeId b)
        {
            if (!_nodes.ContainsKey(a.Value) || !_nodes.ContainsKey(b.Value))
                throw new ArgumentException("One or both nodes do not exist.");

            if (a.Value == b.Value)
                return true;

            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            visited.Add(a.Value);
            queue.Enqueue(a.Value);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var seg in _segments.Values)
                {
                    int neighbor = -1;
                    if (seg.A.Value == current)
                        neighbor = seg.B.Value;
                    else if (seg.B.Value == current)
                        neighbor = seg.A.Value;
                    else
                        continue;

                    if (neighbor == b.Value)
                        return true;

                    if (visited.Add(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return false;
        }

        public int CountConnectedComponents()
        {
            if (_nodes.Count == 0)
                return 0;

            var visited = new HashSet<int>();
            int components = 0;

            foreach (var nodeId in _nodes.Keys)
            {
                if (visited.Contains(nodeId))
                    continue;

                components++;
                var queue = new Queue<int>();
                visited.Add(nodeId);
                queue.Enqueue(nodeId);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    foreach (var seg in _segments.Values)
                    {
                        int neighbor = -1;
                        if (seg.A.Value == current)
                            neighbor = seg.B.Value;
                        else if (seg.B.Value == current)
                            neighbor = seg.A.Value;
                        else
                            continue;

                        if (visited.Add(neighbor))
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            return components;
        }
    }

    /// <summary>
    /// Represents a road node with world coordinates.
    /// </summary>
    public record RoadNode(RoadNodeId Id, double X, double Z);

    /// <summary>
    /// Represents a road segment connecting two nodes.
    /// </summary>
    public record RoadSegment(RoadSegmentId Id, RoadNodeId A, RoadNodeId B);
}
