using Godot;
using System.Threading.Tasks;

public partial class CruelKeyword : KeywordBase
{
    public async override Task OnCast()
    {
        // Get the target of the spell from the action context
        var context = Managers.ActionManager.CastingContext;

        var target = context.Target;
        if (target == null)
        {
            GD.PrintErr("CruelKeyword: No target found in action context.");
            return;
        }
        
        // If the target is below half health, apply a damage modifier to double the damage
        if (target.Health < target.CharacterData.MaxHealth / 2)
        {
            context.DamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, 2f));
        }
    } 
}
