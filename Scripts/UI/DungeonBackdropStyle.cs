using Godot;

/// <summary>
/// Shared masonry palette and perspective configuration for the dungeon backdrop.
/// </summary>
[GlobalClass]
public partial class DungeonBackdropStyle : Resource
{
    [ExportGroup("Perspective and masonry")]
    [Export]
    public float PixelSize { get; set; }
    [Export]
    public float WallInsetRatio { get; set; }
    [Export]
    public float BackWallTopRatio { get; set; }
    [Export]
    public float EnemyFootOffset { get; set; }
    [Export]
    public Vector2I BrickSize { get; set; }
    [Export]
    public int WallRows { get; set; }
    [Export]
    public int WallColumns { get; set; }
    [Export]
    public int FloorRows { get; set; }
    [Export]
    public int FloorColumns { get; set; }
    [Export]
    public int ShadowLayers { get; set; }
    [Export]
    public float DoorRadiusRatio { get; set; }
    [Export]
    public int TorchGlowRadius { get; set; }
    [ExportGroup("Palette")]
    [Export]
    public Color Surround { get; set; }
    [Export]
    public Color Ceiling { get; set; }
    [Export]
    public Color Mortar { get; set; }
    [Export]
    public Color BackStone { get; set; }
    [Export]
    public Color BackHighlight { get; set; }
    [Export]
    public Color BackWear { get; set; }
    [Export]
    public Color BackShadow { get; set; }
    [Export]
    public Color WallHighlight { get; set; }
    [Export]
    public Color WallJoint { get; set; }
    [Export]
    public Color DeepJoint { get; set; }
    [Export]
    public Color Crack { get; set; }
    [Export]
    public Color WallFoot { get; set; }
    [Export]
    public Color FloorHighlight { get; set; }
    [Export]
    public Color DoorVoid { get; set; }
    [Export]
    public Color ArchJoint { get; set; }
    [Export]
    public Color ArchHighlight { get; set; }
    [Export]
    public Color LeftPier { get; set; }
    [Export]
    public Color RightPier { get; set; }
    [Export]
    public Color PierHighlight { get; set; }
    [Export]
    public Color Threshold { get; set; }
    [Export]
    public Color ThresholdShadow { get; set; }
    [Export]
    public Color TorchIron { get; set; }
    [Export]
    public Color TorchWood { get; set; }
    [Export]
    public Color FlameOuter { get; set; }
    [Export]
    public Color FlameInner { get; set; }
    [Export]
    public Color FlameCore { get; set; }
    [Export]
    public Color WallStone { get; set; }
    [Export]
    public Color FloorStone { get; set; }
    [Export]
    public Color ArchStone { get; set; }
    [Export]
    public Color CeilingShadow { get; set; }
    [Export]
    public Color FloorShadow { get; set; }
    [Export]
    public Color TorchGlow { get; set; }
}
