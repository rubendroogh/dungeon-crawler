using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Executes resource-defined enemy attacks and presents damage and attack intentions.
/// </summary>
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
	private int AttacksTaken { get; set; }

	/// <summary>
	/// The attack shown to the player, chosen deterministically from the resource-defined cycle.
	/// </summary>
	public ActionData NextAction => AvailableActions.Count == 0
		? null
		: AvailableActions[AttacksTaken % AvailableActions.Count].Data;

	[Export] private Label Intent;
	[Export] private string IntentFormat;
	[Export] private string WeaknessFormat;
	[Export] private string ResistanceFormat;

	public override void _Ready()
	{
		base._Ready();
		InitializeAvailableActions();

		DamageAnimationSprite = GetNode<Sprite2D>("DamageSprite");
		DamageAnimationSprite.Visible = false;
		UpdateIntent();
	}

	/// <summary>
	/// Choose an action for the enemy to perform based on the current game state and available targets.
	/// </summary>
	public void ChooseAction(List<Character> targets)
	{
		if (targets == null || targets.Count == 0 || AvailableActions.Count == 0)
		{
			GD.PrintErr("Invalid parameters for getting action for enemy.");
			return;
		}

		// Cycle through the configured attacks so the displayed intention is predictable.
		var selectedAction = AvailableActions[AttacksTaken % AvailableActions.Count];
		AttacksTaken++;

		var target = PlayerManager.Instance.GetPlayer();
		QueueAction(selectedAction, target);
		UpdateIntent();
	}

	/// <summary>
	/// Plays the damage animation for the enemy.
	/// </summary>
	public async override Task PlayDamageAnimation()
	{
		_ = SoundEffectManager.Instance.PlaySoundEffect(CharacterData.HitSound);
		float flickerDuration = 0.24f;
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
		await this.Delay(80);
	}

	/// <summary>
	/// Plays the death animation for the enemy.
	/// </summary>
	protected override async Task PlayDeathAnimation()
	{
		await this.Delay(120);

		// Create a fade out animation for the enemy
		var tween = CreateTween();
		tween.TweenProperty(CharacterSprite, "modulate:a", 0f, .35f)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);

		await ToSignal(tween, "finished");
		await this.Delay(100);
	}

	/// <summary>
	/// Initializes the available actions for the enemy.
	/// </summary>
	private void InitializeAvailableActions()
	{
		foreach (var actionData in CharacterData.AvailableActions)
		{
			AvailableActions.Add(new Action(actionData, new DefaultActionBehaviour()));
		}
	}

	private void UpdateIntent()
	{
		if (AvailableActions.Count == 0)
		{
			return;
		}

		var next = NextAction;
		var damage = string.Join(", ", next.DamageTypes).ToLowerInvariant();
		var amount = next.BaseFireDamage > 0 ? next.BaseFireDamage : next.BasePhysicalDamage;
		Intent.Text = string.Format(IntentFormat, next.Name, $"{amount} {damage}", Health, CharacterData.MaxHealth);
	}

	public override void _Process(double delta)
	{
		if (Intent != null)
        {
            UpdateIntent();
        }
    }

	/// <summary>
	/// Describes the encounter's elemental weaknesses and resistances.
	/// </summary>
	public string GetResistanceSummary()
	{
		var descriptions = new List<string>
		{
			string.Format(WeaknessFormat, DamageType.Light.ToString().ToLowerInvariant(), CharacterData.BaseLightDamageMultiplier)
		};

		if (CharacterData.BaseIceDamageMultiplier > 1)
		{
			descriptions.Add(string.Format(WeaknessFormat, DamageType.Ice.ToString().ToLowerInvariant(), CharacterData.BaseIceDamageMultiplier));
		}

		if (CharacterData.BaseFireDamageMultiplier < 1)
		{
			descriptions.Add(string.Format(ResistanceFormat, DamageType.Fire.ToString().ToLowerInvariant(), CharacterData.BaseFireDamageMultiplier));
		}

		return string.Join(" · ", descriptions);
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
