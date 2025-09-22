using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ResolveResult is a class that represents the result of an action resolving.
/// A ResolveResult only targets one character.
/// </summary>
public partial class ResolveResult
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
    /// Whether the action has been fully resolved and applied to the target.
    /// </summary>
    public bool HasResolved { get; set; }
    
    /// <summary>
    /// The total damage dealt by the action.
    /// </summary>
    public float TotalDamageAmount => Damages.Sum(d => d.Amount) - Heals.Sum(d => d.Amount);
}
