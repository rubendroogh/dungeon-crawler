public partial class BrainFreezeStatusEffectBehaviour : BaseStatusEffectBehaviour
{
    public override void ProcessEffectOnApply(Character target)
    {
        target.PhysicalDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, 4f));
        Managers.BattleLogManager.Log($"Applied Brain Freeze to {target.Name}, increasing physical damage taken by 4x.");
    }

    public override void ProcessEffectOnRemove(Character target)
    {
        target.PhysicalDamageModifiers.Remove(new DamageModifier(DamageModifierType.Multiplicative, 4f));
        Managers.BattleLogManager.Log($"Removed Brain Freeze from {target.Name}, restoring normal physical damage taken.");
    }
}
