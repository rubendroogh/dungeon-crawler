using Godot;
using Priest.Managers;
using System;

/// <summary>
/// Gets the relevant scenes from the DungeonManager
/// and puts them on the screen.
/// </summary>
public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; }

    /// <summary>
    /// The DungeonGenerator instance that generates the scenes for the player to go through.
    /// </summary>
    private DungeonGenerator DungeonGenerator { get; set; }

    /// <summary>
    /// The prefab for a choice button in the scene UI. This should be a Button node with a Label child for the choice text.
    /// </summary>
    [Export]
    public PackedScene SceneChoicePrefab { get; set; }

    /// <summary>
    /// The root node of the scene UI, which contains the title, text, and choices for the current scene.
    /// </summary>
    public CanvasItem SceneNode { get; set; }

    public override void _Ready()
    {
        Instance = this;
        DungeonGenerator = new DungeonGenerator();
    }

    /// <summary>
    /// Starts the dungeon by generating the first scene and displaying it on the screen.
    /// </summary>
    public void StartDungeon()
    {
        SceneNode = GetTree().Root.GetNode("Root/UI/Scene") as CanvasItem;
        SetScene(DungeonGenerator.Generate());
    }

    /// <summary>
    /// Sets the current scene to the given scene by updating the UI elements with the scene's title, text, and choices.
    /// </summary>
    /// <param name="scene">The scene to display.</param>
    public void SetScene(Scene scene)
    {
        // Get the nodes
        // Put the fields from the current scene into the nodes

        GD.Print("Setting scene: " + scene.Title);
        GD.Print(SceneNode);

        var titleLabel = SceneNode.FindChild("SceneTitle") as RichTextLabel;
        var textLabel = SceneNode.FindChild("SceneText") as RichTextLabel;
        var choicesContainer = SceneNode.FindChild("SceneChoices") as VBoxContainer;

        GD.Print(titleLabel);
        GD.Print(textLabel);
        GD.Print(choicesContainer);

        titleLabel.Text = scene.Title;
        textLabel.Text = scene.Text;

        // Clear old choices
        foreach (Node child in choicesContainer.GetChildren())
        {
            GD.Print("Freeing child: " + child);
            child.QueueFree();
        }

        // Add new choices
        foreach (var choice in scene.Choices)
        {
            var choiceButton = SceneChoicePrefab.Instantiate() as Button;
            choiceButton.Text = choice.Title;

            GD.Print("Adding choice: " + choice.Title);
            choicesContainer.AddChild(choiceButton);
        }
    }
}
