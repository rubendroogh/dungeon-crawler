/// <summary>
/// Damage is a class that represents the damage dealt by a spell to a target.
/// </summary>
public partial class Damage
{
    /// <summary>
    /// The amount of damage dealt by the spell.
    /// </summary>
    public float Amount { get; set; }

    /// <summary>
    /// The target of the damage. This is the character that will receive the damage.
    /// </summary>
    public Character Target { get; set; }

    /// <summary>
    /// The type of damage dealt by the spell. This is used to determine how the damage is calculated.
    /// </summary>
    public DamageType Type { get; set; }

    /// <summary>
    /// The source of the damage.
    /// </summary>
    public DamageSource Source { get; set; }

    /// <summary>
    /// The status effect applied by the damage.
    /// </summary>
    public StatusEffect StatusEffect { get; set; }
    
    public Damage(float damageAmount, DamageType damageType, Character target)
    {
        Amount = damageAmount;
        Type = damageType;
        Target = target;
        Source = DamageSource.Unknown;
    }

    public Damage(float damageAmount, DamageType damageType, Character target, DamageSource damageSource)
    {
        Amount = damageAmount;
        Type = damageType;
        Target = target;
        Source = damageSource;
    }

    /// <summary>
    /// Applies the damage to the target.
    /// </summary>
    public void Apply()
    {
        if (StatusEffect != StatusEffect.None)
        {
            // TODO: This is 2 turns for now, but it should be the duration of the effect
            Target.ApplyEffect(StatusEffect, 2);
        }

        Target.Damage(this);
    }
}

public enum DamageType
{
    Physical,
    Dark,
    Light,
    Fire,
    Ice,
    Lightning,
    Sanity,
    Disease,
}

// TODO: Does nothing for now
public enum DamageSource
{
    Unknown,
    Spell,
    Item,
    Environment,
    Character,
}

public enum StatusEffect
{
    None,
    Burn,
    Frozen,
    Insanity,
    BrainFreeze,
}