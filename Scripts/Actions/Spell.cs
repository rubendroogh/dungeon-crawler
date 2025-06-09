/// <summary>
/// A spell is the combination of a spell data and the behavior that it has.
/// </summary>
public partial class Spell
{
    public ActionData Data { get; set; }

    public ISpellBehaviour Behaviour { get; set; }

    public Spell(ActionData spellData, ISpellBehaviour spellBehaviour)
    {
        Data = spellData;
        Behaviour = spellBehaviour;
    }
}
