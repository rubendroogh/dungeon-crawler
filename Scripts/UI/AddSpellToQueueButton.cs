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
        if (ManagerRepository.ActionManager.SelectedSpell == null)
        {
            GD.PrintErr("No spell selected to add to the queue.");
            return;
        }

        if (ManagerRepository.BattleManager.CurrentTurnPhase != TurnPhase.Main)
        {
            GD.PrintErr("Cannot add a spell to the queue outside of the Main phase.");
            return;
        }

        var player = ManagerRepository.BattleManager.GetPlayer();
        var selectedSpell = ManagerRepository.ActionManager.SelectedSpell;
        var selectedCards = ManagerRepository.ActionManager.SelectedCards;
        var target = ManagerRepository.ActionManager.SelectedTarget;

        player.QueueAction(selectedSpell, target, selectedCards);
    }
}
