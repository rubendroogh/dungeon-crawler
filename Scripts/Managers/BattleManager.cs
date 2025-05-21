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
    /// The key is the character, and the value is a boolean indicating if the character is the player.
    /// </summary>
    public Dictionary<Character, bool> Characters { get; set; }

    private ResourcePreloader EnemiesPreloader;

    private int CurrentTurn { get; set; } = 0;

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

    private void InitializeBattle()
    {
        SpawnRandomEnemy();
    }

    public void StartBattle()
    {
        // Logic to start the battle
        GD.Print("Battle Started");
    }

    public void EndBattle()
    {
        // Logic to end the battle
        GD.Print("Battle Ended");
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
