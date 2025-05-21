using System.Linq;
using Godot;

public partial class CastSpellButton : Button
{
    public override void _Ready()
    {
        Pressed += OnPressed;
    }

    public void OnPressed()
    {
        // Get the first character that is not the player
        // TODO: This is a placeholder logic.
        var target = ManagerRepository.BattleManager.Characters.FirstOrDefault(x => x.Value == false).Key;
        ManagerRepository.SpellCastingManager.CastSpell(target);
    }
}
