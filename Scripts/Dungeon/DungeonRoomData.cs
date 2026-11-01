using Godot;
using Godot.Collections;
using Priest.Managers;

/// <summary>
/// Editable content, map placement, and encounter configuration for a dungeon room.
/// </summary>
[GlobalClass]
public partial class DungeonRoomData : Resource
{
    [Export]
    public string Id { get; set; }

    [Export]
    public string Title { get; set; }

    [Export]
    public string MapTitle { get; set; }
    [Export(PropertyHint.MultilineText)]
    public string Text { get; set; }

    [Export]
    public RoomKind Kind { get; set; }

    [Export]
    public Vector2I MapPosition { get; set; }

    [Export]
    public Texture2D MapIcon { get; set; }
    
    [Export]
    public Array<string> Exits { get; set; } = [];
    
    [Export]
    public Array<string> OptionalBranches { get; set; } = [];

    [Export]
    public int Healing { get; set; }

    [Export]
    public CharacterData Opponent { get; set; }

    public bool HasEncounter => Opponent != null;
}
