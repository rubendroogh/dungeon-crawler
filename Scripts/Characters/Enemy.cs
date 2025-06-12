using Godot;
using System;
using System.Collections.Generic;

public partial class Enemy : Character
{
    /// <summary>
    /// The enemy's name.
    /// </summary>
    [Export]
    public string EnemyName { get; set; } = "Enemy";

    /// <summary>
    /// Signal that is emitted when the enemy performs an action.
    /// </summary>
    [Signal]
    public delegate void EnemyActionEventHandler();

    /// <summary>
    /// The queue of actions for the enemy to perform.
    /// This is used to manage actions that are queued for the enemy, allowing for multiple actions to be performed in a single turn.
    /// </summary>
    public List<Action> AvailableActions { get; } = new List<Action>();

    public override void _Ready()
    {
        base._Ready();
        InitializeAvailableActions();
    }

    /// <summary>
    /// Choose an action for the enemy to perform based on the current game state and available targets.
    /// </summary>
    public void ChooseAction(List<Character> targets)
    {
        if (targets == null || targets.Count == 0)
        {
            GD.PrintErr("Invalid parameters for getting action for enemy.");
            return;
        }

        // For simplicity, we will just choose the first available action and target.
        var selectedAction = AvailableActions.Count > 0 ? AvailableActions[0] : null;

        var target = ManagerRepository.BattleManager.GetPlayer();
        QueueAction(selectedAction, target);
    }

    /// <summary>
    /// Resolve the action queue for the enemy, executing all queued actions.
    /// </summary>
    public override void ResolveQueue()
    {
        if (ActionQueue.Count == 0)
        {
            GD.PrintErr("No actions in queue to resolve.");
            return;
        }

        var damagePackets = new List<DamagePacket>();
        foreach (var entry in ActionQueue)
        {
            if (entry.Action == null || entry.Target == null)
            {
                GD.PrintErr("Invalid action or target in action queue.");
                continue;
            }

            // Resolve the action and add the resulting damage packet to the list.
            var actionDamagePacket = entry.Action.GetBehaviour().Resolve(entry.Action.Data, [entry.Target]);
            damagePackets.Add(actionDamagePacket);
        }

        foreach (var damagePacket in damagePackets)
        {
            ManagerRepository.ActionManager.HandleResolveResult(damagePacket);
        }
    }

    /// <summary>
    /// Initializes the available actions for the enemy.
    /// </summary>
    private void InitializeAvailableActions()
    {
        AvailableActions.Add(new Action
        (
            new ActionData
            {
                Name = "Basic Attack",
                BasePhysicalDamage = 10,
            },
            new DefaultActionBehaviour()
        ));
    }

    /// <summary>
    /// Queue an action for the enemy to perform in the next damage phase.
    /// </summary>
    private void QueueAction(Action action, Character target)
    {
        if (action == null || target == null)
        {
            GD.PrintErr("Invalid parameters for adding action to queue.");
            return;
        }

        var entry = new ActionQueueEntry(action, target);
        ActionQueue.Add(entry);
        EmitSignal(SignalName.EnemyAction);
    }
}
