public partial class PhysicalInvincibilityStatusEffectBehaviour : BaseStatusEffectBehaviour
{
    public override void ProcessEffectOnApply(Character target)
    {
        target.PhysicalDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, 0f));
        Managers.BattleLogManager.Log($"Applied Physical Invincibility to {target.Name}, negating all physical damage.");
    }

    public override void ProcessEffectOnRemove(Character target)
    {
        target.PhysicalDamageModifiers.Remove(new DamageModifier(DamageModifierType.Multiplicative, 0f));
        Managers.BattleLogManager.Log($"Removed Physical Invincibility from {target.Name}, restoring normal physical damage taken.");
    }
}
