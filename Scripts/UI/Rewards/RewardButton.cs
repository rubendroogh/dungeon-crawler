using Godot;

public partial class RewardButton : Button
{
    public override void _Ready()
    {
        Pressed += OnPressed;
    }
    
    public void OnPressed()
    {
        var reward = GetChild<RewardUI>(0)?.Reward;
        if (reward != null)
        {
            Managers.RewardSelectionManager.SetSelectedReward(reward);
        }
    }
}
