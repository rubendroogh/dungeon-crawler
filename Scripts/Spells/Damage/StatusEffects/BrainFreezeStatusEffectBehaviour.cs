using Godot;
using System;

public partial class BrainFreezeStatusEffectBehaviour : BaseStatusEffectBehaviour
{
    public override void ProcessEffectOnApply(Character target)
    {
        target.PhysicalDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, 4f));
    }
    
    public override void ProcessEffectOnRemove(Character target)
    {
        target.PhysicalDamageModifiers.Remove(new DamageModifier(DamageModifierType.Multiplicative, 4f));
    }
}
