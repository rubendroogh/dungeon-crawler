using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// BattleManager is responsible for managing the battle state and related variables.
/// </summary>
public partial class BattleManager : Node
{
    [Export]
    public PackedScene EnemyScene { get; set; }

    /// <summary>
    /// The characters involved in the battle.
    /// The key is the character, and the value is a boolean indicating if it's the character's turn.
    /// </summary>
    public Dictionary<Character, bool> Characters { get; set; }

    /// <summary>
    /// The resource preloader for enemies.
    /// </summary>
    private ResourcePreloader EnemiesPreloader;

    /// <summary>
    /// The current turn number.
    /// </summary>
    private int CurrentTurn { get; set; } = 0;

    /// <summary>
    /// Used to determine the current phase of the turn (e.g., Start, Main, Damage, PostCombat, End).
    /// </summary>
    private TurnPhase CurrentTurnPhase { get; set; } = TurnPhase.Start;

    public override void _Ready()
    {
        EnemiesPreloader = GetNode<ResourcePreloader>("EnemiesPreloader");
        if (EnemiesPreloader == null)
        {
            GD.PrintErr("EnemiesPreloader not found in the scene.");
            return;
        }

        CallDeferred(nameof(InitializeBattle));
    }

    /// <summary>
    /// Initializes the battle by setting up the characters and spawning a random enemy.
    /// The battle starts with the player's turn.
    /// </summary>
    private void InitializeBattle()
    {
        Characters = new Dictionary<Character, bool>
        {
            { GetNode<Character>("Player"), true }
        };
        SpawnRandomEnemy();

        StartNewTurn();
    }

    /// <summary>
    /// Starts a new turn in the battle.
    /// This method is called when the current turn ends and a new turn begins.
    /// </summary>
    /// <remarks>
    /// If called manually, it will start a new turn for the current character, skipping uncompleted phases.
    /// </remarks>
    public void StartNewTurn()
    {
        CurrentTurn++;
        CurrentTurnPhase = TurnPhase.Start;

        // Get the character whose turn it is
        var currentCharacter = GetCurrentCharacter();
        if (currentCharacter == null)
        {
            GD.PrintErr("No current character found.");
            return;
        }

        ProcessTurnPhase();
    }

    /// <summary>
    /// Processes the current turn phase for the character whose turn it is.
    /// </summary>
    private void ProcessTurnPhase()
    {
        var currentCharacter = GetCurrentCharacter();
        switch (CurrentTurnPhase)
        {
            case TurnPhase.Start:
                currentCharacter.StartTurn();
                break;
            case TurnPhase.End:
                currentCharacter.EndTurn();
                break;
        }

        StartNewTurnPhase();
    }

    /// <summary>
    /// Starts a new turn phase.
    /// This method is called at the end of each turn phase.
    /// </summary>
    private void StartNewTurnPhase()
    {
        CurrentTurnPhase++;
        if (CurrentTurnPhase > TurnPhase.End)
        {
            EndTurn();
        }
    }

    /// <summary>
    /// This method is called when the current character's turn ends.
    /// It resets the turn phase and moves to the next character's turn.
    /// </summary>
    private void EndTurn()
    {
        MoveToNextCharacter();
        StartNewTurn();
    }

    /// <summary>
    /// Gets the current character whose turn it is.
    /// </summary>
    private Character GetCurrentCharacter()
    {
        return Characters.FirstOrDefault(c => c.Value).Key;
    }

    /// <summary>
    /// Sets the current character to the next character in the turn order.
    /// This method is called when the current character's turn ends.
    /// </summary>
    private void MoveToNextCharacter()
    {
        // Move to the next character's turn
        var currentCharacter = GetCurrentCharacter();
        if (currentCharacter == null)
        {
            GD.PrintErr("No current character found.");
            return;
        }

        Characters[currentCharacter] = false;

        // Find the next character
        var nextCharacter = Characters.Keys.SkipWhile(c => c != currentCharacter).Skip(1).FirstOrDefault();
        if (nextCharacter == null)
        {
            // If no next character, loop back to the first character
            nextCharacter = Characters.Keys.FirstOrDefault();
        }

        Characters[nextCharacter] = true;
    }

    /// <summary>
    /// Spawns a random enemy from the preloaded resources.
    /// </summary>
    private void SpawnRandomEnemy()
    {
        var resourceList = EnemiesPreloader.GetResourceList();
        var enemyResource = resourceList[GD.Randi() % resourceList.Length];

        CharacterData enemyData = EnemiesPreloader.GetResource(enemyResource) as CharacterData;
        if (enemyData == null)
        {
            GD.PrintErr("Failed to load enemy data.");
            return;
        }

        SpawnEnemy(enemyData);
    }

    /// <summary>
    /// Spawns an enemy in the battle.
    /// </summary>
    /// <param name="enemyData">The character data.</param>
    private void SpawnEnemy(CharacterData enemyData)
    {
        var enemy = EnemyScene.Instantiate<Character>();
        if (enemy == null)
        {
            GD.PrintErr("Failed to instantiate enemy scene.");
            return;
        }
        
        enemy.Setup(enemyData);
        enemy.Name = enemyData.Name;
        
        // Add enemy at root of the scene
        GetTree().Root.GetNode("Root").AddChild(enemy);

        Characters.Add(enemy, false);

        ManagerRepository.BattleLogManager.AddToLog($"{enemyData.Name} encountered!");
    }
}

/// <summary>
/// Enumeration representing the different phases of a turn.
/// </summary>
public enum TurnPhase
{
    /// <summary>
    /// The start phase of the turn.
    /// Status effects are triggered and mana is regenerated.
    /// </summary>
    Start,
    /// <summary>
    /// The main phase of the turn.
    /// Characters can queue spells and attacks.
    /// </summary>
    Main,
    /// <summary>
    /// The damage phase of the turn.
    /// Attacks are resolved and damage is calculated.
    /// </summary>
    Damage,
    /// <summary>
    /// The post-combat phase of the turn.
    /// Status effects are resolved and characters can heal or use items.
    /// </summary>
    PostCombat,
    /// <summary>
    /// The end phase of the turn.
    /// Status effects are resolved and the turn ends.
    /// </summary>
    End
}