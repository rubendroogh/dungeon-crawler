
/// <summary>
/// A modifier that can be applied to the damage dealt by a character.
/// </summary>
public class DamageModifier
{
    public DamageModifierType Type { get; set; }

    public float Value { get; set; }

    public DamageModifier(DamageModifierType type, float value)
    {
        Type = type;
        Value = value;
    }
}

/// <summary>
/// The type of damage modifier.
/// This enum defines the different types of damage modifiers that can be applied to a character's damage.
/// </summary>
public enum DamageModifierType
{
    /// <summary>
    /// An additive modifier that adds a fixed amount to the damage.
    /// </summary>
    Additive,
    /// <summary>
    /// A multiplicative modifier that multiplies the damage by a factor.
    /// </summary>
    Multiplicative,
    /// <summary>
    /// A percentage modifier that applies a percentage increase or decrease to the damage.
    /// </summary>
    Percentage,
}