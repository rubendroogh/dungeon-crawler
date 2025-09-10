using Godot;

public partial class ConfirmSelectionButton : Button
{
    public override void _Ready()
    {
        Pressed += OnPressed;
    }

    /// <summary>
    /// Confirms the selected reward and transitions to a new battle.
    /// </summary>
    private void OnPressed()
    {
        if (Managers.RewardSelectionManager.SelectedReward == null)
        {
            GD.PrintErr("No reward selected.");
            return;
        }

        Managers.SoundEffectManager.PlayButtonClick();
        Managers.RewardSelectionManager.ConfirmSelection();
        Managers.TransitionManager.RewardSelectionToGame();
    }
}
