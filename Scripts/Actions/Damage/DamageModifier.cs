/// <summary>
/// A modifier that can be applied to the damage dealt to a character.
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
/// </summary>
public enum DamageModifierType
{
    /// <summary>
    /// Adds a fixed amount to the damage.
    /// </summary>
    Additive,
    /// <summary>
    /// Multiplies the damage by a factor.
    /// </summary>
    Multiplicative,
    /// <summary>
    /// Applies a percentage increase or decrease to the damage.
    /// </summary>
    Percentage,
    /// <summary>
    /// Overrides the damage to a fixed value.
    /// </summary>
    Override,
}