using Godot;

public partial class CastSpellButton : Button
{
    public override void _Ready()
    {
        Pressed += OnPressed;
    }

    public void OnPressed()
    {
        Managers.SoundEffectManager.PlayButtonClick();
        Managers.BattleManager.StartNewTurnPhaseFrom(TurnPhase.Main);
    }
}
