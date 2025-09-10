using System.Collections.Generic;

/// <summary>
/// Represents an entry in the action queue.
/// </summary>
public class ActionQueueEntry
{
    /// <summary>
    /// The action to be performed.
    /// </summary>
    public Action Action { get; }

    /// <summary>
    /// The target of the action.
    /// </summary>
    public Character Target { get; }

    /// <summary>
    /// If the action represents a spell, the cards used in the spell cast.
    /// </summary>
    public List<Card> Cards { get; } = new List<Card>();

    public ActionQueueEntry(Action action, Character target, List<Card> cards = null)
    {
        Action = action;
        Target = target;
        
        if (cards != null)
        {
            Cards = cards;
        }
    }
}