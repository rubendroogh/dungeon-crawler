using Godot;

/// <summary>
/// Action is a base class for all actions in the game, such as spells.
/// An action can be executed by a character, but a spell only by a player.
/// </summary>
public partial class Action
{
    public ActionData Data { get; set; }

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
