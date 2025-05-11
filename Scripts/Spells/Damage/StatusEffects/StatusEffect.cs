using Godot;
using System.Collections.Generic;

/// <summary>
/// The stateful effect that can be applied to a character.
/// </summary>
public partial class StatusEffect
{
    /// <summary>
    /// The base duration of the status effect.
    /// </summary>
    public int BaseDuration { get; set; } = 1;

    /// <summary>
    /// The amount of turns the status effect will last.
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// The type of the status effect.
    /// </summary>
    public StatusEffectType Type { get; set; }

    /// <summary>
    /// The current turn of the status effect.
    /// </summary>
    public IStatusEffectBehaviour Behaviour { get {
        if (StatusEffectBehaviours.TryGetValue(Type, out var behaviour))
        {
            return behaviour;
        }

        GD.PrintErr($"No behaviour found for status effect type: {Type}");
        return null;
    }}

    public StatusEffect(int duration, StatusEffectType type)
    {
        Duration = duration;
        Type = type;
    }

    /// <summary>
    /// The behaviours associated with the status effect.
    /// </summary>
    /// TODO: Move this to a separate class
    private Dictionary<StatusEffectType, IStatusEffectBehaviour> StatusEffectBehaviours = new()
    {
        { StatusEffectType.Frozen, new FrozenStatusEffectBehaviour() },
        // { StatusEffectType.BrainFreeze, new BrainFreezeStatusEffectBehaviour() },
    };
}


public enum StatusEffectType
{
    None,
    Burn,
    Frozen,
    Insanity,
    BrainFreeze,
}
