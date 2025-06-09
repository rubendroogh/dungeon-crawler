using System.Collections.Generic;
using System.Linq;

/// <summary>
/// SpellCastResult is a class that represents the result of a spell cast.
/// </summary>
public partial class DamagePacket
{
    /// <summary>
    /// The list of damages dealt by the spell.
    /// </summary>
    public List<Damage> Damages = new();

    /// <summary>
    /// The list of heals dealt by the spell.
    /// </summary>
    public List<Damage> Heals = new();

    /// <summary>
    /// The total damage dealt by the spell.
    /// </summary>
    public float TotalDamage => Damages.Sum(d => d.Amount) - Heals.Sum(d => d.Amount);
}
