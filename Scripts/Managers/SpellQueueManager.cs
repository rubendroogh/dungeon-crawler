using Godot;

/// <summary>
/// Responsible for showing the correct data in the debug screen.
/// TODO: This has de facto become the Spell Queue Manager. Will rename.
/// </summary>
public partial class SpellQueueManager : Node
{
    /// <summary>
    /// Indicates whether the debug screen is currently active.
    /// </summary>
    private bool DebugMode = true;

    /// <summary>
    /// The ComponentExposer that exposes the spell queue components.
    /// </summary>
    [Export]
    private ComponentExposer SpellQueueExposer;

    /// <summary>
    /// The root node of the debug screen.
    /// </summary>
    private Control DebugRootNode => SpellQueueExposer.GetComponent<Control>(Components.SpellQueueRoot);

    /// <summary>
    /// The label that displays the current spell queue.
    /// </summary>
    private RichTextLabel SpellQueueLabel => DebugRootNode.GetNode<RichTextLabel>(Components.SpellQueueLabel);

    public override void _Ready()
    {
        if (!DebugMode)
        {
            DebugRootNode.Visible = false;
        }
    }

    /// <summary>
    /// Updates the spell queue display in the debug screen based on the player's spell queue.
    /// </summary>
    public void UpdateSpellQueue()
    {
        if (SpellQueueLabel == null)
        {
            GD.PrintErr("SpellQueueLabel is not initialized.");
            return;
        }

        // Loop through the spell queue and display each entry.
        SpellQueueLabel.Clear();
        var spellQueue = Managers.PlayerManager.GetPlayer().ActionQueue;
        foreach (var entry in spellQueue)
        {
            SpellQueueLabel.AddText($"Spell: {entry.Action.Data.Name}, Target: {entry.Target.Name}, Blessings: {entry.Blessings.Count}\n");
        }
    }
}
