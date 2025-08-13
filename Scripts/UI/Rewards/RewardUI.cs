using Godot;

/// <summary>
/// Displays a reward block, allowing players to choose which one they want.
/// </summary>
public partial class RewardUI : PanelContainer
{
    private Label NameLabel { get; set; }

    private RichTextLabel DescriptionLabel { get; set; }

    private TextureRect Icon { get; set; }

    private RewardType RewardType { get; set; }

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
        RewardType = RewardType.Card;
        NameLabel.Text = $"{card.Rank} of {card.Suit}";
        DescriptionLabel.Text = "An additional card for your deck.";
        Icon.Texture = card.GetIcon();
    }

    /// <summary>
    /// Sets up the reward UI for a spell.
    /// </summary>
    public void Setup(ActionData spellData)
    {
        RewardType = RewardType.Spell;
        NameLabel.Text = spellData.Name;
        DescriptionLabel.Text = spellData.Description;
        Icon.Texture = spellData.Image;
    }
}

/// <summary>
/// Represents the different types of rewards that can be granted.
/// </summary>
enum RewardType
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