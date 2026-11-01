using System;
using System.Collections.Generic;

namespace Priest.Managers;

public enum RoomKind { Entrance, Battle, Shrine, Archive, Crossroads, Boss, Exit }

/// <summary>A small branching floor. Each room has a stable identity for one-time rewards.</summary>
public class DungeonGenerator
{
    public Dictionary<string, DungeonRoom> Rooms { get; } = new()
    {
        ["entrance"] = new("entrance", "The stairs of Alderwick", "Everyone from the old world walked into this dungeon on the day of the Influx. None returned. Now the gods have sent you to discover what called them.\n\nA damp passage descends beneath the ruined chapel.", RoomKind.Entrance, ["gate"]),
        ["gate"] = new("gate", "The flooded threshold", "A dead thing stirs in the water. Beyond it, the passage divides.", RoomKind.Battle, ["shrine", "archive"]),
        ["shrine"] = new("shrine", "Left · The forgotten shrine", "A stone basin holds clear water. Four worn figures watch over a single unlit candle. You can drink, or ask a god for another blessing.", RoomKind.Shrine, ["crossroads"]),
        ["archive"] = new("archive", "Right · The drowned archive", "Waterlogged books lie beneath a watchful sentinel. A sealed grimoire glows on its pedestal. Defeat the sentinel to claim a spell.", RoomKind.Archive, ["crossroads"]),
        ["crossroads"] = new("crossroads", "The last lantern", "Two paths meet under a dying lantern. Behind you: the flooded halls. Ahead: the Keeper of the Influx.\n\nA quiet alcove offers one final moment of rest.", RoomKind.Crossroads, ["boss"]),
        ["boss"] = new("boss", "The Keeper of the Influx", "At the heart of the ruin, the Keeper guards the old world's final doorway. It raises its hand, gathering heat. There is no passage without a fight.", RoomKind.Boss, ["exit"]),
        ["exit"] = new("exit", "Beyond the first door", "The Keeper falls. For the first time since the Influx, light reaches the doorway.\n\nYou have survived the first descent into Alderwick.", RoomKind.Exit, [])
    };

    public DungeonRoom Generate() => Rooms["entrance"];
}

public record DungeonRoom(string Id, string Title, string Text, RoomKind Kind, string[] Exits);

public class Scene
{
    public string Title { get; set; }
    public string Text { get; set; }
    public List<Choice> Choices { get; } = new();
}

public class Choice
{
    public string Title { get; set; }
    public Func<bool> Prerequisite { get; set; }
    public System.Action Effect { get; set; }
}
