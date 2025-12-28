using System.Collections.Generic;
using System.Threading.Tasks;
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

	/// <summary>
	/// The HealthBar UI element for the player.
	/// </summary>
	private PlayerHealthBar PlayerHealthBar { get; set; }

	public override void _Ready()
	{
		base._Ready();
		IsPlayer = true;
	}

	/// <summary>
	/// Queue an action for the character to perform in the next damage phase.
	/// Clear the selected spell from the action manager after queuing.
	/// </summary>
	/// <param name="action">The action to queue.</param>
	public void QueueAction(Spell spell, Character target, List<Blessing> blessings = null)
	{
		if (spell == null || target == null)
		{
			GD.PrintErr("Invalid parameters for adding action to queue.");
			return;
		}

		// Copy the blessing list to avoid screwing with the original list.
		var blessingList = new List<Blessing>(blessings ?? []);
		var entry = new ActionQueueEntry(spell, target, blessingList);
		
		ActionQueue.Enqueue(entry);
		EmitSignal(SignalName.SpellQueued);
		Managers.BattleLogManager.Log($"Queued {spell.Data.Name} with {blessingList.Count} blessings for {target.Name}.");

		Managers.DebugScreenManager.UpdateSpellQueue();
	}

	public override async Task PlayDamageAnimation()
	{
		await Task.Delay(300);
        _ = Managers.SoundEffectManager.PlaySoundEffect(CharacterData.HitSound);

        if (Managers.PlayerManager.GetCamera() is CameraShake camera)
        {
            await camera.StartShake(0.7f, 10f);
        }

        await Task.Delay(300);
	}

	protected override void InitializeNodes(CharacterData characterData)
	{
		// Find health bar in the scene tree by name
		PlayerHealthBar = GetTree().Root.GetNode<PlayerHealthBar>("Root/UI/HUD/Bottom/Margin/VBox/PlayerHealthBar");

		if (PlayerHealthBar == null)
		{
			GD.PrintErr("Player.cs: Player HealthBar is not set up in the scene.");
			return;
		}

		PlayerHealthBar.Initialize(characterData.MaxHealth, Health);
	}

	protected override async Task UpdateHealthBar()
	{
		await PlayerHealthBar.SetHealth(Health);
	}

	protected override void UpdateStatusEffectLabel()
	{
		// We should implement a status effect label for the player later in the UI.
		// This method is overridden to prevent the base class from trying to update a non-existent label.
	}
}
