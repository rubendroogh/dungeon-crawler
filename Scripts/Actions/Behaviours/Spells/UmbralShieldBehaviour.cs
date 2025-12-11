using Godot;
using System;

public partial class UmbralShieldBehaviour : BaseStatusEffectBehaviour
{
    public override void ProcessEffectOnApply(Character target)
    {
        // Apply the physical invincibility to the player for the remainder of the turn.
        Managers.PlayerManager.GetPlayer().ApplyEffect(StatusEffectType.PhysicalInvincibility, 1);
    }
}
