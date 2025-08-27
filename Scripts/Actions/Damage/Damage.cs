using System.Threading.Tasks;


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
    
    public Damage(float damageAmount, DamageType damageType)
    {
        Amount = damageAmount;
        Type = damageType;
        Source = DamageSource.Unknown;
    }

    public Damage(float damageAmount, DamageType damageType, DamageSource damageSource)
    {
        Amount = damageAmount;
        Type = damageType;
        Source = damageSource;
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