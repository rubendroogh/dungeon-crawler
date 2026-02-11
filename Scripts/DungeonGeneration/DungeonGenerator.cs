namespace Priest.DungeonGeneration;

/// <summary>
/// The DungeonGenerator is responsible for generating the world,
/// either on load, or when getting close to the generated edge.
/// </summary>
public class DungeonGenerator
{
    /// <summary>
    /// The dungeon's chunks.
    /// </summary>
    public Chunk[,] Chunks { get; private set; }

    /// <summary>
    /// The width of the intial generation in chunks.
    /// </summary>
    private int InitialWidth { get; set; } = 8;

    /// <summary>
    /// The height of the intial generation in chunks.
    /// </summary>
    private int InitialHeight { get; set; } = 8;

    /// <summary>
    /// The initial generation when building a new dungeon.
    /// </summary>
    public void InitialGenerate()
    {
        for (int x = 0; x < InitialWidth; x++)
        {
            for (int y = 0; y < InitialHeight; y++)
            {
                
            }
        }
    }

    public void DrawTiles()
    {
        
    }
}