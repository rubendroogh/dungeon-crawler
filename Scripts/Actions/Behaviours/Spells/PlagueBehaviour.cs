using System.Collections.Generic;

public partial class PlagueBehaviour : DefaultSpellBehaviour
{
    public override ResolveResult Resolve(List<Blessing> blessings, ActionData spellData, Character target)
    {
        // Apply the plague status effect to the target
        target.ApplyEffect(StatusEffectType.Plague, 6);
        return base.Resolve(blessings, spellData, target);
    }
}
