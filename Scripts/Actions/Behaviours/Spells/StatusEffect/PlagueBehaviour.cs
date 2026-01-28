using System.Collections.Generic;
using System.Threading.Tasks;

public partial class PlagueBehaviour : DefaultSpellBehaviour
{
    public async override Task<ResolveResult> Resolve(List<Blessing> blessings, ActionData spellData, Character target)
    {
        // Apply the plague status effect to the target
        await target.ApplyEffect(StatusEffectType.Plague, 6);
        return await base.Resolve(blessings, spellData, target);
    }
}
