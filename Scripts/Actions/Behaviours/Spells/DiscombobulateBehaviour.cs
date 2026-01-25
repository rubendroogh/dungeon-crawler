using System.Collections.Generic;
using Godot;

public partial class DiscombobulateBehaviour : DefaultSpellBehaviour
{
    public override ResolveResult Resolve(List<Blessing> blessings, ActionData spellData, Character target)
    {
        // Hits 2-5 times
        var hitCount = GD.Randi() % 4 + 2;
        
    }
}
