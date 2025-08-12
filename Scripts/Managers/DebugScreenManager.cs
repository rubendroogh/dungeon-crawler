using Godot;

/// <summary>
/// Responsible for showing the correct data in the debug screen.
/// </summary>
public partial class DebugScreenManager : Node
{
    private bool DebugMode = true;

    private Control DebugRootNode;

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

    public void UpdateSpellQueue()
    {
        // Update the spell queue display in the debug screen
        if (SpellQueueLabel == null)
        {
            GD.PrintErr("SpellQueueLabel is not initialized.");
            return;
        }

        // Clear the current text
        SpellQueueLabel.Clear();

        // Get the current spell queue from the battle manager
        var spellQueue = Managers.PlayerManager.GetPlayer().SpellQueue;

        // Display the current spell queue
        foreach (var entry in spellQueue)
        {
            SpellQueueLabel.AddText($"Spell: {entry.Action.Data.Name}, Target: {entry.Target.Name}, Cards: {entry.Cards.Count}\n");
        }
    }
}
