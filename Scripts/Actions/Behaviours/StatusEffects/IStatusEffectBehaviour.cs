using System.Threading.Tasks;

public interface IStatusEffectBehaviour
{
    /// <summary>
    /// Process the effect on the target character when the effect is applied.
    /// This is called when the effect is first applied to the character.
    /// </summary>
    /// <param name="target"></param>
    public Task ProcessEffectOnApply(Character target);

    /// <summary>
    /// Process the effect on the target character when the effect is removed.
    /// This is called when the effect is removed from the character, either due to expiration or removal.
    /// </summary>
    /// <param name="target"></param>
    public Task ProcessEffectOnRemove(Character target);
    
    /// <summary>
    /// Process the effect on the target character on the opponent's turn.
    /// </summary>
    /// <param name="target"></param>
    public Task ProcessEffectStartOpponentTurn(Character target);

    /// <summary>
    /// Process the effect on the target character when damage is applied to it.
    /// </summary>
    /// <param name="target"></param>
    public Task ProcessEffectStartDamage(Character target);

    /// <summary>
    /// Process the effect on the target character when the opponent's turn ends.
    /// </summary>
    /// <param name="target"></param>
    public Task ProcessEffectEndOpponentTurn(Character target);

    /// <summary>
    /// Process the effect on the target character when its turn starts.
    /// </summary>
    /// <param name="target"></param>
    public Task ProcessEffectStartTurn(Character target);

    /// <summary>
    /// Process the effect on the target character when its turn ends.
    /// </summary>
    /// <param name="target"></param>
    public Task ProcessEffectEndTurn(Character target);
}
