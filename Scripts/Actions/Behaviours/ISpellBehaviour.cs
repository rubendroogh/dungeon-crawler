using System.Collections.Generic;

/// <summary>
/// ISpellBehaviour is an interface that defines the behaviour of a spell.
/// </summary>
public interface ISpellBehaviour
{
    /// <summary>
    /// Casts the spell using the selected cards and the spell data. Does not apply the damage to the target.
    /// </summary>
    /// <param name="cards"></param>
    /// <param name="spellData"></param>
    /// <param name="target"></param>
    /// <returns>The result of the spellcast, a list of damages, healing, and status effects.</returns>
    public DamagePacket Cast(List<Card> cards, ActionData spellData, List<Character> targets);
}
