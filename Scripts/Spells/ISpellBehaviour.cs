using System.Collections.Generic;

/// <summary>
/// ISpellBehaviour is an interface that defines the behaviour of a spell.
/// </summary>
public interface ISpellBehaviour
{
    /// <summary>
    /// Calculates the damage the spell deals using the used cards.
    /// </summary>
    /// <param name="cards"></param>
    /// <param name="spellData"></param>
    /// <returns>The damage the spell deals.</returns>
    /// TODO: Possibly also return the healing as a touple?
    public float CalculateTotalDamage(List<Card> cards, SpellData spellData);
}
