using Godot;

public partial class TooltipManager : Node
{
    /// <summary>
    /// The singleton tooltip component.
    /// </summary>
    [Export]
    public TooltipPanel TooltipComponent { get; set; }

    /// <summary>
    /// Indicates whether the tooltip is currently visible.
    /// </summary>
    public bool IsTooltipVisible => TooltipComponent.IsVisible();

    public override void _Ready()
    {
        TooltipComponent.Hide();
    }

    /// <summary>
    /// Shows the tooltip with the given title and text at the specified position.
    /// </summary>
    public void Show(string title, string description, Vector2 position)
    {
        if (TooltipComponent == null)
        {
            GD.PrintErr("TooltipManager: TooltipComponent missing.");
            return;
        }

        TooltipComponent.Show(title, description, position);
    }

    /// <summary>
    /// Updates the position of the tooltip (e.g. to follow the mouse)
    /// </summary>
    public void UpdatePosition(Vector2 position)
    {
        TooltipComponent.SetPosition(position);
    }

    /// <summary>
    /// Hides the tooltip.
    /// </summary>
    public void Hide()
    {
        TooltipComponent.Hide();
    }
}
