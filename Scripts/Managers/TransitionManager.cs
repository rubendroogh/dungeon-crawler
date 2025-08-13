using Godot;
using System;

public partial class TransitionManager : Node
{
    private CanvasItem WorldNode { get; set; }

    private CanvasItem HUDNode { get; set; }

    private CanvasItem CharacterCreationNode { get; set; }

    private CanvasItem RewardSelectionNode { get; set; }

    public override void _Ready()
    {
        WorldNode = GetTree().Root.GetNode("Root/World") as CanvasItem;
        HUDNode = GetTree().Root.GetNode("Root/UI/HUD") as CanvasItem;
        CharacterCreationNode = GetTree().Root.GetNode("Root/UI/CharacterCreation") as CanvasItem;
        RewardSelectionNode = GetTree().Root.GetNode("Root/UI/RewardSelection") as CanvasItem;
    }

    /// <summary>
    /// Transitions to the character creation screen by hiding the world and HUD nodes.
    /// </summary>
    public void ToCharacterCreation()
    {
        // Toggle the visibility of the world node
        if (WorldNode != null)
        {
            WorldNode.Visible = false;
            WorldNode.SetProcess(false);
        }

        // Toggle the visibility of the HUD
        if (HUDNode != null)
        {
            HUDNode.Visible = false;
            HUDNode.SetProcess(false);
        }


        if (CharacterCreationNode != null)
        {
            CharacterCreationNode.Visible = true;
            CharacterCreationNode.SetProcess(true);
        }

        // Hide the reward selection UI
        if (RewardSelectionNode != null)
        {
            RewardSelectionNode.Visible = false;
            RewardSelectionNode.SetProcess(false);
        }
    }

    /// <summary>
    /// Transitions from character creation to the game by showing the world and HUD nodes.
    /// </summary>
    public void CharacterCreationToGame()
    {
        // Toggle the visibility of the world node
        if (WorldNode != null)
        {
            WorldNode.Visible = true;
            WorldNode.SetProcess(true);
        }

        // Toggle the visibility of the HUD
        if (HUDNode != null)
        {
            HUDNode.Visible = true;
            HUDNode.SetProcess(true);
        }

        // Hide the character creation UI
        if (CharacterCreationNode != null)
        {
            CharacterCreationNode.Visible = false;
            CharacterCreationNode.SetProcess(false);
        }

        // Hide the reward selection UI
        if (RewardSelectionNode != null)
        {
            RewardSelectionNode.Visible = false;
            RewardSelectionNode.SetProcess(false);
        }

        // Temporarily immediately start the battle
        // This is a placeholder for actual game logic to start properly in the dungeon
        Managers.BattleManager.InitializeBattle();
    }

    public void ToRewardSelection()
    {
        // Hide the world and HUD nodes
        if (WorldNode != null)
        {
            WorldNode.Visible = false;
            WorldNode.SetProcess(false);
        }

        if (HUDNode != null)
        {
            HUDNode.Visible = false;
            HUDNode.SetProcess(false);
        }

        // Show the reward selection node
        if (RewardSelectionNode != null)
        {
            RewardSelectionNode.Visible = true;
            RewardSelectionNode.SetProcess(true);
        }
    }

    public void RewardSelectionToGame()
    {
        // Hide the reward selection UI
        if (RewardSelectionNode != null)
        {
            RewardSelectionNode.Visible = false;
            RewardSelectionNode.SetProcess(false);
        }

        // Show the world and HUD nodes
        if (WorldNode != null)
        {
            WorldNode.Visible = true;
            WorldNode.SetProcess(true);
        }

        if (HUDNode != null)
        {
            HUDNode.Visible = true;
            HUDNode.SetProcess(true);
        }

        // Temporarily immediately start the battle\
        Managers.BattleManager.InitializeBattle();
    }
}
