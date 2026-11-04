using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Grants the caster resource-defined block through the normal spell resolution pipeline.
/// </summary>
public partial class BlockSpellBehaviour : DefaultSpellBehaviour
{
    public override async Task<ResolveResult> Resolve(List<Blessing> blessings, ActionData spellData, Character target)
    {
        var caster = ActionManager.Instance.CastingContext.Caster;
        if (caster == null || caster.IsDead || blessings == null || blessings.Count == 0)
        {
            return new ResolveResult();
        }

        var result = await base.Resolve(blessings, spellData, caster);
        result.Block = spellData.BaseBlock;
        return result;
    }
}
