using System.Threading.Tasks;

public partial class SolidifiedStatusEffectBehaviour : BaseStatusEffectBehaviour
{
    public override async Task ProcessEffectOnApply(Character target)
    {
        target.PhysicalDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, 4f));
        BattleLogManager.Instance.Log($"Applied Solidified to {target.Name}, increasing physical damage taken by 4x.");
    }

    public override async Task ProcessEffectOnRemove(Character target)
    {
        var modifier = target.PhysicalDamageModifiers.FindLast(m => m.Type == DamageModifierType.Multiplicative && m.Value == 4f);
        if (modifier != null) target.PhysicalDamageModifiers.Remove(modifier);
        BattleLogManager.Instance.Log($"Removed Solidified from {target.Name}, restoring normal physical damage taken.");
    }
}
