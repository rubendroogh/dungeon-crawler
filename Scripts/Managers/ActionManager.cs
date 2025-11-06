using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Manages casting of spells, handling of actions, and current state of all related variables.
/// Also responsible for emitting signals when actions occur.
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
    /// TODO: Change this to a more general "SelectionChanged" event and don't use cards anymore.
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
    /// The currently selected target for the spell.
    /// </summary>
    public Character SelectedTarget { get; set; }

    /// <summary>
    /// The context for keyword effects.
    /// </summary>
    public KeywordContext KeywordContext { get; } = new KeywordContext();

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

        SelectedSpell = spell;

        EmitSignal(SignalName.SpellSelected, spell.Data.Name);
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
}