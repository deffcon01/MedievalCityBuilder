using System;
using System.Linq;
using MedievalCityBuilder.World;
using MedievalCityBuilder.Zoning;
using Xunit;

namespace MedievalCityBuilder.Tests
{
    public class ZoneGridTests
    {
        private static ZoneRegistry CreateRegistry()
        {
            return ZoneRegistry.CreateStartingRegistry();
        }

        private static ZoneGrid CreateGrid(ZoneRegistry registry = null)
        {
            registry ??= CreateRegistry();
            return new ZoneGrid(registry, -5.0, -5.0, 1.0, 10, 10);
        }

        [Fact]
        public void Registry_NoneCannotBeRegistered()
        {
            var registry = new ZoneRegistry();
            var ex = Assert.Throws<ArgumentException>(() => 
                registry.Register(new ZoneDefinition(ZoneTypeId.None, "Test", ZoneCategory.Residential)));
            Assert.Contains("None", ex.Message);
        }

        [Fact]
        public void Registry_DuplicateIdThrows()
        {
            var registry = new ZoneRegistry();
            registry.Register(new ZoneDefinition(new ZoneTypeId(1), "Test1", ZoneCategory.Residential));
            var ex = Assert.Throws<ArgumentException>(() => 
                registry.Register(new ZoneDefinition(new ZoneTypeId(1), "Test2", ZoneCategory.Commercial)));
            Assert.Contains("Duplicate Id", ex.Message);
        }

        [Fact]
        public void Registry_DuplicateNameThrows()
        {
            var registry = new ZoneRegistry();
            registry.Register(new ZoneDefinition(new ZoneTypeId(1), "Test", ZoneCategory.Residential));
            var ex = Assert.Throws<ArgumentException>(() => 
                registry.Register(new ZoneDefinition(new ZoneTypeId(2), "TEST", ZoneCategory.Commercial)));
            Assert.Contains("Duplicate Name", ex.Message);
        }

        [Fact]
        public void Registry_NullThrows()
        {
            var registry = new ZoneRegistry();
            Assert.Throws<ArgumentNullException>(() => registry.Register(null!));
        }

        [Fact]
        public void Registry_GetUnknownThrows()
        {
            var registry = CreateRegistry();
            Assert.Throws<ArgumentException>(() => registry.Get(new ZoneTypeId(99)));
        }

        [Fact]
        public void Registry_TryGetAndIsRegistered()
        {
            var registry = CreateRegistry();
            var id = new ZoneTypeId(1);
            Assert.True(registry.TryGet(id, out var def));
            Assert.Equal("Small Residential", def.Name);
            Assert.True(registry.IsRegistered(id));
            Assert.False(registry.IsRegistered(ZoneTypeId.None));
            Assert.False(registry.TryGet(new ZoneTypeId(99), out _));
            Assert.False(registry.IsRegistered(new ZoneTypeId(99)));
        }

        [Fact]
        public void Registry_CreateStartingRegistry()
        {
            var registry = ZoneRegistry.CreateStartingRegistry();
            Assert.Equal(8, registry.Count);
            
            var defs = registry.Definitions;
            Assert.Equal(8, defs.Count);
            
            Assert.Equal(1, defs[0].Id.Value);
            Assert.Equal("Small Residential", defs[0].Name);
            Assert.Equal(ZoneCategory.Residential, defs[0].Category);
            
            Assert.Equal(2, defs[1].Id.Value);
            Assert.Equal("Multifamily Residential", defs[1].Name);
            Assert.Equal(ZoneCategory.Residential, defs[1].Category);
            
            Assert.Equal(3, defs[2].Id.Value);
            Assert.Equal("Small Open Market", defs[2].Name);
            Assert.Equal(ZoneCategory.Commercial, defs[2].Category);
            
            Assert.Equal(4, defs[3].Id.Value);
            Assert.Equal("Light Commercial", defs[3].Name);
            Assert.Equal(ZoneCategory.Commercial, defs[3].Category);
            
            Assert.Equal(5, defs[4].Id.Value);
            Assert.Equal("Light Industry", defs[4].Name);
            Assert.Equal(ZoneCategory.Industrial, defs[4].Category);
            
            Assert.Equal(6, defs[5].Id.Value);
            Assert.Equal("Residential-Agriculture", defs[5].Name);
            Assert.Equal(ZoneCategory.MixedUse, defs[5].Category);
            
            Assert.Equal(7, defs[6].Id.Value);
            Assert.Equal("Residential-Industry", defs[6].Name);
            Assert.Equal(ZoneCategory.MixedUse, defs[6].Category);
            
            Assert.Equal(8, defs[7].Id.Value);
            Assert.Equal("Residential-Commercial", defs[7].Name);
            Assert.Equal(ZoneCategory.MixedUse, defs[7].Category);
        }

