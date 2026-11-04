using System;
using System.Collections.Generic;
using System.Linq;

namespace Priest.Managers;

/// <summary>
/// Identifies the interaction offered by a dungeon room.
/// </summary>
public enum RoomKind
{
    /// <summary>
    /// Starting room that introduces the floor and leads into the dungeon.
    /// </summary>
    Entrance,

    /// <summary>
    /// Regular combat encounter that offers a reward after victory.
    /// </summary>
    Battle,

    /// <summary>
    /// Sanctuary offering one-time healing before the next encounter.
    /// </summary>
    Shrine,

    /// <summary>
    /// Optional guarded archive whose victory can offer a rare blessing, a spell, or recovery.
    /// </summary>
    Archive,

    /// <summary>
    /// Junction offering a one-time rest and access to optional branches before the boss.
    /// </summary>
    Crossroads,

    /// <summary>
    /// Final combat encounter whose defeat opens the route to the floor's exit.
    /// </summary>
    Boss,

    /// <summary>
    /// Destination that completes the run and offers a fresh descent.
    /// </summary>
    Exit
}

/// <summary>
/// Resolves the resource-defined floor into a room graph for a new run.
/// </summary>
public class DungeonGenerator
{
    public DungeonFloorData Definition { get; }
    public Dictionary<string, DungeonRoomData> Rooms { get; }

    public DungeonGenerator(DungeonFloorData definition)
    {
        Definition = definition;
        Rooms = definition.Rooms.ToDictionary(room => room.Id);
    }

    public DungeonRoomData Generate()
    {
        return Rooms[Definition.StartingRoomId];
    }
}

/// <summary>
/// Presentation content and choices for a dungeon interaction.
/// </summary>
public class Scene
{
    public string Title { get; set; }
    public string Text { get; set; }
    public List<Choice> Choices { get; } = [];
}

/// <summary>
/// A selectable action with an optional eligibility check.
/// </summary>
public class Choice
{
    public string Title { get; set; }
    public Func<bool> Prerequisite { get; set; }
    public System.Action Effect { get; set; }
}
