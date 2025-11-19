using DungeonRPG.Blessings.Enums;
using Godot;

/// <summary>
/// A part of the blessing bar UI representing a single blessing.
/// </summary>
public partial class BlessingUI : TextureRect
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

        Modulate = Blessing.GetColor();
        Texture = new AtlasTexture
        {
            Atlas = Blessing.GetTexture(),
            Region = new Rect2(Vector2.Zero, new Vector2(xSize, ySize))
        };

        CustomMinimumSize = new Vector2((int)Blessing.Level, 48);

        SetLabelText();
        InitializeCustomSignals();

        // Return this for chaining
        return this;
    }

    public override void _GuiInput(InputEvent @event)
    {
        // Handle hover state
        if (Managers.ActionManager.SpellIsSelected)
        {
            if (Blessing.State != State.Spent)
            {
                // Show hover state
            }

            // On click
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (Blessing.State == State.MarkedForUse)
                {
                    // If already marked for use, deselect
                    Blessing.State = State.Available;
                    SetSelectedVisual(false);
                }
                else if (Blessing.State == State.Available)
                {
                    // Select
                    Blessing.State = State.MarkedForUse;
                    SetSelectedVisual(true);
                }
            }
        }
    }

    /// <summary>
    /// Shows the blessing as selected visually.
    /// </summary>
    private void SetSelectedVisual(bool value)
    {
        if (value)
        {
            TextLabel.Text = "Selected";
        }
        else
        {
            TextLabel.Text = "Not seoected";
        }
    }

    /// <summary>
    /// Initialize the OnManaStateChanged signals.
    /// </summary>
    private void InitializeCustomSignals()
    {
        Managers.ManaSourceManager.BlessingStateChanged += OnManaStateChanged;
    }

    /// <summary>
    /// Handles visual feedback of mana state changed.
    /// </summary>
    private void OnManaStateChanged()
    {
        Modulate = Blessing.GetColor();
        SetSelectedVisual(Blessing.State == State.MarkedForUse);
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
