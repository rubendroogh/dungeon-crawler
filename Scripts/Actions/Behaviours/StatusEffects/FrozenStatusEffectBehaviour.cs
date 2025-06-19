public partial class FrozenStatusEffectBehaviour : BaseStatusEffectBehaviour
{
    public override void ProcessEffectOnApply(Character target)
    {
        target.PhysicalDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, 2f));
        Managers.BattleLogManager.Log($"{target.Name} is frozen and takes double physical damage.");
    }

    public override void ProcessEffectOnRemove(Character target)
    {
        target.PhysicalDamageModifiers.Remove(new DamageModifier(DamageModifierType.Multiplicative, 2f));
        Managers.BattleLogManager.Log($"{target.Name} is no longer frozen and physical damage modifiers are removed.");
    }
}
