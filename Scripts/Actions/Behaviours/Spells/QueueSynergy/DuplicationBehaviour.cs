using System;
using System.Threading.Tasks;
using Godot;

public partial class DuplicationBehaviour : DefaultSpellBehaviour
{
    public override async Task PreCastQueue()
    {
        var indexInQueue = ActionManager.Instance.CastingContext.IndexInQueue;
        if (indexInQueue == -1)
        {
            // Queue position is unknown
            GD.PrintErr("DuplicationBehaviour: Index in queue is not set.");
            return;
        }

        if (indexInQueue == 0)
        {
            // Spell fizzles since it's the first in queue and has nothing to copy
            BattleLogManager.Instance.Log("Duplication has no spell to copy: it fizzles.");
            return;
        }

        try
        {
            // This whole thing is pretty ugly, might change one day
            var currentEntry = PlayerManager.Instance.GetPlayer().ActionQueue.ToArray()[indexInQueue];
            var entryToCopy = PlayerManager.Instance.GetPlayer().ActionQueue.ToArray()[indexInQueue - 1];

            currentEntry.Replace(entryToCopy);
            SpellQueueManager.Instance.UpdateSpellQueue();
        }
        catch(Exception ex)
        {
            GD.PrintErr(ex.Message);
            return;
        }
    }
}
