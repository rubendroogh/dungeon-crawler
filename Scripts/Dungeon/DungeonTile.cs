/// <summary>
/// Represents a single tile in the dungeon.
/// Each tile can be of a specific type, such as wall or open path.
/// </summary>
public partial class DungeonTile
{
    /// <summary>
    /// The type of the dungeon tile.
    /// </summary>
    public DungeonTileType Type { get; set; }
}

/// <summary>
/// The possible types of dungeon tiles.
/// </summary>
public enum DungeonTileType
{
    /// <summary>
    /// Represents an impassible wall tile.
    /// </summary>
    Wall,

    /// <summary>
    /// Represents an empty open path tile.
    /// </summary>
    Open
}