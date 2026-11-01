using System.Collections.Generic;
using System.Linq;
using Godot;
using Priest.Managers;

/// <summary>
/// Presents the floor's room graph on a grid and lets the player inspect room progress.
/// </summary>
public partial class DungeonMap : Control
{
    [Export]
    private DungeonMapStyle Style;
    [Export]
    private DungeonFloorData Floor;

    private SceneManager Run { get; set; }
    private string HoveredRoom { get; set; }
    private float Cell { get; set; }
    private Vector2 Origin { get; set; }
    private Font MapFont { get; set; }
    private Rect2 Board { get; set; }
    private Dictionary<string, Vector2I> RoomPositions { get; set; }

    public override Vector2 _GetMinimumSize()
    {
        return Style?.MinimumSize ?? Vector2.Zero;
    }

    public override void _Ready()
    {
        if (Style == null || Floor == null)
        {
            GD.PrintErr("Dungeon map requires floor data and a map style.");
            return;
        }

        RoomPositions = Floor.Rooms.ToDictionary(room => room.Id, room => room.MapPosition);
        UpdateMinimumSize();
        MapFont = Style.Font ?? ThemeDB.FallbackFont;
        Resized += OnMapChanged;
        MouseExited += OnMouseExited;
        CallDeferred(nameof(InitializeCustomSignals));
    }

    private void InitializeCustomSignals()
    {
        Run = SceneManager.Instance;
        Run.MapChanged += OnMapChanged;
        QueueRedraw();
    }

    public override void _ExitTree()
    {
        if (IsInstanceValid(Run))
        {
            Run.MapChanged -= OnMapChanged;
        }

        Resized -= OnMapChanged;
        MouseExited -= OnMouseExited;
    }

    private void OnMapChanged()
    {
        QueueRedraw();
    }

    private void OnMouseExited()
    {
        HoveredRoom = null;
        QueueRedraw();
    }

    public override void _GuiInput(InputEvent input)
    {
        if (input is not InputEventMouseMotion motion || Cell <= 0)
        {
            return;
        }

        var tile = (motion.Position - Origin) / Cell;
        var coordinate = new Vector2I(Mathf.FloorToInt(tile.X), Mathf.FloorToInt(tile.Y));
        var hovered = RoomPositions.FirstOrDefault(entry => entry.Value == coordinate).Key;
        if (HoveredRoom == hovered)
        {
            return;
        }

        HoveredRoom = hovered;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (MapFont == null)
        {
            return;
        }

        UpdateBoardLayout();
        DrawHeader();
        DrawGrid();
        DrawFloor();
        DrawRect(Board, Style.Border, false);
        DrawFooter();
    }

    private void UpdateBoardLayout()
    {
        Cell = Mathf.Floor(Mathf.Min(
            (Size.X - Style.HorizontalPadding) / Floor.MapSize.X,
            (Size.Y - Style.FooterHeight) / Floor.MapSize.Y));
        Origin = new Vector2(Mathf.Floor((Size.X - Cell * Floor.MapSize.X) / 2), Style.BoardTop);
        Board = new Rect2(Origin, new Vector2(Cell * Floor.MapSize.X, Cell * Floor.MapSize.Y));
    }

