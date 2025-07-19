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
    /// The queue of actions for the player to perform.
    /// </summary>
    public List<SpellQueueEntry> SpellQueue { get; } = new List<SpellQueueEntry>();

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
    public void QueueAction(Spell spell, Character target, List<Card> cards = null)
    {
        if (spell == null || cards == null || target == null)
        {
            GD.PrintErr("Invalid parameters for adding action to queue.");
            return;
        }

        var entry = new SpellQueueEntry(spell, target, [.. cards]);
        SpellQueue.Add(entry);
        Managers.BattleLogManager.Log($"Queued {spell.Data.Name} with {cards.Count} cards for {target.Name}.");
        EmitSignal(SignalName.SpellQueued);
    }

    /// <summary>
    /// Resolve the spell queue, executing all queued spells and applying their effects.
    /// </summary>
    public override void ResolveQueue()
    {
        if (SpellQueue.Count == 0)
        {
            GD.PrintErr("No spells in queue to resolve.");
            return;
        }

        var damagePackets = new List<DamagePacket>();
        foreach (var entry in SpellQueue)
        {
            if (entry.Action == null || entry.Target == null)
            {
                GD.PrintErr("Invalid spell or target in spell queue.");
                continue;
            }

            // Resolve the spell and add the resulting damage packet to the list.
            var spell = entry.Action as Spell;
            var actionDamagePacket = spell.GetBehaviour().Resolve(entry.Cards, spell.Data, [entry.Target]);
            damagePackets.Add(actionDamagePacket);
        }
        
        // Process each damage packet and log the results.
        foreach (var damagePacket in damagePackets)
        {
            var totalDamage = Managers.ActionManager.HandleResolveResult(damagePacket);
            Managers.BattleLogManager.Log($"Resolved spell for {totalDamage} damage.");
        }

        // Clear the spell queue after resolving.
        SpellQueue.Clear();
    }

    protected override void UpdateHealthBar()
    {
        // We use a label for debugging purposes to display the player's health.
        // In a real game, this would be replaced with a health bar UI element.
        // Find the health label node in the scene tree.
        var healthLabel = GetTree().Root.GetNode<Label>("Root/UI/HUD/Debug/VBoxContainer/PlayerStats/Health");
        healthLabel.Text = $"Health: {CurrentHealth}/{CharacterData.MaxHealth}";
    }

    protected override void UpdateStatusEffectLabel()
    {
        // Keep this empty for now, as the player does not have a status effect label.
        // This method is overridden to prevent the base class from trying to update a non-existent label.
        // We can implement a status effect label for the player later in the UI.
    }
}
