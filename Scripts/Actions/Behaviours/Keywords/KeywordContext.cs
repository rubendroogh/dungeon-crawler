/// <summary>
/// KeywordContext provides all necessary context for executing keyword effects.
/// This can be expanded as needed to include more information.
/// </summary>
public class KeywordContext
{
    /// <summary>
    /// Update the keyword context with the current action, caster, and target.
    /// This should be called before processing keywords for an action.
    /// </summary>
    public void UpdateKeywordContext(Action action, Character caster, Character target)
    {
        Action = action;
        Caster = caster;
        Target = target;
    }

    /// <summary>
    /// Reset the keyword context to its default state.
    /// This should be called at the start of each new queue processing.
    /// </summary>
    public void ResetKeywordContext()
    {
        Action = null;
        Caster = null;
        Target = null;
        StormProcessed = false;
    }

    /// <summary>
    /// The action associated with this keyword context.
    /// </summary>
    public Action Action { get; private set; }

    /// <summary>
    /// The character casting the spell.
    /// </summary>
    public Character Caster { get; private set; }

    /// <summary>
    /// The character targeted by the spell.
    /// </summary>
    public Character Target { get; private set; }

    /// <summary>
    /// Whether the Storm keyword effect has been processed for the current spell cast.
    /// </summary>
    public bool StormProcessed { get; set; } = false;

    /// <summary>
    /// The number of spells cast this turn by any character.
    /// Most likely the caster since we do not have instant speed spells (yet).
    /// </summary>
    public int CastSpellsThisTurn
    {
        get
        {
            return Managers.BattleManager.CastSpellsThisTurn;
        }
    }
}
