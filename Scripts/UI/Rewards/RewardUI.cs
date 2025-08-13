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
	/// Sets up the reward UI for a card.
	/// </summary>
	public void Setup(Card card)
	{
		NameLabel.Text = $"{card.Rank} of {card.Suit}";
		DescriptionLabel.Text = "An additional card for your deck.";
		Icon.Texture = card.GetIcon();

		Reward = new Reward
		{
			Type = RewardType.Card,
			CardReward = card
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

	internal Card CardReward { get; init; }

	internal ActionData SpellReward { get; init; }

	public Card GetCardReward()
	{
		return CardReward;
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
	/// Represents a playing card reward.
	/// </summary>
	Card,
	/// <summary>
	/// Represents a spell reward.
	/// </summary>
	Spell
}
