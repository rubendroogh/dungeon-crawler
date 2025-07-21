using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// DungeonGenerator is responsible for creating a dungeon layout.
/// </summary>
public partial class DungeonGenerator
{
    /// <summary>
    /// 2D array representing the dungeon grid.
    /// </summary>
    private DungeonTile[,] Dungeon { get; set; }

    /// <summary>
    /// Width of the dungeon grid. This should be a positive odd integer.
    /// </summary>
    private int Width { get; set; }

    /// <summary>
    /// Height of the dungeon grid. This should be a positive odd integer.
    /// </summary>
    private int Height { get; set; }

    /// <summary>
    /// Probability of creating loops in the dungeon.
    /// A value between 0 and 1, where 0 means no loops and 1 means always create loops.
    /// </summary>
    private float LoopProbability { get; set; }

    /// <summary>
    /// Percentage of the dungeon that should be open paths.
    /// A value between 0 and 1, where 0 means no open paths and 1 means all paths are open.
    /// </summary>
    private float OpenPathPercentage { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DungeonGenerator"/> class.
    /// This constructor sets up the dungeon grid with the specified dimensions and properties.
    /// </summary>
    public DungeonGenerator(int width, int height, float loopProbability, float openPathPercentage)
    {
        Width = width;
        Height = height;
        LoopProbability = loopProbability;
        OpenPathPercentage = openPathPercentage;

        Dungeon = new DungeonTile[Width, Height];
        InitializeGrid();
    }

    /// <summary>
    /// Generates the dungeon layout based on the parameters specified in the constructor.
    /// </summary>
    public void Generate()
    {
        throw new NotImplementedException("Dungeon generation logic is not implemented yet.");
    }

    /// <summary>
    /// Initializes the dungeon grid with walls.
    /// Each tile in the grid is set to a wall type by default.
    /// </summary>
    private void InitializeGrid()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Dungeon[x, y] = new DungeonTile
                {
                    Type = DungeonTileType.Wall
                };
            }
        }
    }
}
