using Godot;
using System;

public partial class FrozenStatusEffect : BaseStatusEffect
{
    public override void ProcessEffect(Character target)
    {
        // Make the target's physical damage multiplier 2
        target.CharacterData.BasePhysicalDamageMultiplier = 2;
    }
}
