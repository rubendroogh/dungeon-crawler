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

    public override void _Process(double delta)
    {
        Disabled = BattleManager.Instance == null || !BattleManager.Instance.CanQueueSpells;
        if (!Disabled) Text = PlayerManager.Instance.GetPlayer().ActionQueue.Count == 0 ? "Pass turn" : "Cast queue";
    }
}
