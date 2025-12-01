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
    /// Used to track if the blessing should show the hover state.
    /// </summary>
    private bool IsHovering { get; set; }

    /// <summary>
    /// Sets up the BlessingUI with the given blessing.
    /// </summary>
    public BlessingUI Setup(Blessing blessing)
    {
        Blessing = blessing;

        var xSize = GetXSize();
        var ySize = 48;

        SelfModulate = Blessing.GetColor();
        Texture = new AtlasTexture
        {
            Atlas = Blessing.GetTexture(),
            Region = new Rect2(Vector2.Zero, new Vector2(xSize, ySize))
        };

        CustomMinimumSize = new Vector2((int)Blessing.Level, 48);

        SetLabelText();
        InitializeCustomSignals();

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;

        // Return this for chaining
        return this;
    }

    public override void _GuiInput(InputEvent @event)
    {
        // Show popup panel
        

        if (!Managers.ActionManager.SpellIsSelected)
        {
            return;
        }

        if (Blessing.State != State.Spent)
        {
            // TODO: Show hover state
        }

        // On click
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            if (Blessing.State == State.MarkedForUse)
            {
                // If already marked for use, deselect
                Managers.ManaSourceManager.BlessingBar.SetBlessingState(Blessing.ID, State.Available);
                OnManaStateChanged();
            }
            else if (Blessing.State == State.Available)
            {
                Managers.ManaSourceManager.BlessingBar.SetBlessingState(Blessing.ID, State.MarkedForUse);
                OnManaStateChanged();
            }
        }
    }

    /// <summary>
    /// Handles mouse enter event to show hover state.
    /// </summary>
    private void OnMouseEntered()
    {
        IsHovering = true;
        Managers.TooltipManager.Show("Test blessing", "Test blessing description", GetViewport().GetMousePosition());
    }

    /// <summary>
    /// Handles mouse exit event to hide hover state.
    /// </summary>
    private void OnMouseExited()
    {
        IsHovering = false;
        Managers.TooltipManager.Hide();
    }

    /// <summary>
    /// Sets the visual properties to indicate the blessing state.
    /// </summary>
    private void SetVisualMode(State state)
    {
        SelfModulate = Blessing.GetColor();
        if (state == State.MarkedForUse)
        {
            MouseDefaultCursorShape = CursorShape.PointingHand;
            TextLabel.Text = "Marked for use";
        }
        else if (state == State.Available)
        {
            MouseDefaultCursorShape = CursorShape.PointingHand;
            TextLabel.Text = "Available";
        }
        else if (state == State.Spent)
        {
            TextLabel.Text = "Spent";
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
        SetVisualMode(Blessing.State);
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
