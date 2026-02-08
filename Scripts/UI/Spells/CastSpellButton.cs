using Godot;

public partial class CastSpellButton : Button
{
    public override void _Ready()
    {
        Pressed += OnPressed;
    }

    public void OnPressed()
    {
        _ = SoundEffectManager.Instance.PlayButtonClick();
        BattleManager.Instance.StartNewTurnPhaseFrom(TurnPhase.Main);
    }
}
