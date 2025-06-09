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
    /// Used to determine the current phase of the turn (e.g., Start, Main, Damage, PostCombat, End).
    /// Cannot be set directly; it is updated automatically as the turn progresses.
    /// </summary>
    public TurnPhase CurrentTurnPhase { get; private set; } = TurnPhase.Start;
    
    /// <summary>
    /// Indicates whether the turn phase has been processed.
    /// This is used to ensure that the turn phase is processed only once per turn.
    /// </summary>
    private bool TurnPhaseProcessed { get; set; } = false;

    /// <summary>
    /// The current turn number.
    /// </summary>
    private int CurrentTurn { get; set; } = 0;

    /// <summary>
    /// The resource preloader for enemies.
    /// </summary>
    private ResourcePreloader EnemiesPreloader;

    /// <summary>
    /// The label that displays the current turn number and phase.
    /// </summary>
    private Label TurnLabel;

    public override void _Ready()
    {
        EnemiesPreloader = GetNode<ResourcePreloader>("EnemiesPreloader");
        TurnLabel = GetNode<Label>("TurnPanelContainer/TurnLabel");
        if (EnemiesPreloader == null)
        {
            GD.PrintErr("EnemiesPreloader not found in the scene.");
            return;
        }

        CallDeferred(nameof(InitializeBattle));
    }

    public override void _Process(double delta)
    {
        AdvanceTurnFlow();
    }

    /// <summary>
    /// Gets the player character from the scene.
    /// This method is used to access the player character for actions such as casting spells or attacking.
    /// </summary>
    public Player GetPlayer()
    {
        return GetNode<Player>("Player");
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
    }

    /// <summary>
    /// Starts a new turn phase and processes it for the current character.
    /// This method is called automatically after the current turn phase is completed.
    /// </summary>
    public void StartNewTurnPhase()
    {
        CurrentTurnPhase++;
        TurnPhaseProcessed = false;
        TurnLabel.Text = $"Turn {CurrentTurn} - Phase: {CurrentTurnPhase}";
    }

    /// <summary>
    /// Starts a new turn phase from a specific phase.
    /// It only changes the phase if the provided phase is the current phase.
    /// </summary>
    public void StartNewTurnPhaseFrom(TurnPhase phase)
    {
        if (CurrentTurnPhase == phase)
        {
            StartNewTurnPhase();
        }
        else
        {
            GD.PrintErr($"Cannot start new turn phase from {phase}. Current phase is {CurrentTurnPhase}.");
        }
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
    /// Advances the turn flow by processing the current turn phase and then starting the next one.
    /// Sets a flag to ensure turns are not 
    /// </summary>
    private void AdvanceTurnFlow()
    {
        if (TurnPhaseProcessed) return;

        ProcessTurnPhase();
        TurnPhaseProcessed = true;

        // If phase requires immediate progression, handle it here.
        if (ShouldAutoAdvancePhase(CurrentTurnPhase))
        {
            StartNewTurnPhase();
        }
    }

    /// <summary>
    /// Processes the current turn phase for the character whose turn it is.
    /// This contains the logic for each phase of the turn.
    /// </summary>
    private void ProcessTurnPhase()
    {
        var currentCharacter = GetCurrentCharacter();
        switch (CurrentTurnPhase)
        {
            case TurnPhase.Start:
                currentCharacter.StartTurn();
                break;
            case TurnPhase.Main:
                break;
            case TurnPhase.Damage:
                DamagePhase();
                break;
            case TurnPhase.End:
                currentCharacter.EndTurn();
                break;
            case TurnPhase.PostEnd:
                EndTurn();
                break;
        }
    }

    /// <summary>
    /// Processes the damage phase. It executes logic from queued spells or attacks
    /// and makes sure the state is correctly preserved.
    /// </summary>
    private void DamagePhase()
    {
        // var queue = GetCurrentCharacter().ActionQueue;
        var queue = GetPlayer().ActionQueue;
        if (queue.Count == 0)
        {
            GD.Print("No actions in the queue for the current character.");
            return;
        }

        foreach (var actionEntry in queue)
        {
            ManagerRepository.ActionManager.CastSpell(actionEntry);
        }
        
        // Clear the spell queue after processing
        queue.Clear();
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
        // If no character is currently set, return the first character in the dictionary
        var currentCharacter = Characters.FirstOrDefault(c => c.Value).Key ?? Characters.FirstOrDefault().Key;        
        return currentCharacter;
    }

    /// <summary>
    /// Checks if the current phase should auto-advance to the next phase.
    /// </summary>
    private bool ShouldAutoAdvancePhase(TurnPhase phase)
    {
        // Only auto-advance phases that are automatic
        return phase != TurnPhase.Main;
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

        // Set the enemy as the selected target for spell casting
        // TODO We want to add target selection logic later
        ManagerRepository.ActionManager.SelectedTarget = enemy;

        ManagerRepository.BattleLogManager.Log($"{enemyData.Name} encountered!");
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
    /// Attacks are resolved and damage is calculated and applied.
    /// </summary>
    Damage,
    /// <summary>
    /// The post-combat phase of the turn.
    /// Status effects are resolved and characters can heal or use items.
    /// Note: This phase is currently not implemented.
    /// </summary>
    // PostCombat,
    /// <summary>
    /// The end phase of the turn.
    /// Status effects are resolved and the turn ends.
    /// </summary>
    End,
    /// <summary>
    /// The post-end phase of the turn.
    /// Reaching this phase indicates that the turn has ended and the next turn will start.
    /// </summary>
    PostEnd
}