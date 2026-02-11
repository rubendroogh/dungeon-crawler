namespace Priest.DungeonGeneration;

public class Chunk(Tile[,] tiles)
{
    /// <summary>
    /// A 2-dimensional array containing the tile data
    /// of the chunk.
    /// </summary>
    public Tile[,] Tiles { get; private set; } = tiles;

    /// <summary>
    /// The width of the chunk.
    /// </summary>
    private static int Width = 64;

    /// <summary>
    /// The height of the chunk.
    /// </summary>
    private static int Height = 64;

    /// <summary>
    /// Gets the size of a chunk. A chunk is always square,
    /// so it's both width and height.
    /// </summary>
    public static int ChunkSize => Width;

    
}