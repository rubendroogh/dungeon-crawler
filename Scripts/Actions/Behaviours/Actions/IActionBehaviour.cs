using System.Threading.Tasks;

/// <summary>
/// Defines the logic and effect of an action.
/// </summary>
public interface IActionBehaviour
{
    /// <summary>
    /// Resolves the action using the selected target and the action data. Does not actually apply the effect to the target.
    /// </summary>
    /// <returns>The result of the action, a list of damages, healing, and status effects.</returns>
    public Task<ResolveResult> Resolve(ActionData actionData, Character target);

    /// <summary>
    /// Animates the action being applied to the given target.
    /// </summary>
    public Task AnimateAction(ActionData actionData, Character target, Character performer = null);
}
