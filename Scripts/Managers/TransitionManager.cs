using Godot;
using System;

public partial class TransitionManager : Node
{
    private CanvasItem WorldNode { get; set; }
    
    private CanvasItem HUDNode { get; set; }

    private CanvasItem CharacterCreationNode { get; set; }
    
    public override void _Ready()
    {
        WorldNode = GetTree().Root.GetNode("Root/World") as CanvasItem;
        HUDNode = GetTree().Root.GetNode("Root/UI/HUD") as CanvasItem;
        CharacterCreationNode = GetTree().Root.GetNode("Root/UI/CharacterCreation") as CanvasItem;
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
    }
}
