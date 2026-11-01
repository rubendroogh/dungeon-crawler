using Godot;

/// <summary>
/// Inspector-editable palette and layout measurements shared by both map views.
/// </summary>
[GlobalClass]
public partial class DungeonMapStyle : Resource
{
    [ExportGroup("Layout")]
    [Export]
    public Font Font { get; set; }
    [Export]
    public Vector2 MinimumSize { get; set; }
    [ExportGroup("Palette")]
    [Export]
    public Color Gold { get; set; }
    [Export]
    public Color Ink { get; set; }
    [Export]
    public Color Background { get; set; }
    [Export]
    public Color Board { get; set; }
    [Export]
    public Color Grid { get; set; }
    [Export]
    public Color Divider { get; set; }
    [Export]
    public Color Border { get; set; }
    [Export]
    public Color Visited { get; set; }
    [Export]
    public Color Unexplored { get; set; }
    [Export]
    public Color Corridor { get; set; }
    [Export]
    public Color CorridorShadow { get; set; }
    [Export]
    public Color CurrentRoom { get; set; }
    [Export]
    public Color VisitedRoom { get; set; }
    [Export]
    public Color UnexploredRoom { get; set; }
    [Export]
    public Color CurrentBorder { get; set; }
    [Export]
    public Color Completed { get; set; }
    [Export]
    public Color CompletedBackground { get; set; }
    [Export]
    public Color Boss { get; set; }
    [ExportGroup("Measurements")]
    [Export]
    public float BoardTop { get; set; }
    [Export]
    public float HorizontalPadding { get; set; }
    [Export]
    public float FooterHeight { get; set; }
    [Export]
    public float RoomInset { get; set; }
    [Export]
    public int HeadingFontSize { get; set; }
    [Export]
    public int DetailFontSize { get; set; }
    [Export]
    public int CaptionFontSize { get; set; }
    [Export]
    public int CoordinateFontSize { get; set; }
}
