using System.Threading.Tasks;

public partial class PhysicalInvincibilityStatusEffectBehaviour : BaseStatusEffectBehaviour
{
    public override async Task ProcessEffectOnApply(Character target)
    {
        target.PhysicalDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, 0f));
        BattleLogManager.Instance.Log($"Applied Physical Invincibility to {target.Name}, negating all physical damage.");
    }

    public override async Task ProcessEffectOnRemove(Character target)
    {
        var modifier = target.PhysicalDamageModifiers.FindLast(m => m.Type == DamageModifierType.Multiplicative && m.Value == 0f);
        if (modifier != null) target.PhysicalDamageModifiers.Remove(modifier);
        BattleLogManager.Instance.Log($"Removed Physical Invincibility from {target.Name}, restoring normal physical damage taken.");
    }
}
