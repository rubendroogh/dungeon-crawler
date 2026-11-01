using System.Collections.Generic;
using Godot;
using Priest.Managers;

/// <summary>A grid atlas of the current floor. Inspecting a room never moves the player.</summary>
public partial class DungeonMap : Control
{
    private static readonly Dictionary<string, Vector2I> Layout = new()
    {
        ["entrance"] = new(3, 1), ["gate"] = new(3, 3),
        ["shrine"] = new(1, 5), ["archive"] = new(5, 5),
        ["crossroads"] = new(3, 7), ["boss"] = new(3, 9), ["exit"] = new(3, 11)
    };
    private readonly Color Gold = new("dbc18a");
    private readonly Color Ink = new("8c999c");
    private SceneManager Run;
    private string HoveredRoom;
    private float Cell;
    private Vector2 Origin;
    private Font MapFont;

    public void Bind(SceneManager run)
    {
        Run = run;
        Run.MapChanged += QueueRedraw;
        QueueRedraw();
    }

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(370, 760);
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        SizeFlagsVertical = SizeFlags.ExpandFill;
        MouseFilter = MouseFilterEnum.Stop;
        MapFont = ThemeDB.FallbackFont;
        Resized += QueueRedraw;
        MouseExited += () => { HoveredRoom = null; QueueRedraw(); };
    }

    public override void _ExitTree()
    {
        if (Run != null) Run.MapChanged -= QueueRedraw;
    }

    public override void _GuiInput(InputEvent input)
    {
        if (input is not InputEventMouseMotion motion || Cell <= 0) return;
        var tile = (motion.Position - Origin) / Cell;
        string hovered = null;
        foreach (var entry in Layout)
            if (new Vector2I(Mathf.FloorToInt(tile.X), Mathf.FloorToInt(tile.Y)) == entry.Value)
                hovered = entry.Key;
        if (HoveredRoom == hovered) return;
        HoveredRoom = hovered;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (MapFont == null) return;
        DrawRect(new Rect2(Vector2.Zero, Size), new Color("10171b"));
        DrawRect(new Rect2(0, 0, Size.X, 3), Gold.Darkened(.45f));
        Write("ALDERWICK", new(24, 35), Gold, 22);
        Write("FLOOR 01  /  THE FIRST DESCENT", new(24, 56), Ink, 11);
        DrawLine(new(24, 69), new(Size.X - 24, 69), new Color("303c40"));

        Cell = Mathf.Floor(Mathf.Min((Size.X - 66) / 7, (Size.Y - 236) / 13));
        Origin = new Vector2(Mathf.Floor((Size.X - Cell * 7) / 2), 98);
        var board = new Rect2(Origin, new Vector2(Cell * 7, Cell * 13));
        DrawRect(board, new Color("141e22"));
        for (var column = 0; column <= 7; column++)
        {
            var x = Origin.X + column * Cell;
            DrawLine(new(x, Origin.Y), new(x, board.End.Y), new Color("202c31"));
            if (column < 7) Center(((char)('A' + column)).ToString(), new(x + Cell / 2, Origin.Y - 10), Ink.Darkened(.25f), 10);
        }
        for (var row = 0; row <= 13; row++)
        {
            var y = Origin.Y + row * Cell;
            DrawLine(new(Origin.X, y), new(board.End.X, y), new Color("202c31"));
            if (row < 13) Write((row + 1).ToString("00"), new(Origin.X - 22, y + Cell / 2 + 4), Ink.Darkened(.35f), 9);
        }
        if (Run?.Dungeon != null)
        {
            // Corridors follow the room graph, using only orthogonal grid steps.
            foreach (var room in Run.Dungeon.Rooms.Values)
                foreach (var exit in room.Exits)
                    DrawCorridor(room.Id, exit);
            foreach (var entry in Layout)
                if (Run.Dungeon.Rooms.TryGetValue(entry.Key, out var room)) DrawRoom(room, entry.Value);
        }
        DrawRect(board, new Color("3a4749"), false);

        var footer = board.End.Y + 22;
        Legend(new(26, footer), Gold, "YOU");
        Legend(new(105, footer), new Color("81a499"), "VISITED");
        Legend(new(221, footer), new Color("52636c"), "UNEXPLORED");
        DrawLine(new(24, footer + 19), new(Size.X - 24, footer + 19), new Color("303c40"));
        var id = HoveredRoom ?? Run?.CurrentRoom?.Id;
        if (id != null && Run.Dungeon.Rooms.TryGetValue(id, out var selected))
        {
            var point = Layout[id];
            Write($"{(char)('A' + point.X)}{point.Y + 1:00}  /  {RoomName(selected.Kind)}", new(24, footer + 44), Gold, 16);
            var status = id == Run.CurrentRoom?.Id ? "YOU ARE HERE" : Run.IsRoomComplete(id) ? "COMPLETED" : Run.VisitedRooms.Contains(id) ? "VISITED" : "UNEXPLORED";
            Write(status, new(24, footer + 64), Ink, 11);
        }
        Write("Both branches can be explored.", new(24, Size.Y - 22), Ink, 12);
    }

    private void DrawCorridor(string from, string to)
    {
        if (!Layout.TryGetValue(from, out var start) || !Layout.TryGetValue(to, out var end)) return;
        var traced = Run.VisitedRooms.Contains(from) && Run.VisitedRooms.Contains(to);
        var color = traced ? new Color("708e84") : new Color("34454d");
        var point = start;
        // Branch outward before descending; join inward only after reaching the lower row.
        var horizontalFirst = start.X == 3;
        void Step(bool horizontal)
        {
            while (horizontal ? point.X != end.X : point.Y != end.Y)
            {
                var next = point + (horizontal ? new Vector2I(System.Math.Sign(end.X - point.X), 0) : new Vector2I(0, System.Math.Sign(end.Y - point.Y)));
                DrawLine(TileCenter(point), TileCenter(next), new Color("0b1115"), 10);
                DrawLine(TileCenter(point), TileCenter(next), color, 4);
                point = next;
            }
        }
        Step(horizontalFirst);
        Step(!horizontalFirst);
    }

    private void DrawRoom(DungeonRoom room, Vector2I tile)
    {
        var current = Run.CurrentRoom?.Id == room.Id;
        var visited = Run.VisitedRooms.Contains(room.Id);
        var hovered = HoveredRoom == room.Id;
        var color = current ? Gold : visited ? new Color("81a499") : new Color("627985");
        var center = TileCenter(tile);
        var side = Cell - 8;
        var rect = new Rect2(center - Vector2.One * side / 2, Vector2.One * side);
        if (current) DrawRect(rect.Grow(3), new Color("66583c"), false, 2);
        DrawRect(rect, current ? new Color("3c3528") : visited ? new Color("233932") : new Color("1b282f"));
        DrawRect(rect, hovered ? Colors.White : color.Darkened(current ? 0 : .3f), false, 2);
        DrawLine(rect.Position + new Vector2(3, 3), new(rect.End.X - 3, rect.Position.Y + 3), color.Darkened(.45f));
        DrawIcon(room.Kind, center, color, Mathf.Min(12, side * .29f));
        if (Run.IsRoomComplete(room.Id))
        {
            var mark = rect.End - new Vector2(7, 7);
            DrawRect(new Rect2(mark - Vector2.One * 4, Vector2.One * 8), new Color("12201c"));
            DrawLine(mark + new Vector2(-3, 0), mark + new Vector2(-1, 2), new Color("a0c9a9"), 2);
            DrawLine(mark + new Vector2(-1, 2), mark + new Vector2(3, -3), new Color("a0c9a9"), 2);
        }
        if (current) DrawPolygon([center + new Vector2(-4, -side / 2 - 9), center + new Vector2(4, -side / 2 - 9), center + new Vector2(0, -side / 2 - 4)], [Gold]);
    }

    private void DrawIcon(RoomKind kind, Vector2 c, Color color, float r)
    {
        Vector2 P(float x, float y) => c + new Vector2(x, y) * r;
        void Line(float x1, float y1, float x2, float y2) => DrawLine(P(x1, y1), P(x2, y2), color, 2);
        switch (kind)
        {
            case RoomKind.Entrance:
                for (var i = 0; i < 4; i++) Line(-.8f + i * .3f, .8f - i * .5f, .8f, .8f - i * .5f);
                break;
            case RoomKind.Battle:
                Line(-.8f, -.8f, .8f, .8f); Line(.8f, -.8f, -.8f, .8f);
                Line(-.9f, .3f, -.3f, .9f); Line(.9f, .3f, .3f, .9f);
                break;
            case RoomKind.Shrine:
                DrawPolyline([P(0, -1), P(.6f, -.1f), P(0, .6f), P(-.6f, -.1f), P(0, -1)], color, 2);
                Line(-.8f, .9f, .8f, .9f); break;
            case RoomKind.Archive:
                DrawPolyline([P(-.9f, -.7f), P(0, -.4f), P(.9f, -.7f), P(.9f, .7f), P(0, 1), P(-.9f, .7f), P(-.9f, -.7f)], color, 2);
                Line(0, -.4f, 0, .9f); break;
            case RoomKind.Crossroads:
                DrawRect(new Rect2(P(-.5f, -.5f), Vector2.One * r), color, false, 2);
                Line(-.7f, -.6f, 0, -1); Line(0, -1, .7f, -.6f); Line(-.7f, .7f, .7f, .7f); break;
            case RoomKind.Boss:
                var crownColor = Run.VisitedRooms.Contains("boss") ? color : new Color("bf837b");
                DrawPolyline([P(-.9f, -.6f), P(-.4f, -.1f), P(0, -1), P(.4f, -.1f), P(.9f, -.6f), P(.7f, .7f), P(-.7f, .7f), P(-.9f, -.6f)], crownColor, 2);
                break;
            case RoomKind.Exit:
                DrawPolyline([P(-.7f, .9f), P(-.7f, -.9f), P(.7f, -.9f), P(.7f, .9f)], color, 2);
                Line(0, .6f, 0, -.5f); Line(-.3f, -.1f, 0, -.5f); Line(0, -.5f, .3f, -.1f); break;
        }
    }

    private Vector2 TileCenter(Vector2I tile) => Origin + (new Vector2(tile.X, tile.Y) + Vector2.One * .5f) * Cell;
    private void Write(string text, Vector2 position, Color color, int size) => DrawString(MapFont, position, text, HorizontalAlignment.Left, -1, size, color);
    private void Center(string text, Vector2 position, Color color, int size) => Write(text, position - new Vector2(MapFont.GetStringSize(text, HorizontalAlignment.Left, -1, size).X / 2, 0), color, size);
    private void Legend(Vector2 position, Color color, string text)
    {
        DrawRect(new Rect2(position + new Vector2(0, -7), Vector2.One * 6), color);
        Write(text, position + new Vector2(12, 0), color, 10);
    }
    private static string RoomName(RoomKind kind) => kind switch
    {
        RoomKind.Entrance => "Chapel stairs", RoomKind.Battle => "Flooded threshold", RoomKind.Shrine => "Forgotten shrine",
        RoomKind.Archive => "Drowned archive", RoomKind.Crossroads => "Last lantern", RoomKind.Boss => "Keeper of the Influx", _ => "The first door"
    };
}
