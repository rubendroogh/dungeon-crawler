using Godot;

namespace Priest.DungeonGeneration;

/// <summary>
/// This manager is responsible for drawing the generated tiles
/// to the actual tilemaps.
/// </summary>
public partial class DungeonTileDrawer : Node
{
    /// <summary>
    /// The ground layer containing the most lower tiles.
    /// </summary>
    [Export]
    public TileMapLayer GroundLayer;

    public static DungeonTileDrawer Instance;

    public override void _Ready()
    {
        Instance = this;
        var dg = new DungeonGenerator();
        dg.InitialGenerate();
    }

    /// <summary>
    /// Draws a single tile at a coordinate.
    /// </summary>
    public void Draw(Vector2I coordinates, Tile tile)
    {
        GroundLayer.SetCell(coordinates, 0, tile.Sprite);
    }
}