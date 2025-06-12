using System.Collections.Generic;

public partial class SpellQueueEntry : ActionQueueEntry
{
    /// <summary>
    /// Represents the cards used in the spell cast.
    /// </summary>
    public List<Card> Cards { get; } = new List<Card>();

    /// <summary>
    /// Represents an entry in the spell queue.
    /// </summary>
    public SpellQueueEntry(Action spell, Character target, List<Card> cards) : base(spell, target)
    {
        Cards = cards;
    }
}
