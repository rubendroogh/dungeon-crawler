using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// BattleManager is responsible for managing the battle state and related variables.
/// It handles the turn phases, character turns, and spawning of enemies.
/// It also manages the flow of the battle, including starting new turns and processing actions.
/// </summary>
public partial class BattleManager : Node
{
    /// <summary>
    /// The characters involved in the battle.
    /// The key is the character, and the value is a boolean indicating if it's the character's turn.
    /// </summary>
    public Dictionary<Character, bool> Characters { get; set; }

    /// <summary>
    /// Used to determine the current phase of the turn (e.g., Start, Main, Damage, End).
    /// Cannot be set directly; it is updated automatically as the turn progresses.
    /// </summary>
    public TurnPhase CurrentTurnPhase { get; private set; } = TurnPhase.Start;

    /// <summary>
    /// The number of spells cast this turn by any character. The spells must be fully resolved.
    /// </summary>
    public int CastSpellsThisTurn { get; set; } = 0;

    /// <summary>
    /// The scene to instantiate for enemies. It contains the enemy character data, sprite, and health bar.
    /// </summary>
    [Export]
    private PackedScene EnemyScene { get; set; }

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
    /// The ComponentExposer that exposes the turn indication components.
    /// </summary>
    [Export]
    private ComponentExposer TurnIndicationExposer;

    /// <summary>
    /// The label that displays the current turn number and phase.
    /// </summary>
    private Label TurnLabel => TurnIndicationExposer.GetComponent<Label>(Components.TurnLabel);

    /// <summary>
    /// Indicates whether the battle has been initialized.
    /// </summary>
    private bool IsBattleInitialized { get; set; } = false;

    /// <summary>
    /// Starts a new turn phase and processes it for the current character.
    /// This method is called automatically after the current turn phase is completed.
    /// </summary>
    public async Task StartNewTurnPhase()
    {
        CurrentTurnPhase++;
        if (CurrentTurnPhase > TurnPhase.PostEnd)
        {
            CurrentTurnPhase = TurnPhase.Start;
        }

        await this.Delay(300);
        UpdateTurnLabel();
    }

    /// <summary>
    /// Only starts a new turn phase if the provided phase is the current phase.
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
    public void InitializeBattle()
    {
        Characters = new Dictionary<Character, bool>
        {
            { Managers.PlayerManager.GetPlayer(), true }
        };
        Managers.OpponentManager.SpawnRandomOpponent();
        IsBattleInitialized = true;

        // Kick off the battle loop
        _ = RunBattleLoop();
    }

    /// <summary>
    /// Runs the main battle loop, processing turns until the battle is over.
    /// </summary>
    /// <returns></returns>
    private async Task RunBattleLoop()
    {
        while (IsBattleInitialized)
        {
            TurnPhaseProcessed = false;
            await AdvanceTurnFlow();

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }

    /// <summary>
    /// Advances the turn flow by processing the current turn phase and then starting the next one.
    /// Sets a flag to ensure turns are not processed multiple times.
    /// </summary>
    private async Task AdvanceTurnFlow()
    {
        try
        {
            await ProcessTurnPhase();
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"Error processing turn phase {CurrentTurnPhase}: {ex.Message}");
        }
        
        TurnPhaseProcessed = true;

        // If phase requires immediate progression, handle it here.
        if (ShouldAutoAdvancePhase(CurrentTurnPhase))
        {
            await StartNewTurnPhase();
        }
    }

    /// <summary>
    /// Processes the current turn phase for the character whose turn it is.
    /// This contains the logic for each phase of the turn.
    /// </summary>
    /// <remarks>
    /// Never call StartNewTurnPhase from this method!
    /// </remarks>
    private async Task ProcessTurnPhase()
    {
        // Determine the current character based on the turn order.
        // Determine the character type, and process the turn accordingly.
        var currentCharacter = GetCurrentCharacter();
        if (currentCharacter == null)
        {
            GD.PrintErr("No current character found. Cannot process turn phase.");
            return;
        }

        switch (CurrentTurnPhase)
        {
            case TurnPhase.Start:
                StartPhase();
                break;
            case TurnPhase.Main:
                MainPhase();
                break;
            case TurnPhase.Damage:
                await DamagePhase();
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
    /// Processes the start phase of the turn.
    /// Increases turn counter, resets mana charges and triggers relevant status effects.
    /// </summary>
    private void StartPhase()
    {
        CurrentTurn++;
        // Managers.ActionManager.ResetCards();

        var currentCharacter = GetCurrentCharacter();
        currentCharacter.StartTurn();
    }

    /// <summary>
    /// Processes the main phase of the turn.
    /// In this phase, characters can queue spells and attacks.
    /// If the current character is not a player, the AI will choose an action.
    /// </summary>
    private void MainPhase()
    {
        var currentCharacter = GetCurrentCharacter();
        if (currentCharacter is not Player player)
        {
            var enemy = currentCharacter as Enemy;
            // Choose the player as the target for the enemy's action.
            // TODO Implement target selection logic for enemies.
            enemy.ChooseAction(Characters.Keys.Where(c => c != enemy).ToList());
        }
    }

    /// <summary>
    /// Processes the damage phase. It executes logic from queued spells or attacks
    /// and makes sure the state is correctly preserved.
    /// </summary>
    private async Task DamagePhase()
    {
        // Get the current character and their action queue.
        var currentCharacter = GetCurrentCharacter();
        if (currentCharacter == null)
        {
            GD.PrintErr("No current character found. Cannot process damage phase.");
            return;
        }

        await currentCharacter.ResolveQueue();
    }

    /// <summary>
    /// This method is called when the current character's turn ends.
    /// It moves to the next character's turn.
    /// </summary>
    private void EndTurn()
    {
        // If all non-player characters are dead, the player is victorious
        if (Characters.All(c => c.Key.IsDead == !c.Key.IsPlayer))
        {
            EndBattleVictory();
            return;
        }

        // TODO: Handle player death
        CastSpellsThisTurn = 0;
        MoveToNextCharacter();
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
        var isMain = phase == TurnPhase.Main;
        var isPlayer = GetCurrentCharacter() == Managers.PlayerManager.GetPlayer();

        return !(isMain && isPlayer);
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
    /// Handles the end of the battle when the player is victorious by transitioning to the rewards selection.
    /// </summary>
    private void EndBattleVictory()
    {
        Managers.TransitionManager.ToRewardSelection();
        IsBattleInitialized = false;
    }

    /// <summary>
    /// Updates the turn label to reflect the player's turn number and phase.
    /// </summary>
    private void UpdateTurnLabel()
    {
        float displayTurnCount = (float)CurrentTurn / (float)Characters.Count;

        // Round up
        displayTurnCount = Mathf.Ceil(displayTurnCount);
        TurnLabel.Text = $"Turn {displayTurnCount}, {CurrentTurnPhase} Phase";
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