        [Fact]
        public void Registry_DefinitionsOrderedById()
        {
            var registry = new ZoneRegistry();
            registry.Register(new ZoneDefinition(new ZoneTypeId(5), "E", ZoneCategory.Residential));
            registry.Register(new ZoneDefinition(new ZoneTypeId(2), "B", ZoneCategory.Residential));
            registry.Register(new ZoneDefinition(new ZoneTypeId(9), "I", ZoneCategory.Industrial));
            
            var defs = registry.Definitions;
            Assert.Equal(3, defs.Count);
            Assert.Equal(2, defs[0].Id.Value);
            Assert.Equal(5, defs[1].Id.Value);
            Assert.Equal(9, defs[2].Id.Value);
        }

        [Fact]
        public void ZoneDefinition_Validation()
        {
            Assert.Throws<ArgumentException>(() =>
                new ZoneDefinition(ZoneTypeId.None, "Test", ZoneCategory.Residential));
            
            Assert.Throws<ArgumentException>(() =>
                new ZoneDefinition(new ZoneTypeId(1), null!, ZoneCategory.Residential));
            
            Assert.Throws<ArgumentException>(() =>
                new ZoneDefinition(new ZoneTypeId(1), "", ZoneCategory.Residential));
            
            Assert.Throws<ArgumentException>(() =>
                new ZoneDefinition(new ZoneTypeId(1), "   ", ZoneCategory.Residential));
        }

