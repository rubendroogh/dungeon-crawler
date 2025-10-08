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
    public List<Blessing> Cards { get; } = new List<Blessing>();

    public ActionQueueEntry(Action action, Character target, List<Blessing> cards = null)
    {
        Action = action;
        Target = target;
        
        if (cards != null)
        {
            Cards = cards;
        }
    }
}