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
    /// The overlay control used for highlighting on hover.
    /// </summary>
    private Control HighlightOverlay => GetNode<Control>("HighlightOverlay");

    /// <summary>
    /// Sets up the BlessingUI with the given blessing.
    /// </summary>
    public BlessingUI Setup(Blessing blessing)
    {
        Blessing = blessing;

        UpdateBlessingBarSize();
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
        TooltipManager.Instance.UpdatePosition(GetGlobalMousePosition());

        if (!ManaSourceManager.Instance.ManaSelectionMode)
        {
            return;
        }

        // On click
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            if (Blessing.State == State.MarkedForUse)
            {
                // If already marked for use, deselect
                ManaSourceManager.Instance.BlessingBar.SetBlessingState(Blessing.ID, State.Available);
                OnManaStateChanged();
            }
            else if (Blessing.State == State.Available)
            {
                ManaSourceManager.Instance.BlessingBar.SetBlessingState(Blessing.ID, State.MarkedForUse);
                OnManaStateChanged();
            }
        }
    }

    /// <summary>
    /// Sets the highlight overlay visibility.
    /// </summary>
    public void SetHighlight(bool highlight)
    {
        HighlightOverlay.Visible = highlight;
    }

    /// <summary>
    /// Handles mouse enter event to show hover state.
    /// </summary>
    private void OnMouseEntered()
    {
        if (Blessing.State != State.Spent)
        {
            if (ManaSourceManager.Instance.ManaSelectionMode)
            {
                SetHighlight(true);
            }
        }

        var mousePosition = GetGlobalMousePosition();
        TooltipManager.Instance.Show(Blessing.ToString(), Blessing.GetDescription(), mousePosition);
    }

    /// <summary>
    /// Handles mouse exit event to hide hover state.
    /// </summary>
    private void OnMouseExited()
    {
        SetHighlight(false);
        TooltipManager.Instance.Hide();
    }

    /// <summary>
    /// Sets the visual properties to indicate the blessing state.
    /// </summary>
    private void SetVisualMode(State state)
    {
        SelfModulate = Blessing.GetDomainColor();
        if (ManaSourceManager.Instance.ManaSelectionMode)
        {
            MouseDefaultCursorShape = CursorShape.PointingHand;
        }
        else
        {
            MouseDefaultCursorShape = CursorShape.Arrow;
        }

        switch (state)
        {
            case State.MarkedForUse:
                TextLabel.Text = "Marked for use";
                break;
            case State.Available:
                TextLabel.Text = "Available";
                break;
            case State.Spent:
                TextLabel.Text = "Spent";
                break;
        }
    }

    /// <summary>
    /// Initialize the OnManaStateChanged signals.
    /// </summary>
    private void InitializeCustomSignals()
    {
        ManaSourceManager.Instance.BlessingStateChanged += OnManaStateChanged;
        ManaSourceManager.Instance.ManaSelectionModeChanged += _ => OnManaStateChanged();
    }

    /// <summary>
    /// Handles visual feedback of mana state changed.
    /// </summary>
    private void OnManaStateChanged()
    {
        SetVisualMode(Blessing.State);
        UpdateBlessingBarSize();
    }

    /// <summary>
    /// Sets the label text to display the blessing's domain and level.
    /// </summary>
    private void SetLabelText()
    {
        TextLabel.Text = $"{Blessing.Domain} ({Blessing.Level})";
    }

    /// <summary>
    /// Updates the size of the blessing UI based on its level
    /// and the blessing bar's max mana.
    /// </summary>
    private void UpdateBlessingBarSize()
    {
        var fullWidth = ManaSourceManager.Instance.Width;
        var percentage = (float)Blessing.Level / ManaSourceManager.Instance.BlessingBar.MaxMana;

        var xSize = (int)((fullWidth - 12) * percentage);
        var ySize = 48 - 12;

        SelfModulate = Blessing.GetDomainColor();
        Texture = new AtlasTexture
        {
            Atlas = Blessing.GetTexture(),
            Region = new Rect2(Vector2.Zero, new Vector2(xSize, ySize))
        };

        CustomMinimumSize = new Vector2((int)Blessing.Level, ySize);
    }
}
