using System.Collections.Generic;
using System.Threading.Tasks;

public partial class UmbralShieldBehaviour : DefaultSpellBehaviour
{
    public async override Task<ResolveResult> Resolve(List<Blessing> blessings, ActionData spellData, Character target)
    {
        // Apply the physical invincibility to the player for the remainder of the turn.
        await Managers.PlayerManager.GetPlayer().ApplyEffect(StatusEffectType.PhysicalInvincibility, 1);
        return await base.Resolve(blessings, spellData, target);
    }
}
