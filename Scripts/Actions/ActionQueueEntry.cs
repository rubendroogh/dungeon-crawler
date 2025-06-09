using System.Collections.Generic;


/// <summary>
/// Represents an entry in the spell queue.
/// </summary>
public class ActionQueueEntry
{
    public Spell Spell { get; }
    public List<Card> Cards { get; }
    public Character Target { get; }

    public ActionQueueEntry(Spell spell, List<Card> cards, Character target)
    {
        Spell = spell;
        Cards = cards;
        Target = target;
    }
}