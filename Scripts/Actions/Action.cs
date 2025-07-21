using Godot;

/// <summary>
/// Action is a base class for all actions in the game, such as spells.
/// An action can be executed by a character, but a spell only by a player.
/// </summary>
public partial class Action
{
    /// <summary>
    /// Data associated with the action, like damage amount, type, etc.
    /// </summary>
    public ActionData Data { get; set; }

    /// <summary>
    /// Behaviour that defines how the action is executed.
    /// This should implement the IActionBehaviour interface, which contains the logic for executing the action.
    /// </summary>
    protected IActionBehaviour Behaviour { get; set; }

    public Action(ActionData spellData, IActionBehaviour spellBehaviour)
    {
        Data = spellData;
        Behaviour = spellBehaviour;
    }

    /// <summary>
    /// Gets the behaviour of the action, which is expected to implement IActionBehaviour.
    /// </summary>
    public virtual IActionBehaviour GetBehaviour()
    {
        return Behaviour;
    }
}
