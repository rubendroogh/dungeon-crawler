using Godot;

public partial class CastSpellButton : Button
{
    public override void _Ready()
    {
        Pressed += OnPressed;
    }

    public void OnPressed()
    {
        ManagerRepository.BattleManager.StartNewTurnPhaseFrom(TurnPhase.Main);
    }
}
