using System.Collections.Generic;
using System.Linq;

/// <summary>
/// DamagePacket is a class that represents the result of an action resolving.
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
    /// The total damage dealt by the action.
    /// </summary>
    public float TotalDamage => Damages.Sum(d => d.Amount) - Heals.Sum(d => d.Amount);
}
