using Godot;

/// <summary>
/// Cancels the player's prepared queue and releases its reserved blessings.
/// </summary>
public partial class ClearSpellQueueButton : Button
{
    public override void _Ready()
    {
        Pressed += OnPressed;
    }

    private void OnPressed()
    {
        if (!BattleManager.Instance.CanQueueSpells)
        {
            return;
        }

        PlayerManager.Instance.GetPlayer().ActionQueue.Clear();
        ActionManager.Instance.ClearSelectedSpell();
        ManaSourceManager.Instance.ResetAllMana();
        SpellQueueManager.Instance.UpdateSpellQueue();
    }

    public override void _ExitTree()
    {
        Pressed -= OnPressed;
    }
}
