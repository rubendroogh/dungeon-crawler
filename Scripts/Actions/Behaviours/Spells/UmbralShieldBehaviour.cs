using System.Collections.Generic;

public partial class UmbralShieldBehaviour : DefaultSpellBehaviour
{
    public override ResolveResult Resolve(List<Blessing> blessings, ActionData spellData, Character target)
    {
        // Apply the physical invincibility to the player for the remainder of the turn.
        Managers.PlayerManager.GetPlayer().ApplyEffect(StatusEffectType.PhysicalInvincibility, 1);
        return base.Resolve(blessings, spellData, target);
    }
}
