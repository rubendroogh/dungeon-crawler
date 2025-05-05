using Godot;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages casting of spells and current state of all related variables.
/// </summary>
public partial class SpellCastingManager : Node
{
    /// <summary>
    /// Signal that is emitted when a spell is cast.
    /// </summary>
    [Signal]
    public delegate void SpellCastEventHandler();

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
    /// The spell to cast.
    /// </summary>
    public Spell SelectedSpell { get; set; }

    /// <summary>
    /// The maximum number of cards that can be selected for casting a spell.
    /// </summary>
    public int MaxSelectedCards { get; set; } = 4;

    /// <summary>
    /// The label that shows the currently selected cards.
    /// </summary>
    private Label SelectedCardsLabel { get; set; }

    /// <summary>
    /// Mark the card as selected and ready to cast a spell with.
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
        return true;
    }

    /// <summary>
    /// Unmark the card as selected and ready to cast a spell with.
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
        return true;
    }

    /// <summary>
    /// Set the currently selected spell.
    /// This method creates a new Spell instance using the provided SpellData and a default spell behavior.
    /// </summary>
    public void SetSelectedSpell(SpellData spellData)
    {
        if (spellData == null)
        {
            GD.PrintErr("SpellData is null");
            return;
        }

        ManagerRepository.BattleLogManager.AddToLog($"Selected spell: {spellData.Name}");
        EmitSignal(SignalName.SpellSelected, spellData.Name);

        // TODO: Clear selected cards
        SelectedSpell = ManagerRepository.SpellListManager.GetSpell(spellData.Name);
        MaxSelectedCards = spellData.MaxManaCharges;
    }

    /// <summary>
    /// Set the currently selected spell.
    /// </summary>
    public void SetSelectedSpell(Spell spell)
    {
        SelectedSpell = spell;
        MaxSelectedCards = spell.Data.MaxManaCharges;
    }

    /// <summary>
    /// Cast the currently selected spell using the currently selected cards.
    /// </summary>
    /// <param name="target">The chosen target.</param>
    public void CastSpell(Character target)
    {
        if (SelectedCards.Count == 0)
        {
            ManagerRepository.BattleLogManager.AddToLog("No cards selected.");
            return;
        }

        if (SelectedSpell == null)
        {
            ManagerRepository.BattleLogManager.AddToLog("No spell selected.");
            return;
        }

        var totalDamage = SelectedSpell.Behaviour.CalculateTotalDamage(SelectedCards, SelectedSpell.Data);
        target.CurrentHealth -= totalDamage;

        ManagerRepository.BattleLogManager.AddToLog($"Cast {SelectedSpell.Data.Name} on {target.Name} for {(int)totalDamage} damage!");
        EmitSignal(SignalName.SpellCast);
        SelectedCards.Clear();

        // TODO: Handle all other effects of the spell
        target.TakeDamage(totalDamage);
    }
}
