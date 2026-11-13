using System;
using System.Collections.Generic;

namespace MedievalCityBuilder.Zoning
{
    /// <summary>
    /// Integer grid coordinates for a zone cell.
    /// </summary>
    public readonly record struct ZoneCell(int X, int Z);

    /// <summary>
    /// Zone grid for painting zones on a 2D grid over terrain.
    /// Zones are painted on a grid of cells over the terrain; zoning is free and creates no buildings.
    /// Placement rules, road distance, persistence and the scene brush are separate tasks.
    /// A saved grid will require a save format version increment.
    /// </summary>
    public sealed class ZoneGrid
    {
        private readonly int[] _cells;

        public ZoneRegistry Registry { get; }
        public double MinX { get; }
        public double MinZ { get; }
        public double CellSize { get; }
        public int CellsX { get; }
        public int CellsZ { get; }
        public double MaxX => MinX + CellsX * CellSize;
        public double MaxZ => MinZ + CellsZ * CellSize;

        public ZoneGrid(ZoneRegistry registry, double minX, double minZ, double cellSize, int cellsX, int cellsZ)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            if (double.IsNaN(minX) || double.IsInfinity(minX))
                throw new ArgumentOutOfRangeException(nameof(minX), "minX must be finite.");
            if (double.IsNaN(minZ) || double.IsInfinity(minZ))
                throw new ArgumentOutOfRangeException(nameof(minZ), "minZ must be finite.");
            if (double.IsNaN(cellSize) || double.IsInfinity(cellSize) || cellSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(cellSize), "cellSize must be finite and > 0.");
            if (cellsX < 1)
                throw new ArgumentOutOfRangeException(nameof(cellsX), "cellsX must be >= 1.");
            if (cellsZ < 1)
                throw new ArgumentOutOfRangeException(nameof(cellsZ), "cellsZ must be >= 1.");

            long totalCells = (long)cellsX * cellsZ;
            if (totalCells > 4_000_000)
                throw new ArgumentOutOfRangeException(nameof(cellsX), "Grid size exceeds sanity cap of 4,000,000 cells.");

            Registry = registry;
            MinX = minX;
            MinZ = minZ;
            CellSize = cellSize;
            CellsX = cellsX;
            CellsZ = cellsZ;
            _cells = new int[cellsX * cellsZ];
        }

        private int Index(int x, int z) => z * CellsX + x;

        private void ValidateCoords(int x, int z)
        {
            if (x < 0 || x >= CellsX || z < 0 || z >= CellsZ)
                throw new ArgumentOutOfRangeException($"Coordinates ({x}, {z}) are out of range.");
        }

        public ZoneTypeId Get(int x, int z)
        {
            ValidateCoords(x, z);
            return new ZoneTypeId(_cells[Index(x, z)]);
        }

        public bool Set(int x, int z, ZoneTypeId zone)
        {
            ValidateCoords(x, z);

            if (!zone.IsNone && !Registry.IsRegistered(zone))
                throw new ArgumentException($"Zone type {zone} is not registered.", nameof(zone));

            int idx = Index(x, z);
            int current = _cells[idx];
            int newValue = zone.Value;

            if (current == newValue)
                return false;

            _cells[idx] = newValue;
            return true;
        }

        public bool TryGetCellAt(double worldX, double worldZ, out ZoneCell cell)
        {
            if (double.IsNaN(worldX) || double.IsInfinity(worldX) ||
                double.IsNaN(worldZ) || double.IsInfinity(worldZ))
            {
                cell = default;
                return false;
            }

            if (worldX < MinX || worldX > MaxX || worldZ < MinZ || worldZ > MaxZ)
            {
                cell = default;
                return false;
            }

            int x = (int)Math.Floor((worldX - MinX) / CellSize);
            int z = (int)Math.Floor((worldZ - MinZ) / CellSize);

            // Clamp to last cell for upper edge
            x = Math.Max(0, Math.Min(x, CellsX - 1));
            z = Math.Max(0, Math.Min(z, CellsZ - 1));

            cell = new ZoneCell(x, z);
            return true;
        }

