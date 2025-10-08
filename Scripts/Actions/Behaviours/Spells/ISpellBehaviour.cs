using System.Collections.Generic;

/// <summary>
/// ISpellBehaviour is an interface that defines the behaviour of a spell.
/// </summary>
public interface ISpellBehaviour : IActionBehaviour
{
    /// <summary>
    /// Casts the spell using the selected cards and the spell data. Does not apply the damage to the target.
    /// </summary>
    /// <returns>The result of the spellcast, a list of damages, healing, and status effects.</returns>
    public ResolveResult Resolve(List<Blessing> cards, ActionData spellData, Character target);

    /// <summary>
    /// Checks if the spell can be cast by the given character with the provided cards and abilities.
    /// </summary>
    /// <returns>Whether the spell can be cast.</returns>
    public bool CanCast(Character caster, Spell spell, List<Blessing> cards);
}
