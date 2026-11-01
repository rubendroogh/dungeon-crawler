using System.Threading.Tasks;

public partial class FrozenStatusEffectBehaviour : BaseStatusEffectBehaviour
{
    public override async Task ProcessEffectOnApply(Character target)
    {
        target.PhysicalDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, 2f));
        BattleLogManager.Instance.Log($"{target.Name} is frozen and takes double physical damage.");
    }

    public override async Task ProcessEffectOnRemove(Character target)
    {
        var modifier = target.PhysicalDamageModifiers.FindLast(m => m.Type == DamageModifierType.Multiplicative && m.Value == 2f);
        if (modifier != null) target.PhysicalDamageModifiers.Remove(modifier);
        BattleLogManager.Instance.Log($"{target.Name} is no longer frozen and physical damage modifiers are removed.");
    }
}
