using Godot;

/// <summary>
/// Projects resource-configured masonry into the same viewport that holds the enemy.
/// </summary>
public partial class DungeonBackdrop : Control
{
    [Export]
    private Control CombatHud;
    [Export]
    private Control CombatStage;
    [Export]
    private DungeonBackdropStyle Style;

    private Rect2 RoomBounds { get; set; }
    private float PixelSize => Style.PixelSize;

    public override void _Ready()
    {
        if (Style == null)
        {
            GD.PrintErr("Dungeon backdrop requires a style resource.");
            Hide();
            SetProcess(false);
            return;
        }

        MouseFilter = MouseFilterEnum.Ignore;
        Resized += OnResized;
    }

    private void OnResized()
    {
        QueueRedraw();
    }

    public override void _ExitTree()
    {
        Resized -= OnResized;
    }

    public override void _Process(double delta)
    {
        var bounds = new Rect2(Vector2.Zero, Size);
        if (CombatHud?.IsVisibleInTree() == true && IsInstanceValid(CombatStage))
        {
            var origin = GetGlobalTransform().AffineInverse() * CombatStage.GlobalPosition;
            var size = CombatStage.Size;
            if (size.X > 0 && size.Y > 0)
            {
                bounds = new Rect2(origin, size);
            }
        }
        if (bounds == RoomBounds)
        {
            return;
        }

        RoomBounds = bounds;
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), Style.Surround);
        var bounds = RoomBounds.Size.X > 0 ? RoomBounds : new Rect2(Vector2.Zero, Size);
        DrawSetTransform(bounds.Position, 0, Vector2.One * PixelSize);
        var w = bounds.Size.X / PixelSize;
        var h = bounds.Size.Y / PixelSize;
        var left = Mathf.Round(w * Style.WallInsetRatio);
        var right = w - left;
        var top = Mathf.Round(h * Style.BackWallTopRatio);
        // Keep the threshold aligned with the configured sprite foot offset.
        var floor = Mathf.Min(h - 20, Mathf.Round(h * .5f + Style.EnemyFootOffset / PixelSize));

        DrawRect(new Rect2(0, 0, w, h), Style.Ceiling);
        DrawBackWall(left, right, top, floor);
        DrawSideWall(w, h, left, top, floor, false);
        DrawSideWall(w, h, left, top, floor, true);
        DrawFloor(w, h, left, right, floor);
        DrawDoorway(w * .5f, top, floor, right - left);
        DrawTorch(left + 9, top + (floor - top) * .49f);
        DrawTorch(right - 9, top + (floor - top) * .49f);

        DrawForegroundShadows(w, h);
        DrawSetTransform(Vector2.Zero);
    }

    private void DrawForegroundShadows(float width, float height)
    {
        for (var i = 0; i < Style.ShadowLayers; i++)
        {
            var opacity = 1 - i / (float)Style.ShadowLayers;
            DrawRect(new Rect2(0, i * 2, width, 2), new Color(Style.CeilingShadow, Style.CeilingShadow.A * opacity));
            DrawRect(new Rect2(0, height - i * 2 - 2, width, 2), new Color(Style.FloorShadow, Style.FloorShadow.A * opacity));
        }
    }

    private void DrawBackWall(float left, float right, float top, float floor)
    {
        DrawRect(new Rect2(left, top, right - left, floor - top), Style.Mortar);
        var brickWidth = Style.BrickSize.X;
        var brickHeight = Style.BrickSize.Y;
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
                DrawRect(rect, Style.BackStone * new Color(shade, shade, shade));
                DrawLine(new(x1, y), new(x2, y), Style.BackHighlight);
                if (rect.Size.X > 8)
                {
                    DrawLine(new(x1 + 3, bottom - 2), new(x1 + 5 + Noise(row, column, 2) * 5, bottom - 2), Style.BackWear);
                }
            }
        }
        DrawRect(new Rect2(left, top, right - left, 3), Style.BackShadow);
    }

    private void DrawSideWall(float w, float h, float inset, float top, float floor, bool right)
    {
        Vector2 Project(float depth, float height)
        {
            var t = 1 - Mathf.Pow(1 - depth, 1.5f);
            var x = inset * t;
            return Snap(new Vector2(right ? w - x : x, Mathf.Lerp(top * t, Mathf.Lerp(h, floor, t), height)));
        }

        var rows = Style.WallRows;
        var columns = Style.WallColumns;
        for (var row = 0; row < rows; row++)
        {
            for (var column = -1; column < columns; column++)
            {
                var offset = row % 2 * .5f;
                var d1 = Mathf.Clamp((column + offset) / columns, 0, 1);
                var d2 = Mathf.Clamp((column + offset + 1) / columns, 0, 1);
                if (d2 <= d1)
                {
                    continue;
                }

                var a = Project(d1, row / (float)rows);
                var b = Project(d2, row / (float)rows);
                var c = Project(d2, (row + 1f) / rows);
                var d = Project(d1, (row + 1f) / rows);
                var brightness = .58f + d1 * .25f + Noise(row, column, right ? 2 : 1) * .18f;
                DrawPolygon([a, b, c, d], [Shade(Style.WallStone, brightness)]);
                DrawLine(a, b, Style.WallHighlight);
                DrawLine(b, c, Style.WallJoint);
                DrawLine(c, d, Style.DeepJoint);
                if (column % 3 == row % 3)
                {
                    var crack = a.Lerp(c, .4f);
                    DrawLine(Snap(crack), Snap(crack + new Vector2(right ? -2 : 2, 3)), Style.Crack);
                }
            }
        }
        // The wall foot has a thicker, worn stone course.
        DrawLine(new(right ? w - inset : inset, floor), new(right ? w : 0, h), Style.WallFoot, 3);
    }

    private void DrawFloor(float w, float h, float left, float right, float floor)
    {
        Vector2 Project(float across, float depth)
        {
            var x = Mathf.Lerp(Mathf.Lerp(left, 0, depth), Mathf.Lerp(right, w, depth), across);
            return Snap(new Vector2(x, Mathf.Lerp(floor, h, depth)));
        }
        for (var row = 0; row < Style.FloorRows; row++)
        {
            var near = Mathf.Pow(row / (float)Style.FloorRows, 1.65f);
            var far = Mathf.Pow((row + 1) / (float)Style.FloorRows, 1.65f);
            for (var col = -1; col < Style.FloorColumns; col++)
            {
                var shift = row % 2 * .5f;
                var x1 = Mathf.Clamp((col + shift) / Style.FloorColumns, 0, 1);
                var x2 = Mathf.Clamp((col + shift + 1) / Style.FloorColumns, 0, 1);
                if (x1 == x2)
                {
                    continue;
                }

                var a = Project(x1, near);
                var b = Project(x2, near);
                var c = Project(x2, far);
                var d = Project(x1, far);
                var shade = .7f + Noise(row, col, 8) * .35f;
                DrawPolygon([a, b, c, d], [Shade(Style.FloorStone, shade)]);
                DrawLine(a, b, Style.FloorHighlight);
                DrawLine(b, c, Style.DeepJoint);
                DrawLine(c, d, Style.DeepJoint);
            }
        }
    }

    private void DrawDoorway(float center, float top, float floor, float wallWidth)
    {
        var radius = Mathf.Round(wallWidth * Style.DoorRadiusRatio);
        var spring = top + radius + 10;
        var outer = radius + 7;
        DrawRect(new Rect2(center - radius, spring, radius * 2, floor - spring), Style.DoorVoid);
        Vector2 ArchPoint(float angle, float radius)
        {
            return Snap(new Vector2(center + Mathf.Cos(angle) * radius, spring - Mathf.Sin(angle) * radius));
        }
        for (var i = 0; i < 12; i++)
        {
            var a = i * Mathf.Pi / 12;
            var b = (i + 1) * Mathf.Pi / 12;
            DrawPolygon([new(center, spring), ArchPoint(a, radius), ArchPoint(b, radius)], [Style.DoorVoid]);
            var shade = .77f + Noise(i, 0, 3) * .3f;
            DrawPolygon([ArchPoint(a, radius + 1), ArchPoint(a, outer), ArchPoint(b, outer), ArchPoint(b, radius + 1)], [Shade(Style.ArchStone, shade)]);
            DrawLine(ArchPoint(a, radius + 1), ArchPoint(a, outer), Style.ArchJoint);
            DrawLine(ArchPoint(a, outer), ArchPoint(b, outer), Style.ArchHighlight);
        }
        for (var y = spring; y < floor; y += 10)
        {
            var height = Mathf.Min(9, floor - y);
            DrawRect(new Rect2(center - outer, y, 6, height), Style.LeftPier);
            DrawRect(new Rect2(center + radius + 1, y, 6, height), Style.RightPier);
            DrawLine(new(center - outer, y), new(center - radius - 1, y), Style.PierHighlight);
        }
        DrawRect(new Rect2(center - outer - 3, floor, outer * 2 + 6, 3), Style.Threshold);
        DrawRect(new Rect2(center - outer - 5, floor + 3, outer * 2 + 10, 2), Style.ThresholdShadow);
    }

    private void DrawTorch(float x, float y)
    {
        var origin = Snap(new(x, y));
        for (var radius = Style.TorchGlowRadius; radius >= 5; radius -= 3)
        {
            DrawCircle(origin, radius, Style.TorchGlow);
        }

        DrawRect(new Rect2(origin + new Vector2(-3, 4), new Vector2(6, 3)), Style.TorchIron);
        DrawRect(new Rect2(origin + new Vector2(-1, 1), new Vector2(2, 10)), Style.TorchWood);
        DrawPolygon([origin + new Vector2(-3, 2), origin + new Vector2(-4, -3), origin + new Vector2(-1, -9), origin + new Vector2(0, -5), origin + new Vector2(2, -7), origin + new Vector2(4, -1), origin + new Vector2(2, 3)], [Style.FlameOuter]);
        DrawPolygon([origin + new Vector2(-2, 1), origin + new Vector2(-1, -5), origin + new Vector2(2, -2), origin + new Vector2(1, 2)], [Style.FlameInner]);
        DrawRect(new Rect2(origin, Vector2.One), Style.FlameCore);
    }

    /// <summary>
    /// Applies material shading while preserving the configured opacity.
    /// </summary>
    private static Color Shade(Color color, float brightness)
    {
        return new Color(color.R * brightness, color.G * brightness, color.B * brightness, color.A);
    }

    private static Vector2 Snap(Vector2 point)
    {
        return new Vector2(Mathf.Round(point.X), Mathf.Round(point.Y));
    }

    /// <summary>
    /// Produces deterministic material variation so redraws do not shimmer.
    /// </summary>
    private static float Noise(int row, int column, int salt)
    {
        var hash = unchecked((uint)(row * 374761393 + column * 668265263 + salt * 1274126177));
        hash = (hash ^ (hash >> 13)) * 1274126177;
        return (hash & 255) / 255f;
    }
}
