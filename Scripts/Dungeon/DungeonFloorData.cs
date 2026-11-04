using Godot;
using Godot.Collections;

/// <summary>
/// Central content catalog and presentation text for one dungeon floor.
/// </summary>
[GlobalClass]
public partial class DungeonFloorData : Resource
{
    [Export]
    public string StartingRoomId { get; set; }
    
    [Export]
    public Vector2I MapSize { get; set; }

    [Export]
    public string Name { get; set; }

    [Export]
    public string Subtitle { get; set; }

    [Export]
    public Array<DungeonRoomData> Rooms { get; set; } = [];

    [Export]
    public Array<ActionData> RewardSpells { get; set; } = [];

    [Export]
    public int RewardHealing { get; set; }

    /// <summary>
    /// The player's recurring mana budget before earning a rare blessing.
    /// </summary>
    [Export(PropertyHint.Range, "2,8,1")]
    public int StartingMana { get; set; }

    /// <summary>
    /// Maximum number of permanent mana upgrades that can be claimed on this floor.
    /// </summary>
    [Export(PropertyHint.Range, "0,8,1")]
    public int MaximumBlessingRewards { get; set; }

    [Export]
    public Dictionary Text { get; set; } = new();

    /// <summary>
    /// Formats a shared piece of floor text using its resource-defined template.
    /// </summary>
    public string FormatText(string key, params object[] arguments)
    {
        if (!Text.ContainsKey(key))
        {
            GD.PrintErr($"Missing dungeon text: {key}");
            return key;
        }

        return string.Format(Text[key].AsString(), arguments);
    }
}