    private void DrawHeader()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), Style.Background);
        DrawRect(new Rect2(0, 0, Size.X, 3), Style.Gold.Darkened(.45f));
        Write(Floor.Name, new(24, 35), Style.Gold, Style.HeadingFontSize);
        Write(Floor.Subtitle, new(24, 56), Style.Ink, Style.CaptionFontSize);
        DrawLine(new(24, 69), new(Size.X - 24, 69), Style.Divider);
    }

    private void DrawGrid()
    {
        DrawRect(Board, Style.Board);
        for (var column = 0; column <= Floor.MapSize.X; column++)
        {
            var x = Origin.X + column * Cell;
            DrawLine(new(x, Origin.Y), new(x, Board.End.Y), Style.Grid);
            if (column < Floor.MapSize.X)
            {
                Center(((char)('A' + column)).ToString(), new(x + Cell / 2, Origin.Y - 10),
                    Style.Ink.Darkened(.25f), Style.CoordinateFontSize);
            }
        }

        for (var row = 0; row <= Floor.MapSize.Y; row++)
        {
            var y = Origin.Y + row * Cell;
            DrawLine(new(Origin.X, y), new(Board.End.X, y), Style.Grid);
            if (row < Floor.MapSize.Y)
            {
                Write((row + 1).ToString("00"), new(Origin.X - 22, y + Cell / 2 + 4),
                    Style.Ink.Darkened(.35f), Style.CoordinateFontSize - 1);
            }
        }
    }

    private void DrawFloor()
    {
        if (Run?.Dungeon == null)
        {
            return;
        }

        foreach (var room in Run.Dungeon.Rooms.Values)
        {
            foreach (var exit in room.Exits)
            {
                DrawCorridor(room.Id, exit);
            }
        }

        foreach (var room in Run.Dungeon.Rooms.Values)
        {
            DrawRoom(room);
        }
    }

    private void DrawFooter()
    {
        var footer = Board.End.Y + 22;
        DrawLegend(new(26, footer), Style.Gold, Floor.FormatText("map_you"));
        DrawLegend(new(105, footer), Style.Visited, Floor.FormatText("map_visited"));
        DrawLegend(new(221, footer), Style.Unexplored, Floor.FormatText("map_unexplored"));
        DrawLine(new(24, footer + 19), new(Size.X - 24, footer + 19), Style.Divider);
        DrawRoomDetails(footer);
        Write(Floor.FormatText("map_hint"), new(24, Size.Y - 22), Style.Ink, Style.CaptionFontSize + 1);
    }

    private void DrawRoomDetails(float footer)
    {
        var id = HoveredRoom ?? Run?.CurrentRoom?.Id;
        if (id == null || !Run.Dungeon.Rooms.TryGetValue(id, out var selected))
        {
            return;
        }

        var point = selected.MapPosition;
        Write($"{(char)('A' + point.X)}{point.Y + 1:00}  /  {selected.MapTitle}",
            new(24, footer + 44), Style.Gold, Style.DetailFontSize);
        Write(GetRoomStatus(id), new(24, footer + 64), Style.Ink, Style.CaptionFontSize);
    }

    private string GetRoomStatus(string id)
    {
        if (id == Run.CurrentRoom?.Id)
        {
            return Floor.FormatText("map_here");
        }

        if (Run.IsRoomComplete(id))
        {
            return Floor.FormatText("map_completed");
        }

        return Floor.FormatText(Run.VisitedRooms.Contains(id) ? "map_visited" : "map_unexplored");
    }

    /// <summary>
    /// Branches outward before descending and joins inward on the destination row.
    /// </summary>
    private void DrawCorridor(string from, string to)
    {
        if (!RoomPositions.TryGetValue(from, out var start) || !RoomPositions.TryGetValue(to, out var end))
        {
            return;
        }

        var traced = Run.VisitedRooms.Contains(from) && Run.VisitedRooms.Contains(to);
        var color = traced ? Style.Visited : Style.Corridor;
        var horizontalFirst = start.X == Floor.MapSize.X / 2;
        var corner = horizontalFirst ? new Vector2I(end.X, start.Y) : new Vector2I(start.X, end.Y);
        DrawCorridorSegment(start, corner, color);
        DrawCorridorSegment(corner, end, color);
    }

    private void DrawCorridorSegment(Vector2I start, Vector2I end, Color color)
    {
        DrawLine(TileCenter(start), TileCenter(end), Style.CorridorShadow, 10);
        DrawLine(TileCenter(start), TileCenter(end), color, 4);
    }

    private void DrawRoom(DungeonRoomData room)
    {
        var current = Run.CurrentRoom?.Id == room.Id;
        var visited = Run.VisitedRooms.Contains(room.Id);
        var color = current ? Style.Gold : visited ? Style.Visited : Style.Unexplored;
        var center = TileCenter(room.MapPosition);
        var side = Cell - Style.RoomInset;
        var rect = new Rect2(center - Vector2.One * side / 2, Vector2.One * side);
        if (current)
        {
            DrawRect(rect.Grow(3), Style.CurrentBorder, false, 2);
        }

        DrawRect(rect, current ? Style.CurrentRoom : visited ? Style.VisitedRoom : Style.UnexploredRoom);
        DrawRect(rect, HoveredRoom == room.Id ? Colors.White : color.Darkened(current ? 0 : .3f), false, 2);
        DrawLine(rect.Position + new Vector2(3, 3), new(rect.End.X - 3, rect.Position.Y + 3), color.Darkened(.45f));
        DrawRoomIcon(room, center, color, Mathf.Min(12, side * .29f));
        DrawRoomMarkers(room, center, rect, current);
    }

    private void DrawRoomIcon(DungeonRoomData room, Vector2 center, Color color, float radius)
    {
        if (room.MapIcon == null)
        {
            return;
        }

        var iconColor = room.Kind == RoomKind.Boss && !Run.VisitedRooms.Contains(room.Id) ? Style.Boss : color;
        DrawTextureRect(room.MapIcon, new Rect2(center - Vector2.One * radius, Vector2.One * radius * 2), false, iconColor);
    }

    private void DrawRoomMarkers(DungeonRoomData room, Vector2 center, Rect2 rect, bool current)
    {
        if (Run.IsRoomComplete(room.Id))
        {
            var mark = rect.End - new Vector2(7, 7);
            DrawRect(new Rect2(mark - Vector2.One * 4, Vector2.One * 8), Style.CompletedBackground);
            DrawLine(mark + new Vector2(-3, 0), mark + new Vector2(-1, 2), Style.Completed, 2);
            DrawLine(mark + new Vector2(-1, 2), mark + new Vector2(3, -3), Style.Completed, 2);
        }

        if (current)
        {
            var top = rect.Position.Y;
            DrawPolygon([new(center.X - 4, top - 9), new(center.X + 4, top - 9), new(center.X, top - 4)], [Style.Gold]);
        }
    }

    private Vector2 TileCenter(Vector2I tile)
    {
        return Origin + (new Vector2(tile.X, tile.Y) + Vector2.One * .5f) * Cell;
    }

    private void Write(string text, Vector2 position, Color color, int size)
    {
        DrawString(MapFont, position, text, HorizontalAlignment.Left, -1, size, color);
    }

    private void Center(string text, Vector2 position, Color color, int size)
    {
        var width = MapFont.GetStringSize(text, HorizontalAlignment.Left, -1, size).X;
        Write(text, position - new Vector2(width / 2, 0), color, size);
    }

    private void DrawLegend(Vector2 position, Color color, string text)
    {
        DrawRect(new Rect2(position + new Vector2(0, -7), Vector2.One * 6), color);
        Write(text, position + new Vector2(12, 0), color, Style.CoordinateFontSize);
    }
}
