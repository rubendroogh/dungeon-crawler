using System.Collections.Generic;


/// <summary>
/// Represents an entry in the spell queue.
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

    public ActionQueueEntry(Action action, Character target)
    {
        Action = action;
        Target = target;
    }
}