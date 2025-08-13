using Godot;

public partial class BaseRewardUI : PanelContainer
{
    private Label NameLabel { get; set; }

    private RichTextLabel DescriptionLabel { get; set; }

    private TextureRect Icon { get; set; }

    public void Setup()
    {

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