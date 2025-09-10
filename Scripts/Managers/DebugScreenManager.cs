using Godot;

/// <summary>
/// Responsible for showing the correct data in the debug screen.
/// </summary>
public partial class DebugScreenManager : Node
{
    /// <summary>
    /// Indicates whether the debug screen is currently active.
    /// </summary>
    private bool DebugMode = true;

    /// <summary>
    /// The root node of the debug screen.
    /// </summary>
    private Control DebugRootNode;

    /// <summary>
    /// The label that displays the current spell queue.
    /// </summary>
    private RichTextLabel SpellQueueLabel;

    public override void _Ready()
    {
        DebugRootNode = GetTree().Root.GetNode<Control>("Root/UI/HUD/Debug");
        SpellQueueLabel = DebugRootNode.GetNode<RichTextLabel>("VBoxContainer/SpellQueue/RichTextLabel");

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
            SpellQueueLabel.AddText($"Spell: {entry.Action.Data.Name}, Target: {entry.Target.Name}, Cards: {entry.Cards.Count}\n");
        }
    }
}
