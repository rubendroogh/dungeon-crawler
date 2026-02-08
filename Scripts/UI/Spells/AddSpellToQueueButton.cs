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
        ActionManager.Instance.SpellSelected += UpdateButtonInteractableState;
		ManaSourceManager.Instance.BlessingStateChanged += UpdateButtonInteractableState;
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
        if (!ActionManager.Instance.SpellIsSelected)
        {
            Disabled = true;
            return;
        }

        // Whenever any mana changes, we need to check if the selected spell can be paid for agian
        if (!ManaSourceManager.Instance.SelectedManaCanPay(ActionManager.Instance.SelectedSpell.Data.Cost))
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
        if (ActionManager.Instance.SelectedSpell == null)
        {
            GD.PrintErr("No spell selected to add to the queue.");
            return;
        }

        if (BattleManager.Instance.CurrentTurnPhase != TurnPhase.Main)
        {
            GD.PrintErr("Cannot add a spell to the queue outside of the Main phase.");
            return;
        }

        var player = PlayerManager.Instance.GetPlayer();
        var selectedSpell = ActionManager.Instance.SelectedSpell;
        if (!ManaSourceManager.Instance.SelectedManaCanPay(selectedSpell.Data.Cost))
        {
            return;
        }

        var manaUsed = ManaSourceManager.Instance.BlessingBar.BlessingsMarkedForUse;
        ManaSourceManager.Instance.SpendSelectedMana();
        ManaSourceManager.Instance.DeselectAllMana();
        _ = SoundEffectManager.Instance.PlayButtonClick();

        var target = ActionManager.Instance.SelectedTarget;
        player.QueueAction(selectedSpell, target, manaUsed);
    }
}
