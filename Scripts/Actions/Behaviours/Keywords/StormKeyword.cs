using System.Threading.Tasks;
using Godot;

public class StormKeyword : KeywordBase
{
    private bool HasProcessed = false;

    public override async Task OnCast()
    {
        GD.Print("StormKeyword: OnCast triggered.");
        if (HasProcessed)
        {
            // Already processed this keyword effect for this cast.
            return;
        }

        // First get the spell queue for the current caster.
        var caster = Managers.ActionManager.KeywordContext.Caster;
        var spell = Managers.ActionManager.KeywordContext.Action as Spell;
        if (caster == null || spell == null)
        {
            GD.PrintErr("StormKeyword: Invalid caster or spell in keyword context.");
            return;
        }

        // Get the spell queue for the current caster.
        var spellQueue = caster.ActionQueue;
        if (spellQueue == null)
        {
            GD.PrintErr("StormKeyword: Unable to retrieve spell queue.");
            return;
        }

        // Count how many spells have been cast this turn by this caster (not including this one).
        int spellsCastThisTurn = Managers.ActionManager.KeywordContext.CastSpellsThisTurn;
        GD.Print($"StormKeyword: Spells cast this turn: {spellsCastThisTurn}");
        if (spellsCastThisTurn == 0)
        {
            // No additional casts needed.
            return;
        }

        // Duplicate the spell for each additional spell cast this turn (minus the original).
        for (int i = 0; i < spellsCastThisTurn; i++)
        {
            // Queue the spell again with the same target and no additional cards.
            spellQueue.Add(new ActionQueueEntry(spell, target: Managers.ActionManager.KeywordContext.Target));
            Managers.BattleLogManager.Log($"StormKeyword: Queued duplicate of {spell.Data.Name} due to Storm effect.");
        }

        // Update the debug screen to show the new spell queue.
        Managers.DebugScreenManager.UpdateSpellQueue();

        // Mark as processed to avoid re-processing in the same turn queue.
        HasProcessed = true;
    }
}
