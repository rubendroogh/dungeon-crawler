using System;
using Godot;

/// <summary>Pixel-scale masonry, projected into the same viewport that holds the enemy.</summary>
public partial class DungeonBackdrop : Control
{
    private Control CombatHud;
    private Node CombatStage;
    private Rect2 RoomBounds;
    private const float PixelSize = 4;

    public void FollowCombatArea(Control hud, Node stage)
    {
        CombatHud = hud;
        CombatStage = stage;
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        Resized += QueueRedraw;
    }

    public override void _Process(double delta)
    {
        var bounds = new Rect2(Vector2.Zero, Size);
        if (CombatHud?.IsVisibleInTree() == true && IsInstanceValid(CombatStage))
        {
            var origin = GetGlobalTransform().AffineInverse() * CombatStage.Get("global_position").AsVector2();
            var size = CombatStage.Get("size").AsVector2();
            if (size.X > 0 && size.Y > 0) bounds = new Rect2(origin, size);
        }
        if (bounds == RoomBounds) return;
        RoomBounds = bounds;
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), new Color("101113"));
        var bounds = RoomBounds.Size.X > 0 ? RoomBounds : new Rect2(Vector2.Zero, Size);
        DrawSetTransform(bounds.Position, 0, Vector2.One * PixelSize);
        var w = bounds.Size.X / PixelSize;
        var h = bounds.Size.Y / PixelSize;
        var left = Mathf.Round(w * .27f);
        var right = w - left;
        var top = Mathf.Round(h * .12f);
        // Enemy sprites are centered in the stage and extend 64 viewport pixels below it.
        var floor = Mathf.Min(h - 20, Mathf.Round(h * .5f + 64 / PixelSize));

        DrawRect(new Rect2(0, 0, w, h), new Color("141519"));
        DrawBackWall(left, right, top, floor);
        DrawSideWall(w, h, left, top, floor, false);
        DrawSideWall(w, h, left, top, floor, true);
        DrawFloor(w, h, left, right, floor);
        DrawDoorway(w * .5f, top, floor, right - left);
        DrawTorch(left + 9, top + (floor - top) * .49f);
        DrawTorch(right - 9, top + (floor - top) * .49f);

        // Foreground shadow and ceiling depth keep attention on the lit encounter space.
        for (var i = 0; i < 10; i++)
        {
            DrawRect(new Rect2(0, i * 2, w, 2), new Color(0, 0, 0, .26f * (1 - i / 10f)));
            DrawRect(new Rect2(0, h - i * 2 - 2, w, 2), new Color(0, 0, 0, .3f * (1 - i / 10f)));
        }
        DrawSetTransform(Vector2.Zero);
    }

    private void DrawBackWall(float left, float right, float top, float floor)
    {
        DrawRect(new Rect2(left, top, right - left, floor - top), new Color("171a1b"));
        const int brickWidth = 22;
        const int brickHeight = 11;
        var row = 0;
        for (var y = top; y < floor; y += brickHeight, row++)
        {
            var column = 0;
            for (var x = left - (row % 2) * brickWidth / 2; x < right; x += brickWidth, column++)
            {
                var x1 = Mathf.Max(left, x);
                var x2 = Mathf.Min(right, x + brickWidth - 1);
                var bottom = Mathf.Min(floor, y + brickHeight - 1);
                var shade = .8f + Noise(row, column, 5) * .3f;
                var rect = new Rect2(x1, y, x2 - x1, bottom - y);
                DrawRect(rect, new Color("343b3c") * new Color(shade, shade, shade));
                DrawLine(new(x1, y), new(x2, y), new Color("454b49"));
                if (rect.Size.X > 8)
                    DrawLine(new(x1 + 3, bottom - 2), new(x1 + 5 + Noise(row, column, 2) * 5, bottom - 2), new Color("2b3233"));
            }
        }
        DrawRect(new Rect2(left, top, right - left, 3), new Color("111416"));
    }

    private void DrawSideWall(float w, float h, float inset, float top, float floor, bool right)
    {
        Vector2 Project(float depth, float height)
        {
            var t = 1 - Mathf.Pow(1 - depth, 1.5f);
            var x = inset * t;
            return Snap(new Vector2(right ? w - x : x, Mathf.Lerp(top * t, Mathf.Lerp(h, floor, t), height)));
        }

        const int rows = 11;
        const int columns = 7;
        for (var row = 0; row < rows; row++)
        {
            for (var column = -1; column < columns; column++)
            {
                var offset = row % 2 * .5f;
                var d1 = Mathf.Clamp((column + offset) / columns, 0, 1);
                var d2 = Mathf.Clamp((column + offset + 1) / columns, 0, 1);
                if (d2 <= d1) continue;
                var a = Project(d1, row / (float)rows);
                var b = Project(d2, row / (float)rows);
                var c = Project(d2, (row + 1f) / rows);
                var d = Project(d1, (row + 1f) / rows);
                var brightness = .58f + d1 * .25f + Noise(row, column, right ? 2 : 1) * .18f;
                DrawPolygon([a, b, c, d], [new Color(.24f * brightness, .27f * brightness, .28f * brightness)]);
                DrawLine(a, b, new Color("363c3b"));
                DrawLine(b, c, new Color("151b1c"));
                DrawLine(c, d, new Color("111718"));
                if (column % 3 == row % 3)
                {
                    var crack = a.Lerp(c, .4f);
                    DrawLine(Snap(crack), Snap(crack + new Vector2(right ? -2 : 2, 3)), new Color("1c2425"));
                }
            }
        }
        // The wall foot has a thicker, worn stone course.
        DrawLine(new(right ? w - inset : inset, floor), new(right ? w : 0, h), new Color("121718"), 3);
    }

    private void DrawFloor(float w, float h, float left, float right, float floor)
    {
        Vector2 Project(float across, float depth)
            => Snap(new Vector2(Mathf.Lerp(Mathf.Lerp(left, 0, depth), Mathf.Lerp(right, w, depth), across), Mathf.Lerp(floor, h, depth)));
        for (var row = 0; row < 8; row++)
        {
            var near = Mathf.Pow(row / 8f, 1.65f);
            var far = Mathf.Pow((row + 1) / 8f, 1.65f);
            for (var col = -1; col < 8; col++)
            {
                var shift = row % 2 * .5f;
                var x1 = Mathf.Clamp((col + shift) / 8f, 0, 1);
                var x2 = Mathf.Clamp((col + shift + 1) / 8f, 0, 1);
                if (x1 == x2) continue;
                var a = Project(x1, near);
                var b = Project(x2, near);
                var c = Project(x2, far);
                var d = Project(x1, far);
                var shade = .7f + Noise(row, col, 8) * .35f;
                DrawPolygon([a, b, c, d], [new Color(.17f * shade, .19f * shade, .19f * shade)]);
                DrawLine(a, b, new Color("383c38"));
                DrawLine(b, c, new Color("111718"));
                DrawLine(c, d, new Color("111718"));
            }
        }
    }

    private void DrawDoorway(float center, float top, float floor, float wallWidth)
    {
        var radius = Mathf.Round(wallWidth * .24f);
        var spring = top + radius + 10;
        var outer = radius + 7;
        DrawRect(new Rect2(center - radius, spring, radius * 2, floor - spring), new Color("080d11"));
        Vector2 ArchPoint(float angle, float r) => Snap(new(center + Mathf.Cos(angle) * r, spring - Mathf.Sin(angle) * r));
        for (var i = 0; i < 12; i++)
        {
            var a = i * Mathf.Pi / 12;
            var b = (i + 1) * Mathf.Pi / 12;
            DrawPolygon([new(center, spring), ArchPoint(a, radius), ArchPoint(b, radius)], [new Color("080d11")]);
            var shade = .77f + Noise(i, 0, 3) * .3f;
            DrawPolygon([ArchPoint(a, radius + 1), ArchPoint(a, outer), ArchPoint(b, outer), ArchPoint(b, radius + 1)], [new Color(.34f * shade, .35f * shade, .31f * shade)]);
            DrawLine(ArchPoint(a, radius + 1), ArchPoint(a, outer), new Color("171d1c"));
            DrawLine(ArchPoint(a, outer), ArchPoint(b, outer), new Color("4c5046"));
        }
        for (var y = spring; y < floor; y += 10)
        {
            var height = Mathf.Min(9, floor - y);
            DrawRect(new Rect2(center - outer, y, 6, height), new Color("43493f"));
            DrawRect(new Rect2(center + radius + 1, y, 6, height), new Color("3b423c"));
            DrawLine(new(center - outer, y), new(center - radius - 1, y), new Color("575b4d"));
        }
        DrawRect(new Rect2(center - outer - 3, floor, outer * 2 + 6, 3), new Color("4a4d42"));
        DrawRect(new Rect2(center - outer - 5, floor + 3, outer * 2 + 10, 2), new Color("202824"));
    }

    private void DrawTorch(float x, float y)
    {
        var origin = Snap(new(x, y));
        for (var radius = 25; radius >= 5; radius -= 3)
            DrawCircle(origin, radius, new Color(.84f, .42f, .12f, .018f));
        DrawRect(new Rect2(origin + new Vector2(-3, 4), new Vector2(6, 3)), new Color("101717"));
        DrawRect(new Rect2(origin + new Vector2(-1, 1), new Vector2(2, 10)), new Color("513b29"));
        DrawPolygon([origin + new Vector2(-3, 2), origin + new Vector2(-4, -3), origin + new Vector2(-1, -9), origin + new Vector2(0, -5), origin + new Vector2(2, -7), origin + new Vector2(4, -1), origin + new Vector2(2, 3)], [new Color("cf6d2f")]);
        DrawPolygon([origin + new Vector2(-2, 1), origin + new Vector2(-1, -5), origin + new Vector2(2, -2), origin + new Vector2(1, 2)], [new Color("f3c876")]);
        DrawRect(new Rect2(origin, Vector2.One), new Color("fff0ba"));
    }

    private static Vector2 Snap(Vector2 point) => new(Mathf.Round(point.X), Mathf.Round(point.Y));

    private static float Noise(int row, int column, int salt)
    {
        var hash = unchecked((uint)(row * 374761393 + column * 668265263 + salt * 1274126177));
        hash = (hash ^ (hash >> 13)) * 1274126177;
        return (hash & 255) / 255f;
    }
}
