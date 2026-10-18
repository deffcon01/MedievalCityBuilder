using Godot;
using System;

using MedievalCityBuilder.Persistence;
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
    [Export] public double SeaLevel = 0.35;         // PLACEHOLDER: pending visual tuning

    private MeshInstance3D? _meshInstance;
    private ArrayMesh? _arrayMesh;
    private TerrainMeshData? _meshData;

    /// <summary>
    /// World extents in XZ after centering. Returns (minX, maxX, minZ, maxZ).
    /// </summary>
    public (float minX, float maxX, float minZ, float maxZ) WorldExtentsXZ { get; private set; }

    /// <summary>
    /// Exposed HeightScale for other nodes (e.g., CameraRig) to read.
    /// </summary>
    public double HeightScaleValue => HeightScale;

    /// <summary>
    /// The current world data used to build the terrain and water.
    /// </summary>
    public WorldSaveData CurrentWorld { get; private set; } = null!;

    /// <summary>
    /// The terrain surface used for raycasting and height sampling.
    /// </summary>
    public TerrainSurface Surface { get; private set; } = null!;

    /// <summary>
    /// The water settings for the current world.
    /// </summary>
    public WaterSettings Water { get; private set; } = null!;

    /// <summary>
    /// Invoked after ApplyWorld completes and the new world is ready.
    /// </summary>
    public event Action? WorldApplied;

    public override void _Ready()
    {
        // Build initial world data from exports and apply
        var generation = new WorldGenerationSettings(
            (ulong)Seed,
            WorldGenerationSettings.CurrentGeneratorVersion,
            WorldSize
        );
        var terrainSettings = TerrainSettings.Default;
        var waterSettings = new WaterSettings(SeaLevel);
        var initialData = new WorldSaveData(generation, terrainSettings, waterSettings);
        ApplyWorld(initialData);
    }

    /// <summary>
    /// Builds or rebuilds the terrain mesh, water plane, and world extents from the provided world data.
    /// Removes all previously built children to avoid name clashes.
    /// </summary>
    public void ApplyWorld(WorldSaveData data)
    {
        // Remove all previously built children
        foreach (var child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }

        var worldSettings = data.Generation;
        var terrainSettings = data.Terrain;
        var waterSettings = data.Water;
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

        // Create water plane
        var waterPlaneMesh = new PlaneMesh
        {
            Size = new Vector2(extentX, extentZ),
            SubdivideWidth = 1,
            SubdivideDepth = 1,
        };
        var waterMeshInstance = new MeshInstance3D
        {
            Mesh = waterPlaneMesh,
            Name = "WaterPlane"
        };
        // Local position: center of the plane in XZ, Y at SeaLevel * HeightScale
        // The plane is centered at its local origin, so position at (extentX/2, y, extentZ/2) relative to the uncentered terrain
        // But the terrain is now centered at Position = (-extentX/2, 0, -extentZ/2), so we need to place water at
        // local position (extentX/2, SeaLevel * HeightScale, extentZ/2) relative to TerrainView
        waterMeshInstance.Position = new Vector3(extentX * 0.5f, (float)(waterSettings.SeaLevel * HeightScale), extentZ * 0.5f);

        var waterMaterial = new StandardMaterial3D
        {
            AlbedoColor = new Color(0.1f, 0.3f, 0.7f, 0.6f),
            Roughness = 0.2f,
            Transparency = StandardMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = StandardMaterial3D.ShadingModeEnum.PerPixel,
            CullMode = StandardMaterial3D.CullModeEnum.Back,
            RenderPriority = 1, // Render after opaque terrain
        };
        waterMeshInstance.SetSurfaceOverrideMaterial(0, waterMaterial);
        waterMeshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off; // No shadows

        AddChild(waterMeshInstance);

        // Log terrain and water info
        double underwaterFraction = WaterAnalysis.UnderwaterFraction(heightmap, waterSettings);
        GD.Print($"TerrainView: seed={data.Generation.Seed}, size={data.Generation.Size}, vertices={vertexCount}, extents=({WorldExtentsXZ.minX}, {WorldExtentsXZ.maxX}, {WorldExtentsXZ.minZ}, {WorldExtentsXZ.maxZ})");
        GD.Print($"WaterPlane: SeaLevel={data.Water.SeaLevel}, underwaterFraction={underwaterFraction:F4}");

        CurrentWorld = data;
        Surface = TerrainSurface.Centered(heightmap, CellSize, HeightScale);
        Water = data.Water;
        WorldApplied?.Invoke();
    }
}