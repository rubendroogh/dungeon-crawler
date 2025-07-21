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
    public StatusEffectType StatusEffect { get; set; }
    
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
    public int Apply()
    {
        if (StatusEffect != StatusEffectType.None)
        {
            // TODO: This is 2 turns for now, but it should be the duration of the effect
            Target.ApplyEffect(StatusEffect, 2);
        }

        return Target.Damage(this);
    }
}

/// <summary>
/// Defines the different types of damage that can be dealt by a spell.
/// </summary>
public enum DamageType
{
    /// <summary>
    /// Represents physical damage, which is the most common type of damage.
    /// It is usually dealt by melee attacks or physical spells.
    /// </summary>
    Physical,

    /// <summary>
    /// Represents dark damage, which is often associated with curses or shadow magic.
    /// </summary>
    Dark,
    /// <summary>
    /// Represents light damage, which is often associated with holy or radiant magic.
    /// </summary>
    Light,
    /// <summary>
    /// Represents fire damage, which is often associated with explosions or burning effects.
    /// </summary>
    Fire,
    /// <summary>
    /// Represents ice damage, which is often associated with freezing or chilling effects.
    /// </summary>
    Ice,
    /// <summary>
    /// Represents lightning damage, which is rare and usually environmental.
    /// </summary>
    Lightning,
    /// <summary>
    /// Represents sanity damage, which is often associated with mental effects.
    /// </summary>
    Sanity,
    /// <summary>
    /// Represents disease damage, which is often associated with poison or infection.
    /// </summary>
    Disease,
}


/// <summary>
/// Defines the source of the damage.
/// This is used to determine how the damage is applied and what effects it may have.
/// </summary>
public enum DamageSource
{
    /// <summary>
    /// Represents an unknown source of damage.
    /// </summary>
    Unknown,
    /// <summary>
    /// Represents damage dealt by a spell.
    /// </summary>
    Spell,
    /// <summary>
    /// Represents damage dealt by an item, like a bomb or potion.
    /// </summary>
    Item,
    /// <summary>
    /// Represents damage dealt by the environment, like traps and hazards.
    /// </summary>
    Environment,
    /// <summary>
    /// Represents damage dealt by a character, like an attack or ability.
    /// </summary>
    Character,
}