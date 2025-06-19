using Godot;

public partial class CardUI : TextureRect
{
    [Export]
    public bool IsSelected { get; set; } = false;

    [Export]
    public bool IsUsed { get; set; } = false;

    public Card Card { get; init; }

    private float HoverRotation = Mathf.DegToRad(35);

    private float AnimationDuration = 0.2f;

    public override void _Ready()
    {
        // Center the anchor point (0.5 is center)
        AnchorLeft = 0.5f;
        AnchorTop = 0.5f;
        AnchorRight = 0.5f;
        AnchorBottom = 0.5f;
        CallDeferred(nameof(SetPivot));

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;

        SetUsable(true);

        CallDeferred(nameof(InitializeCustomSignals));
    }

    /// <summary>
    /// Initializes custom signals for the CardUI.
    /// </summary>
    private void InitializeCustomSignals()
    {
        Managers.ActionManager.CardsReset += OnCardsReset;
        Managers.BattleManager.GetPlayer().SpellQueued += OnSpellQueued;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                // Handle click event
                if (IsSelected)
                {
                    var isCardRemoved = Managers.ActionManager.RemoveCardFromSelection(Card);
                    if (isCardRemoved)
                    {
                        IsSelected = false;
                        AnimateRotation(0);
                        return;
                    }
                }
                else
                {
                    var isCardAdded = Managers.ActionManager.AddCardToSelection(Card);
                    if (isCardAdded)
                    {
                        IsSelected = true;
                        AnimateRotation(HoverRotation);
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Called when any spell is queued.
    /// This is used to disable the card UI and grey it out after a spell using it is queued.
    /// </summary>
    private void OnSpellQueued()
    {
        if (!IsSelected)
        {
            return;
        }

        SetUsable(false);

        // Remove the card from the selection
        Managers.ActionManager.RemoveCardFromSelection(Card);
    }

    /// <summary>
    /// Called when the ResetCards method is called on the ActionManager.
    /// Used to handle the visual and functional changes necessary for each card.
    /// </summary>
    public void OnCardsReset()
    {
        IsSelected = false;
        SetUsable(true);
    }

    /// <summary>
    /// Changes the visual style and clickability depending on if it should be usable.
    /// </summary>
    /// <param name="shouldBeUsable"></param>
    private void SetUsable(bool shouldBeUsable)
    {
        if (shouldBeUsable)
        {
            IsUsed = false;
            // GD.Print("Should be usable");

            // Enable mouse input
            MouseFilter = MouseFilterEnum.Stop;
            MouseDefaultCursorShape = CursorShape.PointingHand;

            Modulate = Colors.White;
        }
        else
        {
            IsUsed = true;
            IsSelected = false;
            AnimateRotation(0);
            // GD.Print("Should not be usable");

            // Grey out the card
            Modulate = new Color(0.5f, 0.5f, 0.5f, 1);

            // Disable mouse input
            MouseFilter = MouseFilterEnum.Ignore;
        }
    }

    private void SetPivot()
    {
        PivotOffset = Size / 2;
    }
    
    private void OnMouseEntered()
    {
        if (!IsSelected)
        {
            AnimateRotation(HoverRotation);
        }
    }

    private void OnMouseExited()
    {
        if (!IsSelected)
        {
            AnimateRotation(0);
        }
    }

    private void AnimateRotation(float targetAngle)
    {
        var tween = CreateTween();
        // Animate the rotation smoothly over 0.2 seconds
        tween.TweenProperty(this, "rotation", targetAngle, AnimationDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }
}