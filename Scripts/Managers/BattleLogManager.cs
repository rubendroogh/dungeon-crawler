using Godot;
using System;

/// <summary>
/// BattleLogManager is responsible for managing the battle log in the game.
/// </summary>
public partial class BattleLogManager : Node
{
    public override void _Ready()
    {
        base._Ready();
        InitializeBattleLog();
    }

    private RichTextLabel BattleLog { get; set; }

    private void InitializeBattleLog()
    {
        BattleLog = GetNode<RichTextLabel>("../../BattleLogPanelContainer/BattleLog");
    }

    public void AddToLog(string message)
    {
        // Logic to add a message to the battle log
        BattleLog.AppendText(message + "\n");
    }

    public void ClearLog()
    {
        // Logic to clear the battle log
        GD.Print("Battle Log Cleared");
    }
}
