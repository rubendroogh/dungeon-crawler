using System.Linq;
using Godot;

/// <summary>
/// Responsible for showing the player's action queue in the UI.
/// </summary>
public partial class SpellQueueManager : Node
{
    /// <summary>
    /// The ComponentExposer that exposes the spell queue components.
    /// </summary>
    [Export]
    private ComponentExposer SpellQueueExposer;

    /// <summary>
    /// The scene containing the individual SpellQueueUI element.
    /// </summary>
    [Export]
    private PackedScene SpellQueueUIScene;

    /// <summary>
    /// The root node of the spell queue list.
    /// </summary>
    private Control SpellQueue => SpellQueueExposer.GetComponent<Control>(Components.SpellQueueList);

    /// <summary>
    /// Updates the spell queue list based on the player's spell queue.
    /// </summary>
    public void UpdateSpellQueue()
    {
        if (SpellQueue == null)
        {
            GD.PrintErr("SpellQueue is not initialized.");
            return;
        }

        // Remove all existing children
        SpellQueue.GetChildren().ToList().ForEach(child => child.QueueFree());

        // Add instances for each spell in the player's spell queue
        var spellQueue = Managers.PlayerManager.GetPlayer().ActionQueue;
        foreach (var entry in spellQueue)
        {
            var spellQueueUIInstance = SpellQueueUIScene.Instantiate<SpellQueueUI>();
            spellQueueUIInstance.Setup(entry.Action, entry.Blessings);
            SpellQueue.AddChild(spellQueueUIInstance);
        }
    }
}
