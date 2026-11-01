using Godot;

/// <summary>Simple first-person stone corridor drawn with the existing palette.</summary>
public partial class DungeonBackdrop : Control
{
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        Resized += QueueRedraw;
    }

    public override void _Draw()
    {
        var w = Size.X;
        var h = Size.Y;
        var stone = new Color("272a2d");
        var edge = new Color("393b3c");
        DrawRect(new Rect2(Vector2.Zero, Size), new Color("0e1115"));
        DrawPolygon([new(0, h), new(w * .35f, h * .54f), new(w * .65f, h * .54f), new(w, h)], [new Color("242321")]);
        DrawPolygon([new(0, 0), new(w * .35f, h * .25f), new(w * .35f, h * .7f), new(0, h)], [stone]);
        DrawPolygon([new(w, 0), new(w * .65f, h * .25f), new(w * .65f, h * .7f), new(w, h)], [stone]);
        for (var i = 1; i < 8; i++)
        {
            var t = i / 8f;
            var x = w * .35f * t;
            var y = h * .25f * t;
            DrawLine(new(x, y), new(x, h - h * .3f * t), edge, 2);
            DrawLine(new(w - x, y), new(w - x, h - h * .3f * t), edge, 2);
        }
        for (var i = 1; i < 7; i++)
        {
            var t = i / 7f;
            DrawLine(new(0, h * t), new(w * .35f, h * (.25f + .45f * t)), edge, 2);
            DrawLine(new(w, h * t), new(w * .65f, h * (.25f + .45f * t)), edge, 2);
        }
        DrawRect(new Rect2(w * .35f, h * .25f, w * .3f, h * .45f), new Color("171a1d"));
        DrawRect(new Rect2(w * .43f, h * .31f, w * .14f, h * .39f), new Color("080b10"));
        DrawCircle(new(w * .32f, h * .39f), 17, new Color("e7b463"));
        DrawCircle(new(w * .68f, h * .39f), 17, new Color("e7b463"));
    }
}
