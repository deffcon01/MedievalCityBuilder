using Godot;

namespace MedievalCityBuilder.Presentation;

public partial class Main : Node3D
{
    public override void _Ready()
    {
        GD.Print("MedievalCityBuilder: Main scene loaded");

        // Children run _Ready before parent, so TerrainView is already ready
        var terrainView = GetNode<TerrainView>("TerrainView");
        var cameraRig = GetNode<CameraRig>("CameraRig");

        // Inset bounds by 2 world units as placeholder margin
        var (minX, maxX, minZ, maxZ) = terrainView.WorldExtentsXZ;
        float margin = 2.0f;
        cameraRig.SetBounds(minX + margin, maxX - margin, minZ + margin, maxZ - margin);

        // Raise CameraRig pivot Y to HeightScale * 0.5 as placeholder
        cameraRig.Position = new Vector3(cameraRig.Position.X, (float)(terrainView.HeightScaleValue * 0.5), cameraRig.Position.Z);
    }
}