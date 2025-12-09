using Godot;

/// <summary>
/// Displays a reward block, allowing players to choose which one they want.
/// </summary>
public partial class RewardUI : Container
{
	/// <summary>
	/// The reward associated with this UI element.
	/// </summary>
	public Reward Reward { get; private set; }

	private Label NameLabel { get; set; }

	private RichTextLabel DescriptionLabel { get; set; }

	private TextureRect Icon { get; set; }

	public override void _Ready()
	{
		var pathStart = "VBoxContainer/";
		NameLabel = GetNode<Label>(pathStart + "Name");
		DescriptionLabel = GetNode<RichTextLabel>(pathStart + "Description");
		Icon = GetNode<TextureRect>(pathStart + "IconMargin/Icon");
	}

	/// <summary>
	/// Sets up the reward UI for a blessing.
	/// </summary>
	public void Setup(Blessing blessing)
	{
		NameLabel.Text = $"{blessing.Level} {blessing.Domain} blessing";
		DescriptionLabel.Text = "An additional blessing from a God.";
		Icon.Texture = null;
		// Icon.Texture = blessing.GetIcon(); TODO: Implement blessing icons

		Reward = new Reward
		{
			Type = RewardType.Blessing,
			BlessingReward = blessing
		};
	}

	/// <summary>
	/// Sets up the reward UI for a spell.
	/// </summary>
	public void Setup(ActionData spellData)
	{
		NameLabel.Text = spellData.Name;
		DescriptionLabel.Text = spellData.Description;
		Icon.Texture = spellData.Image;

		Reward = new Reward
		{
			Type = RewardType.Spell,
			SpellReward = spellData
		};
	}
}

public class Reward
{
	public RewardType Type { get; set; }

	internal Blessing BlessingReward { get; init; }

	internal ActionData SpellReward { get; init; }

	public Blessing GetBlessingReward()
	{
		return BlessingReward;
	}

	public ActionData GetSpellReward()
	{
		return SpellReward;
	}
}

/// <summary>
/// Represents the different types of rewards that can be granted.
/// </summary>
public enum RewardType
{
	/// <summary>
	/// Represents a blessing reward.
	/// </summary>
	Blessing,
	/// <summary>
	/// Represents a spell reward.
	/// </summary>
	Spell
}
