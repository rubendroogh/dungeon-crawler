using Godot;

public partial class AddSpellToQueueButton : Button
{
    public override void _Ready()
    {
        Pressed += OnPressed;
        InitializeCustomSignals();
    }

	/// <summary>
	/// Wires up the custom signals for the button.
	/// </summary>
	private void InitializeCustomSignals()
	{
		Managers.ActionManager.SpellSelected += OnSpellSelected;
	}

    /// <summary>
    /// Handles the selection of a spell by updating the UI.
    /// </summary>
    private void OnSpellSelected(string spellName)
    {
        // Disable the button if the selected spell cannot be paid for
        if (!Managers.ManaSourceManager.CanPay(Managers.ActionManager.SelectedSpell.Data.Cost))
        {
            Disabled = true;
        }
        else
        {
            Disabled = false;
        }
    }

    /// <summary>
    /// Adds the currently selected spell to the spell queue when the button is pressed.
    /// </summary>
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

        // HERE!

        var player = Managers.PlayerManager.GetPlayer();
        var selectedSpell = Managers.ActionManager.SelectedSpell;
        var selectedCards = Managers.ActionManager.SelectedCards;
        var target = Managers.ActionManager.SelectedTarget;

        Managers.SoundEffectManager.PlayButtonClick();
        player.QueueAction(selectedSpell, target, selectedCards);
    }
}
