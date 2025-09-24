using Godot;

public partial class StatusEffectIcon : Control
{
    [Export]
    private Texture2D BrainFreezeIcon { get; set; }

    [Export]
    private Texture2D BurnIcon { get; set; }

    [Export]
    private Texture2D FrozenIcon { get; set; }

    [Export]
    private Texture2D InsanityIcon { get; set; }

    private TextureRect Icon { get; set; }

    public override void _Ready()
    {
        Icon = GetNode<TextureRect>("Panel/Icon");
    }

    /// <summary>
    /// Sets the correct icon based on the provided status effect type.
    /// </summary>
    public void Setup(StatusEffectType statusEffectType)
    {
        if (Icon == null)
        {
            Icon = GetNode<TextureRect>("Panel/Icon");
        }

        switch (statusEffectType)
        {
            case StatusEffectType.None:
                Icon.Texture = null;
                break;
            case StatusEffectType.BrainFreeze:
                Icon.Texture = BrainFreezeIcon;
                break;
            case StatusEffectType.Burn:
                Icon.Texture = BurnIcon;
                break;
            case StatusEffectType.Frozen:
                Icon.Texture = FrozenIcon;
                break;
            case StatusEffectType.Insanity:
                Icon.Texture = InsanityIcon;
                break;
            default:
                Icon.Texture = null;
                break;
        }
    }
}
