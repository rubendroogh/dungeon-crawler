using System.Collections.Generic;

/// <summary>
/// Represents an entry in the action queue.
/// </summary>
public class ActionQueueEntry
{
    /// <summary>
    /// The action to be performed.
    /// </summary>
    public Action Action { get; private set; }

    /// <summary>
    /// The target of the action.
    /// </summary>
    public Character Target { get; private set; }

    /// <summary>
    /// If the action represents a spell, the blessings used in the spell cast.
    /// </summary>
    public List<Blessing> Blessings { get; private set; } = new List<Blessing>();

    public ActionQueueEntry(Action action, Character target, List<Blessing> blessings = null)
    {
        Action = action;
        Target = target;
        
        if (blessings != null)
        {
            Blessings = blessings;
        }
    }

    /// <summary>
    /// Replaces the current entry in the queue with another.
    /// Can be used for substitution spells or other queue synergy.
    /// </summary>
    /// <remarks>
    /// You should update the spellQueue after calling this.
    /// </remarks>
    /// <param name="newEntry">The queue entry to replace this one with.</param>
    public void Replace(ActionQueueEntry newEntry)
    {
        Action = newEntry.Action;
        Target = newEntry.Target;
        Blessings = newEntry.Blessings;
    }
}