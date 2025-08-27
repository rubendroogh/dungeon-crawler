using System.Collections.Generic;
using System.Linq;

/// <summary>
/// DamagePacket is a class that represents the result of an action resolving.
/// A DamagePacket only targets one character.
/// </summary>
public partial class DamagePacket
{
    /// <summary>
    /// The list of damages dealt by the action.
    /// </summary>
    public List<Damage> Damages = new();

    /// <summary>
    /// The list of heals dealt by the action.
    /// </summary>
    public List<Damage> Heals = new();

    /// <summary>
    /// The target character of the damage packet.
    /// </summary>
    public Character Target { get; set; }

    /// <summary>
    /// The total damage dealt by the action.
    /// </summary>
    public float TotalBaseAmount => Damages.Sum(d => d.Amount) - Heals.Sum(d => d.Amount);

    /// <summary>
    /// The total modified damage dealt by the action based on the weaknesses and resistances of the target.
    /// </summary>
    public float TotalModifiedAmount => Damages.Sum(d => Target.GetModifiedDamage(d)) - Heals.Sum(d => Target.GetModifiedDamage(d));
}
