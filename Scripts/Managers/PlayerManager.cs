using Godot;

/// <summary>
/// PlayerManager is a singleton that manages the player character in the game.
/// </summary>
public partial class PlayerManager : Node
{
    /// <summary>
    /// The player character instance that holds the character data.
    /// This character is used in battles and represents the player in the game world.
    /// </summary>
    private Character PlayerCharacter { get; set; }

    public override void _Ready()
    {
        // Initialize the player character
        PlayerCharacter = GetNode<Character>("Player");
        if (PlayerCharacter == null)
        {
            GD.PrintErr("Player character not found in the scene.");
        }
    }

    /// <summary>
    /// Gets the player character from the scene.
    /// This method is used to access the player character for actions such as casting spells or attacking.
    /// </summary>
    public Player GetPlayer()
    {
        return PlayerCharacter as Player;
    }

    /// <summary>
    /// Sets the player character data.
    /// </summary>
    public void SetPlayerData(CharacterData characterData)
    {
        if (PlayerCharacter != null)
        {
            PlayerCharacter.SetCharacterData(characterData);
        }
        else
        {
            GD.PrintErr("PlayerCharacter is not initialized. Please set the player character before setting data.");
        }
    }

    public Camera2D GetCamera()
    {
        return GetTree().Root.GetNode<Camera2D>("Root/World/Camera2D");
    }
}
