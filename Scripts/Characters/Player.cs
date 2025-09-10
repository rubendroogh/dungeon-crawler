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
		IsPlayer = true;
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

		// Copy the card list to avoid screwing with the original list.
		var cardList = new List<Card>(cards);
		if (cardList.Count == 0)
		{
			// TODO: Add animation for hinting at card selection.
			return;
		}

		var entry = new SpellQueueEntry(spell, target, cardList);
		SpellQueue.Add(entry);
		EmitSignal(SignalName.SpellQueued);
		Managers.BattleLogManager.Log($"Queued {spell.Data.Name} with {cardList.Count} cards for {target.Name}.");

		Managers.DebugScreenManager.UpdateSpellQueue();
	}

	/// <summary>
	/// Resolve the spell queue, executing all queued spells, applying their effects, and showing animations.
	/// </summary>
	public override async Task ResolveQueue()
	{
		if (SpellQueue.Count == 0)
		{
			GD.PrintErr("No spells in queue to resolve.");
			return;
		}

		foreach (var entry in SpellQueue)
		{
			if (entry.Action == null || entry.Target == null)
			{
				GD.PrintErr("Invalid spell or target in spell queue.");
				continue;
			}

			// Resolve the spell and play the animation
			var spell = entry.Action as Spell;
			var spellBehaviour = spell.GetBehaviour();
			var spellResolveResult = spellBehaviour.Resolve(entry.Cards, spell.Data, entry.Target);

			await this.Delay(300);
			await spellBehaviour.AnimateSpellCast(spell.Data, entry.Target);
			await entry.Target.PlayDamageAnimation();
			int totalDamage = (int)await Managers.ActionManager.ApplyResolveResult(spellResolveResult);
			Managers.BattleLogManager.Log($"Resolved spell for {totalDamage} damage.");
			await this.Delay(300);
		}

		// Clear the spell queue after resolving.
		SpellQueue.Clear();
		Managers.DebugScreenManager.UpdateSpellQueue();
	}

	public override async Task PlayDamageAnimation()
	{
		await Task.Delay(300);

		var camera = Managers.PlayerManager.GetCamera() as CameraShake;
		if (camera != null)
		{
			await camera.StartShake(1.2f, 10f);
		}

		await Task.Delay(300);
	}

	protected override void InitializeNodes(CharacterData characterData)
	{
		HealthBar = GetTree().Root.GetNode<TextureProgressBar>("Root/UI/HUD/Debug/VBoxContainer/PlayerStats/HealthBar");

		if (HealthBar == null)
		{
			GD.PrintErr("Player HealthBar is not set up in the scene.");
			return;
		}

		HealthBar.MaxValue = characterData.MaxHealth;
		HealthBar.Value = CurrentHealth;
	}

	protected override void UpdateStatusEffectLabel()
	{
		// Keep this empty for now, as the player does not have a status effect label.
		// This method is overridden to prevent the base class from trying to update a non-existent label.
		// We can implement a status effect label for the player later in the UI.
	}
}
