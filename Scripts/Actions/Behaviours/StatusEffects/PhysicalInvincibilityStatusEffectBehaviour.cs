using System.Threading.Tasks;

public partial class PhysicalInvincibilityStatusEffectBehaviour : BaseStatusEffectBehaviour
{
    public override async Task ProcessEffectOnApply(Character target)
    {
        target.PhysicalDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, 0f));
        Managers.BattleLogManager.Log($"Applied Physical Invincibility to {target.Name}, negating all physical damage.");
    }

    public override async Task ProcessEffectOnRemove(Character target)
    {
        target.PhysicalDamageModifiers.Remove(new DamageModifier(DamageModifierType.Multiplicative, 0f));
        Managers.BattleLogManager.Log($"Removed Physical Invincibility from {target.Name}, restoring normal physical damage taken.");
    }
}
