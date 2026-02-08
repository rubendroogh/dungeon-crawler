using Godot;
using System;

/// <summary>
/// Manages display and behavior of opponents in the game.
/// </summary>
public partial class OpponentManager : Node
{
    public static OpponentManager Instance { get; private set; }

    /// <summary>
    /// The ComponentExposer that exposes the opponent components.
    /// </summary>
    [Export]
    private ComponentExposer OpponentExposer;

    /// <summary>
    /// The PackedScene for the opponent character.
    /// </summary>
    [Export]
    private PackedScene OpponentScene;

    /// <summary>
    /// The ResourcePreloader containing character data of possible opponents.
    /// </summary>
    [Export]
    private ResourcePreloader OpponentsPreloader;

    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary>
    /// Spawns a random opponent from the preloaded resources.
    /// </summary>
    public void SpawnRandomOpponent()
    {
        var resourceList = OpponentsPreloader.GetResourceList();
        var opponentResource = resourceList[GD.Randi() % resourceList.Length];

        CharacterData opponentData = OpponentsPreloader.GetResource(opponentResource) as CharacterData;
        if (opponentData == null)
        {
            GD.PrintErr("Failed to load opponent data.");
            return;
        }

        SpawnOpponent(opponentData);
    }

    /// <summary>
    /// Spawns an opponent in the battle.
    /// </summary>
    /// <param name="opponentData">The character data.</param>
    public void SpawnOpponent(CharacterData opponentData)
    {
        var opponentScene = OpponentScene.Instantiate<ComponentExposer>();
        var opponent = opponentScene.GetComponent<Enemy>(Components.CharacterData);
        if (opponent == null)
        {
            GD.PrintErr("Failed to instantiate opponent scene.");
            return;
        }

        _ = opponent.Setup(opponentData);
        opponent.Name = opponentData.Name;

        // Add opponent at root of the opponent scene
        var opponentRoot = OpponentExposer.GetComponent<Node>(Components.Opponents);
        opponentRoot.AddChild(opponentScene);
        BattleManager.Instance.Characters.Add(opponent, false);

        // Set the opponent as the selected target for spell casting
        // TODO We want to add target selection logic later
        ActionManager.Instance.SelectedTarget = opponent;

        BattleLogManager.Instance.Log($"{opponentData.Name} encountered!");
    }
}
