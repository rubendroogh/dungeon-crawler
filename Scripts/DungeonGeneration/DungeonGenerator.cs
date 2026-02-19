using System;
using Godot;

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

    private FastNoiseLite Noise;

    /// <summary>
    /// The initial generation when building a new dungeon.
    /// </summary>
    public void InitialGenerate()
    {
        var fullXSize = InitialWidth * Chunk.ChunkSize;
        var fullYSize = InitialHeight * Chunk.ChunkSize;

        var halfXSize = fullXSize / 2;
        var halfYSize = fullYSize / 2;

        Noise = new FastNoiseLite
        {
            NoiseType = FastNoiseLite.NoiseTypeEnum.Cellular,
            Seed = new Random().Next()
        };

        for (int x = -halfXSize; x < halfXSize; x++)
        {
            for (int y = -halfYSize; y < halfYSize; y++)
            {
                // Get the chunk we're in now:
                
                GenerateAt(new(x, y));
            }
        }
    }

    private void GenerateAt(Vector2I coordinates)
    {
        // GD.Print(Noise.GetNoise2D(coordinates.X, coordinates.Y));
        var noise = Noise.GetNoise2D(coordinates.X, coordinates.Y);
        if (noise < -0.6 && noise > -0.8)
        {
            var tile = new Tile
            {
                Sprite = Tiles.GroundTile
            };

            DungeonTileDrawer.Instance.Draw(coordinates, tile);
        }
    }
}

public static class Tiles
{
    public static Vector2I GroundTile = new(19, 1);

    public static Vector2I RockTile = new(1, 17);
}