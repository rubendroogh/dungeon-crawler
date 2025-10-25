using Godot;

/// <summary>
/// BattleLogManager is responsible for managing the battle log in the game.
/// It initializes the battle log UI component and provides methods to log messages.
/// </summary>
public partial class BattleLogManager : Node
{
    /// <summary>
    /// The ComponentExposer that exposes the battle log components.
    /// </summary>
    [Export]
    private ComponentExposer BattleLogExposer;

    /// <summary>
    /// The RichTextLabel that displays the battle log messages.
    /// </summary>
    private RichTextLabel BattleLog => BattleLogExposer.GetComponent<RichTextLabel>(Components.BattleLog);

    /// <summary>
    /// Logs a message to the battle log.
    /// </summary>
    public void Log(string message)
    {
        BattleLog.AppendText(message + "\n");
    }

    /// <summary>
    /// Clears the battle log, removing all logged messages.
    /// </summary>
    public void ClearLog()
    {
        BattleLog.Clear();
    }
}
