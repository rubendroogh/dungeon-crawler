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
}
