using Godot;

public partial class CastSpellButton : Button
{
    public override void _Ready()
    {
        Pressed += OnPressed;
    }

    public void OnPressed()
    {
        if (ManagerRepository.BattleManager.CurrentTurnPhase != TurnPhase.Main)
        {
            GD.PrintErr("Cannot cast a spell outside of the Main phase.");
            return;
        }

        ManagerRepository.BattleManager.StartNewTurnPhase();
    }
}
