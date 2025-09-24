using Godot;

public partial class AddSpellToQueueButton : Button
{
    public override void _Ready()
    {
        Pressed += OnPressed;
    }

    /// Adds the currently selected spell to the spell queue when the button is pressed.
    public void OnPressed()
    {
        if (Managers.ActionManager.SelectedSpell == null)
        {
            GD.PrintErr("No spell selected to add to the queue.");
            return;
        }

        if (Managers.BattleManager.CurrentTurnPhase != TurnPhase.Main)
        {
            GD.PrintErr("Cannot add a spell to the queue outside of the Main phase.");
            return;
        }

        var player = Managers.PlayerManager.GetPlayer();
        var selectedSpell = Managers.ActionManager.SelectedSpell;
        var selectedCards = Managers.ActionManager.SelectedCards;
        var target = Managers.ActionManager.SelectedTarget;

        Managers.SoundEffectManager.PlayButtonClick();
        player.QueueAction(selectedSpell, target, selectedCards);
    }
}
