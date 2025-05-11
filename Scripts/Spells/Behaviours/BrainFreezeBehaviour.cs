using System.Collections.Generic;

public partial class BrainFreezeBehaviour : DefaultSpellBehaviour
{
    public override SpellCastResult Cast(List<Card> cards, SpellData spellData, List<Character> targets)
    {
        // Check if the target is frozen
        foreach (var target in targets)
        {
            if (target.HasEffect(StatusEffectType.Frozen))
            {
                // If the target is frozen, apply the BrainFreeze effect
                target.ClearEffect(StatusEffectType.Frozen);
                target.ApplyEffect(StatusEffectType.BrainFreeze, 3);
            }
            else
            {
                // If the target is not frozen, apply the frozen effect
                target.ApplyEffect(StatusEffectType.Frozen, 2);
            }
        }

        return base.Cast(cards, spellData, targets);
    }
}
