using Godot;

/// <summary>
/// ManagerRepository is a singleton that holds references to various managers in the game.
/// It is used to access these managers from anywhere in the game.
/// </summary>
public partial class ManagerRepository : Node
{
    public static ActionManager ActionManager { get; set; }

    public static SpellListManager SpellListManager { get; set; }

    public static BattleLogManager BattleLogManager { get; set; }

    public static BattleManager BattleManager { get; set; }

    public override void _Ready()
    {
        ActionManager = GetNode<ActionManager>("ActionManager");
        SpellListManager = GetNode<SpellListManager>("SpellListManager");
        BattleLogManager = GetNode<BattleLogManager>("BattleLogManager");
        BattleManager = GetNode<BattleManager>("BattleManager");
    }
}
