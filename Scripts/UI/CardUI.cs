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
        // Enable mouse input
        MouseFilter = MouseFilterEnum.Stop;
        MouseDefaultCursorShape = CursorShape.PointingHand;

        // Center the anchor point (0.5 is center)
        AnchorLeft = 0.5f;
        AnchorTop = 0.5f;
        AnchorRight = 0.5f;
        AnchorBottom = 0.5f;
        CallDeferred(nameof(SetPivot));

        // Connect built-in signals
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;

        CallDeferred(nameof(InitializeCustomSignals));
    }

    private void InitializeCustomSignals()
    {
        var spellCastingManager = ManagerRepository.SpellCastingManager;
        spellCastingManager.SpellCast += OnSpellCast;
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
                    var isCardRemoved = ManagerRepository.SpellCastingManager.RemoveCardFromSelection(Card);
                    if (isCardRemoved)
                    {
                        IsSelected = false;
                        AnimateRotation(0);
                        return;
                    }
                }
                else
                {
                    var isCardAdded = ManagerRepository.SpellCastingManager.AddCardToSelection(Card);
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

    private void OnSpellCast()
    {
        // Handle the spell cast event
        if (!IsSelected)
        {
            return;
        }

        IsUsed = true;
        AnimateRotation(0);
        IsSelected = false;

        // Grey out the card
        Modulate = new Color(0.5f, 0.5f, 0.5f, 1);
        // Disable mouse input
        MouseFilter = MouseFilterEnum.Ignore;
        // Disconnect signals
        MouseEntered -= OnMouseEntered;
        MouseExited -= OnMouseExited;
        // Remove the card from the selection
        ManagerRepository.SpellCastingManager.RemoveCardFromSelection(Card);
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