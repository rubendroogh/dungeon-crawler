using System.Collections.Generic;
using Godot;

public partial class Player : Character
{
    /// <summary>
    /// The player's name.
    /// </summary>
    [Export]
    public string PlayerName { get; set; } = "Player";

    /// <summary>
    /// Signal that is emitted when a spell is queued.
    /// </summary>
    [Signal]
    public delegate void SpellQueuedEventHandler();

    public override void _Ready()
    {
        base._Ready();
    }
    
    /// <summary>
    /// Queue an action for the character to perform in the next damage phase.
    /// </summary>
    /// <param name="action">The action to queue.</param>
    public override void QueueAction(Spell spell, Character target, List<Card> cards = null)
    {
        if (spell == null || cards == null || target == null)
        {
            GD.PrintErr("Invalid parameters for adding action to queue.");
            return;
        }

        var entry = new ActionQueueEntry(spell, [.. cards], target);
        ActionQueue.Add(entry);
        ManagerRepository.BattleLogManager.Log($"Queued {spell.Data.Name} with {cards.Count} cards for {target.Name}.");
        EmitSignal(SignalName.SpellQueued);
    }

    protected override void UpdateStatusEffectLabel()
    {
        // Keep this empty for now, as the player does not have a status effect label.
        // This method is overridden to prevent the base class from trying to update a non-existent label.
        // We can implement a status effect label for the player later in the UI.
    }
}
