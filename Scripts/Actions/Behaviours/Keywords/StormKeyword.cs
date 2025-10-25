using System.Threading.Tasks;
using Godot;

public class StormKeyword : KeywordBase
{
    public override async Task OnCast()
    {
        var context = Managers.ActionManager.KeywordContext;
        if (context.StormProcessed)
        {
            // Already processed this keyword effect for this cast.
            return;
        }
    
        // First get the spell queue for the current caster.
        var caster = context.Caster;
        if (caster == null || context.Action is not Spell spell)
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
        int spellsCastThisTurn = context.CastSpellsThisTurn;
        if (spellsCastThisTurn == 0)
        {
            // No additional casts needed.
            return;
        }

        // Duplicate the spell for each additional spell cast this turn (minus the original).
        for (int i = 0; i < spellsCastThisTurn; i++)
        {
            // Queue the spell again with the same target and one card.
            var syntheticCard = new Blessing
            {
                Rank = Rank.Two,
                Domain = Domain.Zer
            };

            spellQueue.Enqueue(new ActionQueueEntry(spell, context.Target, [syntheticCard]));
            Managers.BattleLogManager.Log($"Queued duplicate of {spell.Data.Name} due to Storm effect.");
        }

        // Update the debug screen to show the new spell queue.
        Managers.DebugScreenManager.UpdateSpellQueue();

        // Mark as processed to avoid re-processing in the same turn queue.
        context.StormProcessed = true;
    }
}
