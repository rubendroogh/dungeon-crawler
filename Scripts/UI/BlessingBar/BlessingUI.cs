using Godot;

/// <summary>
/// A part of the blessing bar UI representing a single blessing.
/// </summary>
public partial class BlessingUI : TextureProgressBar
{
    public Blessing Blessing { get; private set; }

    public BlessingUI Setup(Blessing blessing)
    {
        Blessing = blessing;
        
        var xSize = 48 * (int)blessing.Level;
        var ySize = 48;

        TintProgress = Blessing.GetColor();
        TextureProgress = new AtlasTexture
        {
            Atlas = Blessing.GetTexture(),
            Region = new Rect2(Vector2.Zero, new Vector2(xSize, ySize))
        };
        
        MaxValue = (int)Blessing.Level;
        Value = (int)Blessing.Level;

        // Return this for chaining
        return this;
    }
}
