using Godot;
using System;
using System.Collections.Generic;
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
    /// The currently selected reward.
    /// </summary>
    public Reward SelectedReward { get; set; }

    /// <summary>
    /// The node to place the reward items in.
    /// </summary>
    private Node RewardContainer { get; set; }

    /// <summary>
    /// The button to confirm the reward selection.
    /// </summary>
    private Button ConfirmSelectionButton { get; set; }

    /// <summary>
    /// The container for displaying a message when there are no possible rewards.
    /// </summary>
    private Container NoPossibleRewardsContainer { get; set; }

    /// <summary>
    /// The list of currently selected card rewards. Used to prevent duplicate selections.
    /// </summary>
    private List<Blessing> SelectedCardRewards = new List<Blessing>();

    /// <summary>
    /// The list of currently selected spell rewards. Used to prevent duplicate selections.
    /// </summary>
    private List<ActionData> SelectedSpellRewards = new List<ActionData>();

    /// <summary>
    /// The number of possible card rewards that can be selected.
    /// </summary>
    private int PossibleCardRewardsCount => GetPossibleCardRewards().Count;

    /// <summary>
    /// The number of possible spell rewards that can be selected.
    /// </summary>
    private int PossibleSpellRewardsCount => GetPossibleSpellRewards().Count;

    public override void _Ready()
    {
        if (RewardItemPackedScene == null)
        {
            GD.PrintErr("RewardItemPackedScene is not set in the inspector.");
            return;
        }

        RewardContainer = GetNode("MarginContainer/VBoxContainer/RewardContainer");
        ConfirmSelectionButton = GetNode<Button>("MarginContainer/VBoxContainer/ConfirmSelectionButton");
        NoPossibleRewardsContainer = GetNode<Container>("MarginContainer/VBoxContainer/RewardContainer/NoPossibleRewardsMessage");

        // Disable the no rewards message container initially
        NoPossibleRewardsContainer.Visible = false;
    }

    /// <summary>
    /// Generates a specified number of reward items.
    /// </summary>
    public void GenerateRewards(int count)
    {
        if (NoPossibleRewardsContainer == null || RewardContainer == null)
        {
            GD.PrintErr("RewardContainer or NoPossibleRewardsContainer is not set.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            if (PossibleCardRewardsCount == 0 && PossibleSpellRewardsCount == 0)
            {
                DisplayNoMoreRewardsMessage();
                break;
            }

            var rewardItem = RewardItemPackedScene.Instantiate();
            var rewardUI = rewardItem.GetChild(0).GetChild<RewardUI>(0); // This is disgusting
            RewardContainer.AddChild(rewardItem);

            // 50/50 chance to select a card or a spell
            // If this gets more complicated, we need another solution
            // Possibly with an IReward
            var cardRewardChance = 0.5f;
            if (GD.Randf() < cardRewardChance && PossibleCardRewardsCount > 0 || PossibleSpellRewardsCount == 0)
            {
                var cardReward = GetCardReward();
                if (cardReward != null)
                {
                    SelectedCardRewards.Add(cardReward);
                    rewardUI.Setup(cardReward);
                }
            }
            else
            {
                var spellChosen = GetSpellReward();
                if (spellChosen != null)
                {
                    SelectedSpellRewards.Add(spellChosen);
                    rewardUI.Setup(spellChosen);
                }
            }
        }

        SelectedCardRewards.Clear();
        SelectedSpellRewards.Clear();
    }

    /// <summary>
    /// Sets the currently selected reward.
    /// </summary>
    public void SetSelectedReward(Reward reward)
    {
        if (reward == null)
        {
            GD.PrintErr("Attempted to set a null reward.");
            return;
        }

        SelectedReward = reward;
        ConfirmSelectionButton.Disabled = false;
    }

    /// <summary>
    /// Confirms the selection of the currently selected reward by adding it to the player's inventory.
    /// </summary>
    public void ConfirmSelection()
    {
        if (SelectedReward == null)
        {
            GD.PrintErr("No reward selected.");
            return;
        }

        // Add the selected reward to the appropriate managerX
        if (SelectedReward.Type == RewardType.Card)
        {
            Managers.DeckManager.AddCardToDeck(SelectedReward.CardReward);
        }
        else if (SelectedReward.Type == RewardType.Spell)
        {
            Managers.SpellListManager.AddSpell(SelectedReward.SpellReward);
        }

        // Clear the selection and reset the UI
        SelectedReward = null;
        var rewardNodes = RewardContainer.GetChildren();
        foreach (var node in rewardNodes)
        {
            node.QueueFree();
        }
    }

    /// <summary>
    /// Generates a card reward.
    /// </summary>
    private Blessing GetCardReward()
    {
        var possibleRewards = GetPossibleCardRewards();
        if (possibleRewards.Count > 0)
        {
            // Assign weights: higher rank = lower weight (rarer)
            // Example: weight = 1 / rank (Two=1, Ace=14)
            var weights = possibleRewards.Select(card => 1f / (float)card.Rank).ToList();
            float totalWeight = weights.Sum();
            float randomValue = GD.Randf() * totalWeight;

            float cumulative = 0f;
            for (int i = 0; i < possibleRewards.Count; i++)
            {
                cumulative += weights[i];
                if (randomValue <= cumulative)
                {
                    return possibleRewards[i];
                }
            }

            // Fallback
            return possibleRewards.Last();
        }

        return null;
    }

    /// <summary>
    /// Generates a spell reward that the player does not already have.
    /// </summary>
    private ActionData GetSpellReward()
    {
        var availableSpells = GetPossibleSpellRewards();
        if (availableSpells.Count > 0)
        {
            var spell = availableSpells[(int)(GD.Randi() % availableSpells.Count)];
            return spell;
        }

        return null;
    }

    /// <summary>
    /// Get every possible card reward. This is a list of all unique card rewards that can be obtained.
    /// </summary>
    private List<Blessing> GetPossibleCardRewards()
    {
        var possibleRewards = new List<Blessing>();
        for (int rank = 1; rank <= 13; rank++)
        {
            for (int suit = 0; suit < 4; suit++)
            {
                var card = new Blessing { Rank = (Rank)rank, Domain = (Domain)suit };
                if (!SelectedCardRewards.Contains(card))
                {
                    possibleRewards.Add(card);
                }
            }
        }

        return possibleRewards;
    }

    /// <summary>
    /// Gets a list of all possible spell rewards that can be obtained.
    /// </summary>
    private List<ActionData> GetPossibleSpellRewards()
    {
        // Get a spell from the preloader
        var resourceList = SpellRewardPreloader.GetResourceList();

        // Filter list on spells that the player does not already possess or has not selected
        var availableSpells = resourceList.Select(res => SpellRewardPreloader.GetResource(res) as ActionData)
            .Where(spell => !Managers.SpellListManager.AvailableSpells.Any(s => s.Data.Name == spell.Name))
            .Where(spell => !SelectedSpellRewards.Any(s => s.Name == spell.Name))
            .ToList();

        return availableSpells;
    }

    /// <summary>
    /// Displays a message indicating that there are no more rewards available.
    /// </summary>
    private void DisplayNoMoreRewardsMessage()
    {
        NoPossibleRewardsContainer.Visible = true;
    }
}
