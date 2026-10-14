using Godot;

using MedievalCityBuilder.World;

namespace MedievalCityBuilder.Presentation;

/// <summary>
/// Terrain presentation node. Generates a heightmap and builds a terrain mesh for display.
/// All simulation logic stays in src/World; this class only handles presentation.
/// </summary>
public partial class TerrainView : Node3D
{
    // --- Tunable values (PLACEHOLDER) ---

    [Export] public long Seed = 12345;              // PLACEHOLDER
    [Export] public WorldSize WorldSize = WorldSize.Small; // PLACEHOLDER
    [Export] public double CellSize = 1.0;          // PLACEHOLDER
    [Export] public double HeightScale = 10.0;      // PLACEHOLDER

    private MeshInstance3D? _meshInstance;
    private ArrayMesh? _arrayMesh;
    private TerrainMeshData? _meshData;

    /// <summary>
    /// World extents in XZ after centering. Returns (minX, maxX, minZ, maxZ).
    /// </summary>
    public (float minX, float maxX, float minZ, float maxZ) WorldExtentsXZ { get; private set; }

    public override void _Ready()
    {
        // Build world generation settings
        var worldSettings = new WorldGenerationSettings(
            (ulong)Seed,
            WorldGenerationSettings.CurrentGeneratorVersion,
            WorldSize
        );

        var terrainSettings = TerrainSettings.Default;
        var table = new WorldSizeTable();

        // Generate heightmap
        var heightmap = TerrainGenerator.Generate(worldSettings, terrainSettings, table);

        // Build mesh data
        _meshData = TerrainMeshData.Build(heightmap, CellSize, HeightScale);

        // Create ArrayMesh
        _arrayMesh = new ArrayMesh();
        _meshInstance = new MeshInstance3D
        {
            Mesh = _arrayMesh
        };

        // Prepare vertex arrays for Godot
        int vertexCount = _meshData.VertexCount;
        var vertices = new Vector3[vertexCount];
        var normals = new Vector3[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            int pi = i * 3;
            int ni = i * 3;
            vertices[i] = new Vector3(_meshData.Positions[pi], _meshData.Positions[pi + 1], _meshData.Positions[pi + 2]);
            normals[i] = new Vector3(_meshData.Normals[ni], _meshData.Normals[ni + 1], _meshData.Normals[ni + 2]);
        }

        var indices = _meshData.Indices;

        // Create arrays for ArrayMesh
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);
        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices;
        arrays[(int)ArrayMesh.ArrayType.Normal] = normals;
        arrays[(int)ArrayMesh.ArrayType.Index] = indices;

        _arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        // Create material (opaque, green albedo, default shading)
        var material = new StandardMaterial3D
        {
            AlbedoColor = new Color(0.2f, 0.6f, 0.2f, 1.0f), // Plain green
            ShadingMode = StandardMaterial3D.ShadingModeEnum.PerPixel,
            Transparency = StandardMaterial3D.TransparencyEnum.Disabled,
        };
        _meshInstance.SetSurfaceOverrideMaterial(0, material);

        AddChild(_meshInstance);

        // Center the terrain on world origin
        // Terrain spans from (0, 0) to (width * CellSize, height * CellSize) in XZ
        // We want to offset by -half extent in X and Z
        float extentX = (float)((heightmap.Width - 1) * CellSize);
        float extentZ = (float)((heightmap.Height - 1) * CellSize);
        Position = new Vector3(-extentX * 0.5f, 0f, -extentZ * 0.5f);

        // Compute world extents after centering
        WorldExtentsXZ = (-extentX * 0.5f, extentX * 0.5f, -extentZ * 0.5f, extentZ * 0.5f);

        GD.Print($"TerrainView: seed={Seed}, size={WorldSize}, vertices={vertexCount}, extents=({WorldExtentsXZ.minX}, {WorldExtentsXZ.maxX}, {WorldExtentsXZ.minZ}, {WorldExtentsXZ.maxZ})");
    }
}