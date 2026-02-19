using Godot;

namespace Priest.DungeonGeneration;

/// <summary>
/// A Tile is the basis of every static object found in the world.
/// </summary>
public class Tile
{
    /// <summary>
    /// The drawn sprite of the tile.
    /// </summary>
    public Vector2I Sprite { get; set; }
}