using Godot;

public partial class CastSpellButton : Button
{
    public override void _Ready()
    {
        Pressed += OnPressed;
    }

    public void OnPressed()
    {
        var target = ManagerRepository.BattleManager.TargetEnemy;
        ManagerRepository.SpellCastingManager.CastSpell(target);
    }
}
