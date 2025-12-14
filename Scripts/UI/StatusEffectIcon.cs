using Godot;

public partial class StatusEffectIcon : Control
{
    [Export]
    private Texture2D SolidifiedIcon { get; set; }

    [Export]
    private Texture2D FrozenIcon { get; set; }

    [Export]
    private Texture2D PhysicalInvincibilityIcon { get; set; }

    private TextureRect Icon { get; set; }

    private StatusEffectType StatusEffectType { get; set; }

    public override void _Ready()
    {
        Icon = GetNode<TextureRect>("Panel/Icon");
        MouseExited += OnMouseExited;
    }

    /// <summary>
    /// Sets the correct icon based on the provided status effect type.
    /// </summary>
    public void Setup(StatusEffectType statusEffectType)
    {
        Icon ??= GetNode<TextureRect>("Panel/Icon");

        StatusEffectType = statusEffectType;

        switch (statusEffectType)
        {
            case StatusEffectType.None:
                Icon.Texture = null;
                break;
            case StatusEffectType.Solidified:
                Icon.Texture = SolidifiedIcon;
                break;
            case StatusEffectType.Frozen:
                Icon.Texture = FrozenIcon;
                break;
            case StatusEffectType.PhysicalInvincibility:
                Icon.Texture = PhysicalInvincibilityIcon;
                break;
            default:
                Icon.Texture = null;
                break;
        }
    }

    /// <summary>
    /// Handles mouse enter events to show the tooltip.
    /// </summary>
    public override void _GuiInput(InputEvent @event)
    {
        if (Managers.TooltipManager.IsTooltipVisible)
        {
            Managers.TooltipManager.UpdatePosition(GetGlobalMousePosition());
        }
        else
        {
            Managers.TooltipManager.Show(
                StatusEffectType.ToString(),
                GetStatusEffectDescription(),
                GetGlobalMousePosition()
            );
        }
    }

    /// <summary>
    /// Handles mouse exit events to hide the tooltip.
    /// </summary>
    private void OnMouseExited()
    {
        Managers.TooltipManager.Hide();
    }

    /// <summary>
    /// Gets the description for the given status effect type.
    /// TODO: Move this to a more appropriate location.
    /// </summary>
    private string GetStatusEffectDescription()
    {
        return StatusEffectType switch
        {
            StatusEffectType.Solidified => "Multiplies physical damage taken by 4x.",
            StatusEffectType.Frozen => "Multiplies physical damage taken by 2x.",
            StatusEffectType.PhysicalInvincibility => "Prevents all physical damage.",
            _ => "No status effect."
        };
    }
}
