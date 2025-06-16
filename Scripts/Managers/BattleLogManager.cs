using Godot;
using System;

/// <summary>
/// BattleLogManager is responsible for managing the battle log in the game.
/// It initializes the battle log UI component and provides methods to log messages
/// </summary>
public partial class BattleLogManager : Node
{
    public override void _Ready()
    {
        base._Ready();
        InitializeBattleLog();
    }

    private RichTextLabel BattleLog { get; set; }

    /// <summary>
    /// Initializes the battle log by retrieving the RichTextLabel node from the scene tree.
    /// </summary>
    private void InitializeBattleLog()
    {
        BattleLog = GetNode<RichTextLabel>("../../BattleLogPanelContainer/MarginContainer/BattleLog");
    }

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
