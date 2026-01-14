using System.Threading.Tasks;

public partial class PlagueStatusEffectBehaviour : BaseStatusEffectBehaviour
{
    public override async Task ProcessEffectEndTurn(Character target)
    {
        // Do damage
        await target.Delay(800);
        
    }
}
