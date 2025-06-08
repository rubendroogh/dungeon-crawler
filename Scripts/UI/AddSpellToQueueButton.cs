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
        if (ManagerRepository.SpellCastingManager.SelectedSpell == null)
        {
            GD.PrintErr("No spell selected to add to the queue.");
            return;
        }

        if (ManagerRepository.BattleManager.CurrentTurnPhase != TurnPhase.Main)
        {
            GD.PrintErr("Cannot add a spell to the queue outside of the Main phase.");
            return;
        }

        var selectedSpell = ManagerRepository.SpellCastingManager.SelectedSpell;
        var selectedCards = ManagerRepository.SpellCastingManager.SelectedCards;
        var target = ManagerRepository.SpellCastingManager.SelectedTarget;

        ManagerRepository.SpellCastingManager.AddSpellToQueue(selectedSpell, selectedCards, target);
    }
}
