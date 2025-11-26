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
        Managers.ActionManager.SpellSelected += UpdateButtonInteractableState;
		Managers.ManaSourceManager.BlessingStateChanged += UpdateButtonInteractableState;
	}

    /// <summary>
    /// Overload for UpdateButtonInteractableState event handler to fit the SpellSelected event.
    /// </summary>
    private void UpdateButtonInteractableState(string value)
    {
        UpdateButtonInteractableState();
    }

    /// <summary>
    /// Updates the button's enabled state based on whether a spell is selected
    /// and if there is enough mana to queue it.
    /// </summary>
    private void UpdateButtonInteractableState()
    {
        // If no spell is selected, we cannot queue one
        if (!Managers.ActionManager.SpellIsSelected)
        {
            Disabled = true;
            return;
        }

        // Whenever any mana changes, we need to check if the selected spell can be paid for agian
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

        var player = Managers.PlayerManager.GetPlayer();
        var selectedSpell = Managers.ActionManager.SelectedSpell;
        if (!Managers.ManaSourceManager.CanPay(selectedSpell.Data.Cost))
        {
            return;
        }

        Managers.ManaSourceManager.SpendSelectedMana();
        Managers.ManaSourceManager.DeselectAllMana();
        _ = Managers.SoundEffectManager.PlayButtonClick();

        var target = Managers.ActionManager.SelectedTarget;
        player.QueueAction(selectedSpell, target);
    }
}