        [Fact]
        public void Grid_ConstructionValidation()
        {
            var registry = CreateRegistry();
            
            Assert.Throws<ArgumentNullException>(() => 
                new ZoneGrid(null!, -5, -5, 1, 10, 10));
            
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new ZoneGrid(registry, double.NaN, -5, 1, 10, 10));
            
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new ZoneGrid(registry, double.PositiveInfinity, -5, 1, 10, 10));
            
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new ZoneGrid(registry, -5, double.NaN, 1, 10, 10));
            
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new ZoneGrid(registry, -5, -5, double.NaN, 10, 10));
            
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new ZoneGrid(registry, -5, -5, 0, 10, 10));
            
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new ZoneGrid(registry, -5, -5, -1, 10, 10));
            
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new ZoneGrid(registry, -5, -5, 1, 0, 10));
            
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new ZoneGrid(registry, -5, -5, 1, 10, 0));
            
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new ZoneGrid(registry, -5, -5, 1, 2001, 2001));
            
            var grid2000 = new ZoneGrid(registry, -5, -5, 1, 2000, 2000);
            Assert.Equal(2000, grid2000.CellsX);
        }

        [Fact]
        public void Grid_InitialState()
        {
            var grid = CreateGrid();
            Assert.Equal(-5.0, grid.MinX);
            Assert.Equal(-5.0, grid.MinZ);
            Assert.Equal(1.0, grid.CellSize);
            Assert.Equal(10, grid.CellsX);
            Assert.Equal(10, grid.CellsZ);
            Assert.Equal(5.0, grid.MaxX);
            Assert.Equal(5.0, grid.MaxZ);
            Assert.Equal(0, grid.PaintedCellCount);
            Assert.Equal(0, grid.PaintedCellCount);
            Assert.Equal(100, grid.CountCells(ZoneTypeId.None));
        }

        [Fact]
        public void Grid_GetSet()
        {
            var grid = CreateGrid();
            
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(-1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(10, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(0, 10));
            
            Assert.Equal(ZoneTypeId.None, grid.Get(0, 0));
            
            Assert.True(grid.Set(0, 0, new ZoneTypeId(1)));
            Assert.Equal(new ZoneTypeId(1), grid.Get(0, 0));
            
            Assert.False(grid.Set(0, 0, new ZoneTypeId(1)));
            
            Assert.True(grid.Set(0, 0, ZoneTypeId.None));
            Assert.Equal(ZoneTypeId.None, grid.Get(0, 0));
            
            Assert.Throws<ArgumentException>(() => grid.Set(0, 0, new ZoneTypeId(99)));
            
            // Verify unchanged after exception
            Assert.Equal(ZoneTypeId.None, grid.Get(0, 0));
        }

        [Fact]
        public void Grid_TryGetCellAt()
        {
            var grid = CreateGrid();
            
            Assert.True(grid.TryGetCellAt(-5, -5, out var cell));
            Assert.Equal(0, cell.X);
            Assert.Equal(0, cell.Z);
            
            Assert.True(grid.TryGetCellAt(0, 0, out cell));
            Assert.Equal(5, cell.X);
            Assert.Equal(5, cell.Z);
            
            Assert.True(grid.TryGetCellAt(4.999, 4.999, out cell));
            Assert.Equal(9, cell.X);
            Assert.Equal(9, cell.Z);
            
            Assert.True(grid.TryGetCellAt(5, 5, out cell));
            Assert.Equal(9, cell.X);
            Assert.Equal(9, cell.Z);
            
            Assert.False(grid.TryGetCellAt(5.001, 0, out _));
            Assert.False(grid.TryGetCellAt(0, -5.001, out _));
            Assert.False(grid.TryGetCellAt(double.NaN, 0, out _));
            Assert.False(grid.TryGetCellAt(0, double.PositiveInfinity, out _));
        }

        [Fact]
        public void Grid_CellCenter()
        {
            var grid = CreateGrid();
            
            Assert.Equal(-4.5, grid.CellCenterX(0));
            Assert.Equal(4.5, grid.CellCenterX(9));
            Assert.Equal(-4.5, grid.CellCenterZ(0));
            Assert.Equal(4.5, grid.CellCenterZ(9));
            
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.CellCenterX(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.CellCenterX(10));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.CellCenterZ(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.CellCenterZ(10));
        }

        [Fact]
        public void Grid_GetBrushCells()
        {
            var grid = CreateGrid();
            
            var cells = grid.GetBrushCells(0, 0, 0.5);
            Assert.Empty(cells);
            
            cells = grid.GetBrushCells(0, 0, 0.8);
            Assert.Equal(4, cells.Count);
            Assert.Equal((4, 4), (cells[0].X, cells[0].Z));
            Assert.Equal((5, 4), (cells[1].X, cells[1].Z));
            Assert.Equal((4, 5), (cells[2].X, cells[2].Z));
            Assert.Equal((5, 5), (cells[3].X, cells[3].Z));
            
            cells = grid.GetBrushCells(0, 0, 1.6);
            Assert.Equal(12, cells.Count);
            
            cells = grid.GetBrushCells(0.5, 0.5, 1.0);
            Assert.Equal(5, cells.Count);
            
            cells = grid.GetBrushCells(-5, -5, 1.0);
            Assert.Single(cells);
            Assert.Equal((0, 0), (cells[0].X, cells[0].Z));
            
            cells = grid.GetBrushCells(-5, -5, 1.6);
            Assert.Equal(3, cells.Count);
            
            cells = grid.GetBrushCells(50, 50, 2);
            Assert.Empty(cells);
            
            cells = grid.GetBrushCells(-5.3, 0, 1.0);
            Assert.NotEmpty(cells);
            foreach (var cell in cells)
            {
                Assert.True(cell.X >= 0 && cell.X < 10);
                Assert.True(cell.Z >= 0 && cell.Z < 10);
            }
            
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetBrushCells(double.NaN, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetBrushCells(0, double.PositiveInfinity, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetBrushCells(0, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetBrushCells(0, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetBrushCells(0, 0, double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetBrushCells(0, 0, double.PositiveInfinity));
            
            // Verify row-major order
            for (int i = 1; i < cells.Count; i++)
            {
                var prev = cells[i - 1];
                var curr = cells[i];
                Assert.True(curr.Z > prev.Z || (curr.Z == prev.Z && curr.X > prev.X));
            }
        }

        [Fact]
        public void Grid_PaintAndErase()
        {
            var grid = CreateGrid();
            
            int changed = grid.Paint(0, 0, 0.8, new ZoneTypeId(1));
            Assert.Equal(4, changed);
            Assert.Equal(4, grid.CountCells(new ZoneTypeId(1)));
            Assert.Equal(4, grid.PaintedCellCount);
            
            changed = grid.Paint(0, 0, 0.8, new ZoneTypeId(1));
            Assert.Equal(0, changed);
            
            changed = grid.Paint(0, 0, 0.8, new ZoneTypeId(2));
            Assert.Equal(4, changed);
            Assert.Equal(0, grid.CountCells(new ZoneTypeId(1)));
            Assert.Equal(4, grid.CountCells(new ZoneTypeId(2)));
            
            changed = grid.Erase(0, 0, 0.8);
            Assert.Equal(4, changed);
            Assert.Equal(0, grid.CountCells(new ZoneTypeId(2)));
            Assert.Equal(0, grid.PaintedCellCount);
            
            changed = grid.Erase(50, 50, 0.8);
            Assert.Equal(0, changed);
            
            Assert.Throws<ArgumentException>(() => grid.Paint(0, 0, 0.8, new ZoneTypeId(99)));
            
            // Painting None through Paint should behave like Erase
            grid.Paint(0, 0, 0.8, new ZoneTypeId(1));
            changed = grid.Paint(0, 0, 0.8, ZoneTypeId.None);
            Assert.Equal(4, changed);
            Assert.Equal(0, grid.CountCells(new ZoneTypeId(1)));
        }

        [Fact]
        public void Grid_GetRawCells()
        {
            var grid = CreateGrid();
            grid.Paint(0, 0, 0.8, new ZoneTypeId(1));
            
            var raw1 = grid.GetRawCells();
            Assert.Equal(100, raw1.Count);
            
            var raw2 = grid.GetRawCells();
            Assert.Equal(raw1, raw2);
            
            // Verify copy semantics by checking values match Get
            for (int z = 0; z < 10; z++)
            {
                for (int x = 0; x < 10; x++)
                {
                    int idx = z * 10 + x;
                    Assert.Equal(grid.Get(x, z).Value, raw1[idx]);
                }
            }
        }

        [Fact]
        public void Grid_FuzzTest()
        {
            var registry = CreateRegistry();
            var grid = CreateGrid(registry);
            var model = new int[100];
            var rng = new DeterministicRandom(1234);
            
            int operationsChanged = 0;
            
            for (int i = 0; i < 2000; i++)
            {
                double worldX = rng.NextDouble() * 14.0 - 7.0;
                double worldZ = rng.NextDouble() * 14.0 - 7.0;
                double radius = rng.NextDouble() * 2.8 + 0.2;
                
                int choice = rng.NextInt(0, 9);
                ZoneTypeId zone;
                bool isErase = choice == 0;
                
                if (isErase)
                {
                    zone = ZoneTypeId.None;
                }
                else
                {
                    zone = new ZoneTypeId(rng.NextInt(1, 9));
                }
                
                // Compute expected using model
                int expectedChanged = 0;
                var cellsToChange = new List<(int x, int z)>();
                
                for (int zIdx = 0; zIdx < 10; zIdx++)
                {
                    double centerZ = -5.0 + (zIdx + 0.5) * 1.0;
                    double dz = centerZ - worldZ;
                    double dzSq = dz * dz;
                    if (dzSq > radius * radius) continue;
                    
                    for (int xIdx = 0; xIdx < 10; xIdx++)
                    {
                        double centerX = -5.0 + (xIdx + 0.5) * 1.0;
                        double dx = centerX - worldX;
                        double distSq = dx * dx + dzSq;
                        
                        if (distSq <= radius * radius)
                        {
                            int idx = zIdx * 10 + xIdx;
                            if (model[idx] != zone.Value)
                            {
                                expectedChanged++;
                                cellsToChange.Add((xIdx, zIdx));
                            }
                        }
                    }
                }
                
                int actualChanged;
                if (isErase)
                {
                    actualChanged = grid.Erase(worldX, worldZ, radius);
                }
                else
                {
                    actualChanged = grid.Paint(worldX, worldZ, radius, zone);
                }
                
                Assert.Equal(expectedChanged, actualChanged);
                if (actualChanged > 0)
                    operationsChanged++;
                
                // Update model
                foreach (var (x, z) in cellsToChange)
                {
                    model[z * 10 + x] = zone.Value;
                }
                
                // Check PaintedCellCount
                int paintedCount = grid.PaintedCellCount;
                int modelPainted = model.Count(v => v != 0);
                Assert.Equal(modelPainted, paintedCount);
            }
            
            Assert.True(operationsChanged >= 500);
            
            // Final verification
            var raw = grid.GetRawCells();
            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(model[i], raw[i]);
            }
            
            // Run again with same seed and verify identical result
            var grid2 = CreateGrid(registry);
            var model2 = new int[100];
            var rng2 = new DeterministicRandom(1234);
            
            for (int i = 0; i < 2000; i++)
            {
                double worldX = rng2.NextDouble() * 14.0 - 7.0;
                double worldZ = rng2.NextDouble() * 14.0 - 7.0;
                double radius = rng2.NextDouble() * 2.8 + 0.2;
                
                int choice = rng2.NextInt(0, 9);
                ZoneTypeId zone = choice == 0 ? ZoneTypeId.None : new ZoneTypeId(rng2.NextInt(1, 9));
                
                if (choice == 0)
                    grid2.Erase(worldX, worldZ, radius);
                else
                    grid2.Paint(worldX, worldZ, radius, zone);
                
                // Update model2 same as above
                for (int zIdx = 0; zIdx < 10; zIdx++)
                {
                    double centerZ = -5.0 + (zIdx + 0.5) * 1.0;
                    double dz = centerZ - worldZ;
                    double dzSq = dz * dz;
                    if (dzSq > radius * radius) continue;
                    
                    for (int xIdx = 0; xIdx < 10; xIdx++)
                    {
                        double centerX = -5.0 + (xIdx + 0.5) * 1.0;
                        double dx = centerX - worldX;
                        double distSq = dx * dx + dzSq;
                        
                        if (distSq <= radius * radius)
                        {
                            model2[zIdx * 10 + xIdx] = zone.Value;
                        }
                    }
                }
            }
            
            var raw2 = grid2.GetRawCells();
            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(raw[i], raw2[i]);
            }
        }
    }
}
