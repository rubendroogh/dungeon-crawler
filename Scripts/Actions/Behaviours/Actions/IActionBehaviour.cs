using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// ISpellBehaviour is an interface that defines the behaviour of an action.
/// </summary>
public interface IActionBehaviour
{
    /// <summary>
    /// Resolves the action using the selected targets and the action data. Does not actually apply the effect to the target.
    /// </summary>
    /// <returns>The result of the spellcast, a list of damages, healing, and status effects.</returns>
    public ResolveResult Resolve(ActionData actionData, List<Character> targets);

    /// <summary>
    /// Animates the spell cast for the given targets.
    /// </summary>
    public Task AnimateSpellCast(ActionData spellData, List<Character> targets);
}
