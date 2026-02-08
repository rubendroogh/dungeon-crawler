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
        target.PhysicalDamageModifiers.Remove(new DamageModifier(DamageModifierType.Multiplicative, 4f));
        BattleLogManager.Instance.Log($"Removed Solidified from {target.Name}, restoring normal physical damage taken.");
    }
}
