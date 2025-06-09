public abstract class BaseStatusEffectBehaviour : IStatusEffectBehaviour
{
    public virtual void ProcessEffectOnApply(Character target)
    {
        // Process the effect on the target character when it is applied
        // This method should be overridden in derived classes to implement specific effects
    }

    public virtual void ProcessEffectOnRemove(Character target)
    {
        // Process the effect on the target character when it is removed
        // This method should be overridden in derived classes to implement specific effects
    }

    public virtual void ProcessEffectStartOpponentTurn(Character target)
    {
        // Process the effect on the target character
        // This method should be overridden in derived classes to implement specific effects
    }

    public virtual void ProcessEffectStartDamage(Character target)
    {
        // Process the effect on the target character
        // This method should be overridden in derived classes to implement specific effects
    }

    public virtual void ProcessEffectEndOpponentTurn(Character target)
    {
        // Process the effect on the target character
        // This method should be overridden in derived classes to implement specific effects
    }

    public virtual void ProcessEffectStartTurn(Character target)
    {
        // Process the effect on the target character
        // This method should be overridden in derived classes to implement specific effects
    }

    public virtual void ProcessEffectEndTurn(Character target)
    {
        // Process the effect on the target character
        // This method should be overridden in derived classes to implement specific effects
    }
}
