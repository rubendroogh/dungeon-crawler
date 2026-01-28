using System.Collections.Generic;
using System.Threading.Tasks;

public partial class SolidifyBehaviour : DefaultSpellBehaviour
{
    public override async Task<ResolveResult> Resolve(List<Blessing> blessings, ActionData spellData, Character target)
    {
        // Check if the target is frozen
        if (target.HasEffect(StatusEffectType.Frozen))
        {
            // If the target is frozen, apply the Solidified effect for 1 turn
            await target.ClearEffect(StatusEffectType.Frozen);
            await target.ApplyEffect(StatusEffectType.Solidified, 1);
        }
        else
        {
            // If the target is not frozen, apply the frozen effect
            await target.ApplyEffect(StatusEffectType.Frozen, 2);
        }

        return await base.Resolve(blessings, spellData, target);
    }
}