        public double CellCenterX(int x)
        {
            ValidateCoords(x, 0); // validate x only
            return MinX + (x + 0.5) * CellSize;
        }

        public double CellCenterZ(int z)
        {
            if (z < 0 || z >= CellsZ)
                throw new ArgumentOutOfRangeException(nameof(z), $"Coordinate {z} is out of range.");
            return MinZ + (z + 0.5) * CellSize;
        }

        private static void ValidateBrushInput(double worldX, double worldZ, double radius)
        {
            if (double.IsNaN(worldX) || double.IsInfinity(worldX) ||
                double.IsNaN(worldZ) || double.IsInfinity(worldZ) ||
                double.IsNaN(radius) || double.IsInfinity(radius) || radius <= 0)
                throw new ArgumentOutOfRangeException("Brush inputs must be finite and radius > 0.");
        }

        public IReadOnlyList<ZoneCell> GetBrushCells(double worldX, double worldZ, double radius)
        {
            ValidateBrushInput(worldX, worldZ, radius);

            double radiusSq = radius * radius;
            var cells = new List<ZoneCell>();

            // Compute bounding box
            double minWorldX = worldX - radius;
            double maxWorldX = worldX + radius;
            double minWorldZ = worldZ - radius;
            double maxWorldZ = worldZ + radius;

            int minXIdx = (int)Math.Floor((minWorldX - MinX) / CellSize);
            int maxXIdx = (int)Math.Floor((maxWorldX - MinX) / CellSize);
            int minZIdx = (int)Math.Floor((minWorldZ - MinZ) / CellSize);
            int maxZIdx = (int)Math.Floor((maxWorldZ - MinZ) / CellSize);

            // Clamp to grid bounds
            minXIdx = Math.Max(0, minXIdx);
            maxXIdx = Math.Min(CellsX - 1, maxXIdx);
            minZIdx = Math.Max(0, minZIdx);
            maxZIdx = Math.Min(CellsZ - 1, maxZIdx);

            for (int z = minZIdx; z <= maxZIdx; z++)
            {
                double centerZ = CellCenterZ(z);
                double dz = centerZ - worldZ;
                double dzSq = dz * dz;
                if (dzSq > radiusSq) continue;

                for (int x = minXIdx; x <= maxXIdx; x++)
                {
                    double centerX = CellCenterX(x);
                    double dx = centerX - worldX;
                    double distSq = dx * dx + dzSq;
                    if (distSq <= radiusSq)
                    {
                        cells.Add(new ZoneCell(x, z));
                    }
                }
            }

            return cells;
        }

        public int Paint(double worldX, double worldZ, double radius, ZoneTypeId zone)
        {
            ValidateBrushInput(worldX, worldZ, radius);

            if (!zone.IsNone && !Registry.IsRegistered(zone))
                throw new ArgumentException($"Zone type {zone} is not registered.", nameof(zone));

            var brushCells = GetBrushCells(worldX, worldZ, radius);
            int changed = 0;
            foreach (var cell in brushCells)
            {
                if (Set(cell.X, cell.Z, zone))
                    changed++;
            }
            return changed;
        }

        public int Erase(double worldX, double worldZ, double radius) => Paint(worldX, worldZ, radius, ZoneTypeId.None);

        public int CountCells(ZoneTypeId zone)
        {
            int count = 0;
            int target = zone.Value;
            foreach (int id in _cells)
            {
                if (id == target)
                    count++;
            }
            return count;
        }

        public int PaintedCellCount
        {
            get
            {
                int count = 0;
                foreach (int id in _cells)
                {
                    if (id != 0)
                        count++;
                }
                return count;
            }
        }

        public IReadOnlyList<int> GetRawCells()
        {
            var copy = new int[_cells.Length];
            Array.Copy(_cells, copy, _cells.Length);
            return copy;
        }
    }
}
