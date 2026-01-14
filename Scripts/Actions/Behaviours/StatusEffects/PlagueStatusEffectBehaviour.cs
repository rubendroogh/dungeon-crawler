using System.Threading.Tasks;
using Godot;

public partial class PlagueStatusEffectBehaviour : BaseStatusEffectBehaviour
{
    public override async Task ProcessEffectEndTurn(Character target)
    {
        // TODO: Add animation
        await target.Delay(500);
        var damage = new Damage(5, DamageType.Disease);
        await target.Damage(damage);
        await target.Delay(500);
    }
}
