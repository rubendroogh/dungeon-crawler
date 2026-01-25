using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class DiscombobulateBehaviour : DefaultSpellBehaviour
{
    public override async Task<ResolveResult> Resolve(List<Blessing> blessings, ActionData spellData, Character target)
    {
        // Hits 3-6 times
        var hitCount = GD.Randi() % 5 + 3;
        List<Damage> damages = [];

        for (int i = 0; i < hitCount; i++)
        {
            GD.Print("poopy");
            foreach (var damageType in spellData.DamageTypes)
            {
                float damage = CalculateDamage(damageType, 1f, spellData);
                var damagePacket = new Damage(damage, damageType);
                await target.Damage(damagePacket);
                damages.Add(damagePacket);
            }
        }

        return new ResolveResult
        {
            HasResolved = true,
            Damages = damages,
        };
    }
}
