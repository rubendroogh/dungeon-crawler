using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Manages casting of spells, handling of actions, and current state of all related variables.
/// This includes managing selected cards, spells, and targets, as well as emitting signals
/// for UI updates and game state changes.
/// </summary>
public partial class ActionManager : Node
{
    /// <summary>
    /// Signal that is emitted when a spell is selected.
    /// </summary>
    [Signal]
    public delegate void SpellSelectedEventHandler(string spellName);

    /// <summary>
    /// Signal that is emitted when the cards should be reset.
    /// </summary>
    [Signal]
    public delegate void CardsResetEventHandler();

    /// <summary>
    /// The currently selected cards.
    /// </summary>
    public List<Blessing> SelectedCards { get; } = [];

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
    /// The currently selected target for the spell.
    /// </summary>
    public Character SelectedTarget { get; set; }

    /// <summary>
    /// The context for keyword effects.
    /// </summary>
    public KeywordContext KeywordContext { get; } = new KeywordContext();

    /// <summary>
    /// Mark the card as selected and ready to cast a spell with and updates the label.
    /// </summary>
    /// <param name="card">The card to add.</param>
    /// <returns>True if the card was added, false if the maximum number of cards is reached.</returns>
    public bool AddCardToSelection(Blessing card)
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
    public bool RemoveCardFromSelection(Blessing card)
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
    /// This method finds the spell by its ActionData name from the Spell List.
    /// </summary>
    public void SetSelectedSpell(ActionData spellData)
    {
        if (spellData == null)
        {
            GD.PrintErr("SpellData is null");
            return;
        }

        var selectedSpell = Managers.SpellListManager.GetSpell(spellData.Name);
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
    /// Handles the result of resolving a DamagePacket.
    /// This method applies all damages and heals in the packet and returns the total damage dealt.
    /// </summary>
    /// <returns>The total damage dealt by the action.</returns>
    public async Task<float> ApplyResolveResult(ResolveResult resolveResult)
    {
        if (resolveResult == null)
        {
            GD.PrintErr("ResolveResult is null");
            return 0f;
        }

        if (resolveResult.Target.IsDead)
        {
            return 0f;
        }

        await resolveResult.Target.Damage(resolveResult);
        return resolveResult.TotalDamageAmount;
    }

    /// <summary>
    /// Resets the mana cards to be usable again.
    /// Used when a new turn starts.
    /// </summary>
    public void ResetCards()
    {
        EmitSignal(SignalName.CardsReset);

        SelectedCards.Clear();
        UpdateSelectedCardsLabel();
    }

    /// <summary>
    /// Updates the label that shows the currently selected cards.
    /// </summary>
    private void UpdateSelectedCardsLabel()
    {
        SelectedCardsLabel.Text = $"{SelectedCards.Count}/{MaxSelectedCards} mana charges";
    }
}