using Godot;

/// <summary>
/// A part of the blessing bar UI representing a single blessing.
/// </summary>
public partial class BlessingUI : TextureProgressBar
{
    /// <summary>
    /// The blessing represented by this UI element.
    /// </summary>
    public Blessing Blessing { get; private set; }

    /// <summary>
    /// The label displaying the blessing's domain and level.
    /// </summary>
    private Label TextLabel => GetNode<Label>("Label");

    /// <summary>
    /// Sets up the BlessingUI with the given blessing.
    /// </summary>
    public BlessingUI Setup(Blessing blessing)
    {
        Blessing = blessing;

        var xSize = GetXSize();
        var ySize = 48;

        TintProgress = Blessing.GetColor();
        TextureProgress = new AtlasTexture
        {
            Atlas = Blessing.GetTexture(),
            Region = new Rect2(Vector2.Zero, new Vector2(xSize, ySize))
        };

        MaxValue = (int)Blessing.Level;
        Value = (int)Blessing.Level;

        SetLabelText();

        // Return this for chaining
        return this;
    }

    /// <summary>
    /// Sets the label text to display the blessing's domain and level.
    /// </summary>
    private void SetLabelText()
    {
        TextLabel.Text = $"{Blessing.Domain} ({Blessing.Level})";
    }

    /// <summary>
    /// Calculates the X size of the blessing UI based on its level.
    /// </summary>
    private int GetXSize()
    {
        var fullWidth = Managers.ManaSourceManager.Width;
        var percentage = (float)Blessing.Level / Managers.ManaSourceManager.BlessingBar.MaxMana;
        return (int)(fullWidth * percentage);
    }
}
