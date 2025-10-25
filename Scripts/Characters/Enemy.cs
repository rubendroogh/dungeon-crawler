using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

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
	/// The sprite that shows when the character gets damaged.
	/// </summary>
	private Sprite2D DamageAnimationSprite { get; set; }

	/// <summary>
	/// The queue of actions for the enemy to perform.
	/// This is used to manage actions that are queued for the enemy, allowing for multiple actions to be performed in a single turn.
	/// </summary>
	public List<Action> AvailableActions { get; } = new List<Action>();

	public override void _Ready()
	{
		base._Ready();
		InitializeAvailableActions();

		DamageAnimationSprite = GetNode<Sprite2D>("DamageSprite");
		DamageAnimationSprite.Visible = false;
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

		// For simplicity, we will just choose a random action from the available actions.
		var selectedAction = AvailableActions[GD.RandRange(0, AvailableActions.Count - 1)];

		var target = Managers.PlayerManager.GetPlayer();
		QueueAction(selectedAction, target);
	}

	/// <summary>
	/// Plays the damage animation for the enemy.
	/// </summary>
	public async override Task PlayDamageAnimation()
	{
		Managers.SoundEffectManager.PlaySoundEffect(CharacterData.HitSound);
		float flickerDuration = 0.6f;
		int flickerCount = 4;

		float interval = flickerDuration / (flickerCount * 2);

		for (int i = 0; i < flickerCount; i++)
		{
			CharacterSprite.Visible = false;
			await this.Delay((int)(interval * 1000));
			CharacterSprite.Visible = true;
			await this.Delay((int)(interval * 1000));
		}

		CharacterSprite.Visible = true; // Hide the sprite after flickering
		await this.Delay(1000);
	}

	/// <summary>
	/// Plays the death animation for the enemy.
	/// </summary>
	protected override async Task PlayDeathAnimation()
	{
		await this.Delay(1000);

		// Create a fade out animation for the enemy
		var tween = CreateTween();
		tween.TweenProperty(CharacterSprite, "modulate:a", 0f, 2f)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);

		await ToSignal(tween, "finished");
		await this.Delay(1000);
	}

	/// <summary>
	/// Initializes the available actions for the enemy.
	/// </summary>
	private void InitializeAvailableActions()
	{
		// TODO: Load actions dynamically
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
		ActionQueue.Enqueue(entry);
		EmitSignal(SignalName.EnemyAction);
	}
}
