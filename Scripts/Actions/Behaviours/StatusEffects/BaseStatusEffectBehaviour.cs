using System.Threading.Tasks;

public abstract class BaseStatusEffectBehaviour : IStatusEffectBehaviour
{
    public virtual async Task ProcessEffectOnApply(Character target)
    {
        // Process the effect on the target character when it is applied
        // This method should be overridden in derived classes to implement specific effects
    }

    public virtual async Task ProcessEffectOnRemove(Character target)
    {
        // Process the effect on the target character when it is removed
        // This method should be overridden in derived classes to implement specific effects
    }

    public virtual async Task ProcessEffectStartOpponentTurn(Character target)
    {
        // Process the effect on the target character
        // This method should be overridden in derived classes to implement specific effects
    }

    public virtual async Task ProcessEffectStartDamage(Character target)
    {
        // Process the effect on the target character
        // This method should be overridden in derived classes to implement specific effects
    }

    public virtual async Task ProcessEffectEndOpponentTurn(Character target)
    {
        // Process the effect on the target character
        // This method should be overridden in derived classes to implement specific effects
    }

    public virtual async Task ProcessEffectStartTurn(Character target)
    {
        // Process the effect on the target character
        // This method should be overridden in derived classes to implement specific effects
    }

    public virtual async Task ProcessEffectEndTurn(Character target)
    {
        // Process the effect on the target character
        // This method should be overridden in derived classes to implement specific effects
    }
}
