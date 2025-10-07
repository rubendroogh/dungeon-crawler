using System.Collections.Generic;
using Godot;


/// <summary>
/// Spell represents a magical action that can be cast by a player.
/// </summary>
public partial class Spell : Action
{
    public Spell(ActionData spellData, IActionBehaviour spellBehaviour) : base(spellData, spellBehaviour)
    {
        Data = spellData;
        Behaviour = spellBehaviour;
    }

    /// <summary>
    /// Gets the behaviour of the spell, which is expected to implement ISpellBehaviour.
    /// </summary>
    public override ISpellBehaviour GetBehaviour()
    {
        return Behaviour as ISpellBehaviour;
    }

    /// <summary>
    /// Check if the spell can be cast by the given character with the provided cards and abilities.
    /// </summary>
    public bool CanCast(Character caster, List<Card> cards)
    {
        var spellBehaviour = GetBehaviour();
        if (spellBehaviour == null)
        {
            GD.PrintErr("Spell behaviour is null");
            return false;
        }

        return spellBehaviour.CanCast(caster, this, cards);
    }
}
