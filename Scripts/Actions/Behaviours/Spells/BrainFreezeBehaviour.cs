using System.Collections.Generic;

public partial class BrainFreezeBehaviour : DefaultSpellBehaviour
{
    public override ResolveResult Resolve(List<Blessing> blessings, ActionData spellData, Character target)
    {
        // Check if the target is frozen
        if (target.HasEffect(StatusEffectType.Frozen))
        {
            // If the target is frozen, apply the BrainFreeze effect for 1 turn
            target.ClearEffect(StatusEffectType.Frozen);
            target.ApplyEffect(StatusEffectType.BrainFreeze, 1);
        }
        else
        {
            // If the target is not frozen, apply the frozen effect
            target.ApplyEffect(StatusEffectType.Frozen, 2);
        }

        return base.Resolve(blessings, spellData, target);
    }
}
