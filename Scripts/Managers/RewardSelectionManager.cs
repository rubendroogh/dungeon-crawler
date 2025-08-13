using Godot;
using System;
using System.Linq;

/// <summary>
/// Manages the selection of rewards for the player.
/// </summary>
public partial class RewardSelectionManager : Node
{
    /// <summary>
    /// The packed scene for the reward item.
    /// </summary>
    [Export]
    public PackedScene RewardItemPackedScene { get; set; }

    /// <summary>
    /// Preloads the possible spell rewards.
    /// </summary>
    [Export]
    public ResourcePreloader SpellRewardPreloader { get; set; }

    /// <summary>
    /// The node to place the reward items in.
    /// </summary>
    private Node RewardContainer { get; set; }

    public override void _Ready()
    {
        if (RewardItemPackedScene == null)
        {
            GD.PrintErr("RewardItemPackedScene is not set in the inspector.");
            return;
        }

        RewardContainer = GetNode("MarginContainer/VBoxContainer/RewardContainer");

        GenerateSingleReward();
        GenerateSingleReward();
        GenerateSingleReward();
    }

    /// <summary>
    /// Generates one of the rewards that the player can select.
    /// TODO: Don't generate duplicate rewards.
    /// </summary>
    private void GenerateSingleReward()
    {
        var rewardItem = RewardItemPackedScene.Instantiate();
        var rewardUI = rewardItem.GetChild<RewardUI>(0);
        RewardContainer.AddChild(rewardItem);

        // 50/50 chance to select a card or a spell
        if (GD.Randf() < 0.5f)
        {
            GenerateCardReward(rewardUI);
        }
        else
        {
            var spellChosen = GenerateSpellReward(rewardUI);
            if (!spellChosen)
            {
                // If no spell was chosen, fallback to a card reward
                GenerateCardReward(rewardUI);
            }
        }
    }

    /// <summary>
    /// Generates a card reward.
    /// </summary>
    private void GenerateCardReward(RewardUI rewardUI)
    {
        // TODO: Make it more rare to receive higher ranks
        var card = new Card
        {
            Rank = (Rank)(GD.Randi() % 13 + 1), // Random rank from 1 to 13
            Suit = (Suit)(GD.Randi() % 4) // Random suit from 0 to 3
        };

        rewardUI.Setup(card);
    }

    /// <summary>
    /// Generates a spell reward that the player does not already have.
    /// </summary>
    /// <returns>True if a spell has been chosen, false if there is no spell left to choose.</returns>
    private bool GenerateSpellReward(RewardUI rewardUI)
    {
        // Get a spell from the preloader
        var resourceList = SpellRewardPreloader.GetResourceList();

        // Filter list on spells that the player does not already possess
        var availableSpells = resourceList.Select(res => SpellRewardPreloader.GetResource(res) as ActionData)
            .Where(spell => !Managers.SpellListManager.AvailableSpells.Any(s => s.Data.Name == spell.Name))
            .ToList();

        if (availableSpells.Count > 0)
        {
            var spell = availableSpells[(int)(GD.Randi() % availableSpells.Count)];
            rewardUI.Setup(spell);

            return true;
        }

        return false;
    }
}
