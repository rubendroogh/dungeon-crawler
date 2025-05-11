public partial class FrozenStatusEffectBehaviour : BaseStatusEffectBehaviour
{
    public override void ProcessEffectStartDamage(Character target)
    {
        target.CurrentPhysicalDamageMultiplier *= 2;
    }

    public override void ProcessEffectEndOpponentTurn(Character target)
    {
        target.CurrentPhysicalDamageMultiplier = target.CharacterData.BasePhysicalDamageMultiplier;
    }
}
