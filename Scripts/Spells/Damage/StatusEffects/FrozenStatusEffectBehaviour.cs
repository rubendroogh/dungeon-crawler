public partial class FrozenStatusEffectBehaviour : BaseStatusEffectBehaviour
{
    public override void ProcessEffectStartOpponentTurn(Character target)
    {
        target.CurrentPhysicalDamageMultiplier *= 2;
    }

    public override void ProcessEffectEndOpponentTurn(Character target)
    {
        target.CurrentPhysicalDamageMultiplier /= 2;
    }
}
