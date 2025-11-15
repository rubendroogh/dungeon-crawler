using Godot;

/// <summary>
/// ManagerRepository is a singleton that holds references to various managers in the game.
/// It is used to access these managers from anywhere in the game.
/// </summary>
public partial class Managers : Node
{
    /// <summary>
    /// ActionManager is responsible for managing actions related to casting spells,
    /// handling selected cards, spells, and targets, and emitting signals for UI updates.
    /// </summary>
    public static ActionManager ActionManager { get; set; }

    /// <summary>
    /// SpellBookManager is responsible for managing the list of spells available in the game.
    /// It handles loading, saving, and retrieving spells, as well as managing the UI for spell selection.
    /// </summary>
    public static SpellBookManager SpellBookManager { get; set; }

    /// <summary>
    /// BattleLogManager is responsible for managing the battle log in the game.
    /// </summary>
    public static BattleLogManager BattleLogManager { get; set; }

    /// <summary>
    /// BattleManager is responsible for managing the battle state,
    /// including the current turn, active characters, and the overall battle flow.
    /// </summary>
    public static BattleManager BattleManager { get; set; }

    /// <summary>
    /// PlayerManager is responsible for managing the player character and related data.
    /// It handles player-specific actions, character data, and interactions with the game world.
    /// </summary>
    public static PlayerManager PlayerManager { get; set; }

    /// <summary>
    /// TransitionManager is responsible for managing transitions between different game states,
    /// such as loading screens, scene transitions, and other visual effects that occur during state changes
    /// </summary>
    public static TransitionManager TransitionManager { get; set; }

    /// <summary>
    /// DebugScreenManager is responsible for showing the correct data in the debug screen.
    /// </summary>
    public static DebugScreenManager DebugScreenManager { get; set; }

    /// <summary>
    /// RewardSelectionManager is responsible for managing the selection of rewards for the player.
    /// </summary>
    public static RewardSelectionManager RewardSelectionManager { get; set; }

    /// <summary>
    /// ManaSourceManager is responsible for managing the deck of blessings used to cast spells.
    /// </summary>
    public static ManaSourceManager ManaSourceManager { get; set; }

    /// <summary>
    /// SoundEffectManager is responsible for managing sound effects in the game.
    /// </summary>
    public static SoundEffectManager SoundEffectManager { get; set; }

    /// <summary>
    /// OpponentManager is responsible for displaying and managing opponents in the game.
    /// </summary>
    public static OpponentManager OpponentManager { get; set; }

    public override void _Ready()
    {
        ActionManager = GetNode<ActionManager>("ActionManager");
        SpellBookManager = GetNode<SpellBookManager>("SpellListManager");
        BattleLogManager = GetNode<BattleLogManager>("BattleLogManager");
        BattleManager = GetNode<BattleManager>("BattleManager");
        PlayerManager = GetNode<PlayerManager>("PlayerManager");
        TransitionManager = GetNode<TransitionManager>("TransitionManager");
        DebugScreenManager = GetNode<DebugScreenManager>("DebugScreenManager");
        SoundEffectManager = GetNode<SoundEffectManager>("SoundEffectManager");
        ManaSourceManager = GetNode<ManaSourceManager>("ManaSourceManager");
        OpponentManager = GetNode<OpponentManager>("OpponentManager");

        // TODO: Find a better way to get this manager
        RewardSelectionManager = GetTree().Root.GetNode<RewardSelectionManager>("Root/UI/RewardSelection/RewardSelectionManager");
    }
}
