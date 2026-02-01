using System.Collections.Generic;
using System.Threading.Tasks;

public partial class DuplicationBehaviour : DefaultSpellBehaviour
{
    public override async Task<ResolveResult> Resolve(List<Blessing> blessings, ActionData spellData, Character target)
    {
        // Get the last spell in the queue from the context
        var lastEntry = Managers.ActionManager.CastingContext.LastEntryResolved;
        var current = Managers.ActionManager.CastingContext.CurrentEntry;
        
        var queue = Managers.PlayerManager.GetPlayer().ActionQueue;
        Managers.BattleLogManager.Log($"Last entry: {lastEntry.Action.Data.Name} | This queue entry: {current.Action.Data.Name}");

        return await base.Resolve(blessings, spellData, target);
    }
}
