using System;
using System.Threading.Tasks;
using Godot;

public partial class DuplicationBehaviour : DefaultSpellBehaviour
{
    public override async Task PreCastQueue()
    {
        var indexInQueue = Managers.ActionManager.CastingContext.IndexInQueue;
        if (indexInQueue == -1)
        {
            // Queue position is unknown
            GD.PrintErr("DuplicationBehaviour: Index in queue is not set.");
            return;
        }

        if (indexInQueue == 0)
        {
            // Spell fizzles since it's the first in queue and has nothing to copy
            return;
        }

        try
        {
            // This whole thing is pretty ugly, might change one day
            var currentEntry = Managers.PlayerManager.GetPlayer().ActionQueue.ToArray()[indexInQueue];
            var entryToCopy = Managers.PlayerManager.GetPlayer().ActionQueue.ToArray()[indexInQueue - 1];

            currentEntry.Replace(entryToCopy);
            Managers.SpellQueueManager.UpdateSpellQueue();
        }
        catch(Exception ex)
        {
            GD.PrintErr(ex.Message);
            return;
        }
    }
}
