using Godot;
using System.Collections.Generic;

/// <summary>
/// Manages casting of spells, handling of actions, and current state of all related variables.
/// </summary>
public partial class ActionManager : Node
{
    /// <summary>
    /// Signal that is emitted when a spell is selected.
    /// </summary>
    [Signal]
    public delegate void SpellSelectedEventHandler(string spellName);

    /// <summary>
    /// The currently selected cards.
    /// </summary>
    public List<Card> SelectedCards { get; } = [];

    /// <summary>
    /// The currently selected spell.
    /// This is the spell that is currently being prepared for queueing.
    /// </summary>
    public Spell SelectedSpell { get; set; }

    /// <summary>
    /// The maximum number of cards that can be selected for casting a spell.
    /// </summary>
    public int MaxSelectedCards { get; set; } = 0;

    /// <summary>
    /// The label that shows the currently selected cards.
    /// </summary>
    [Export]
    public Label SelectedCardsLabel { get; set; }

    /// <summary>
    /// The queue of spells to be cast.
    /// This is used to manage spells that are queued for casting, allowing for multiple spells to be cast in a single turn.
    /// </summary>
    public List<ActionQueueEntry> ActionQueue { get; } = []; // TODO implement spell queueing per character

    /// <summary>
    /// The currently selected target for the spell.
    /// </summary>
    public Character SelectedTarget { get; set; }

    /// <summary>
    /// Mark the card as selected and ready to cast a spell with and updates the label.
    /// </summary>
    /// <param name="card">The card to add.</param>
    /// <returns>True if the card was added, false if the maximum number of cards is reached.</returns>
    public bool AddCardToSelection(Card card)
    {
        if (SelectedCards.Count >= MaxSelectedCards)
        {
            return false;
        }

        SelectedCards.Add(card);
        UpdateSelectedCardsLabel();
        return true;
    }

    /// <summary>
    /// Unmark the card as selected and ready to cast a spell with and updates the label.
    /// </summary>
    /// <param name="card">The card to remove.</param>
    /// <returns>True if the card was removed, false if the card was not in the selection.</returns>
    public bool RemoveCardFromSelection(Card card)
    {
        if (!SelectedCards.Contains(card))
        {
            return false;
        }

        SelectedCards.Remove(card);
        UpdateSelectedCardsLabel();
        return true;
    }

    /// <summary>
    /// Set the currently selected spell.
    /// This method creates a new Spell instance using the provided SpellData and a default spell behavior.
    /// </summary>
    public void SetSelectedSpell(ActionData spellData)
    {
        if (spellData == null)
        {
            GD.PrintErr("SpellData is null");
            return;
        }

        // TODO: Clear selected cards
        var selectedSpell = ManagerRepository.SpellListManager.GetSpell(spellData.Name);
        SetSelectedSpell(selectedSpell);
    }

    /// <summary>
    /// Set the currently selected spell.
    /// </summary>
    public void SetSelectedSpell(Spell spell)
    {
        if (spell == null)
        {
            GD.PrintErr("Spell is null");
            return;
        }

        EmitSignal(SignalName.SpellSelected, spell.Data.Name);

        SelectedSpell = spell;
        MaxSelectedCards = spell.Data.MaxManaCharges;

        UpdateSelectedCardsLabel();
    }

    /// <summary>
    /// Casts the selected spell using the selected cards and target.
    /// </summary>
    public void CastSpell(ActionQueueEntry spellQueueEntry)
    {
        if (spellQueueEntry.Spell == null)
        {
            ManagerRepository.BattleLogManager.Log("No spell selected to cast.");
            return;
        }

        if (spellQueueEntry.Target == null)
        {
            ManagerRepository.BattleLogManager.Log($"No target selected for {spellQueueEntry.Spell.Data.Name}.");
            return;
        }

        if (spellQueueEntry.Cards.Count > spellQueueEntry.Spell.Data.MaxManaCharges)
        {
            ManagerRepository.BattleLogManager.Log($"Cannot cast {spellQueueEntry.Spell.Data.Name} with {spellQueueEntry.Cards.Count} cards. Maximum is {spellQueueEntry.Spell.Data.MaxManaCharges}.");
            return;
        }

        // TODO: Implement multitarget spells
        var spellCastResult = spellQueueEntry.Spell.Behaviour.Cast(spellQueueEntry.Cards, spellQueueEntry.Spell.Data, [spellQueueEntry.Target]);
        var totalDamage = 0f;
        foreach (var damage in spellCastResult.Damages)
        {
            totalDamage += damage.Apply();
        }

        ManagerRepository.BattleLogManager.Log($"Cast {spellQueueEntry.Spell.Data.Name} on {spellQueueEntry.Target.Name} for {totalDamage} damage.");
    }

    /// <summary>
    /// Updates the label that shows the currently selected cards.
    /// </summary>
    private void UpdateSelectedCardsLabel()
    {
        SelectedCardsLabel.Text = $"{SelectedCards.Count}/{MaxSelectedCards} mana charges";
    }
}