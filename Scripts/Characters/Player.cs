using Godot;

public partial class Player : Character
{
    /// <summary>
    /// The player's name.
    /// </summary>
    [Export]
    public string PlayerName { get; set; } = "Player";

    public override void _Ready()
    {
        base._Ready();
    }

    protected override void UpdateStatusEffectLabel()
    {
        // Keep this empty for now, as the player does not have a status effect label.
        // This method is overridden to prevent the base class from trying to update a non-existent label.
        // We can implement a status effect label for the player later in the UI.
    }
}
