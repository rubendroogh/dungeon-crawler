using Godot;

public partial class TooltipPanel : PanelContainer
{
    /// <summary>
    /// The offset to apply to the tooltip position from the mouse cursor.
    /// </summary>
    private Vector2 BaseOffset = new(8, 8);

    /// <summary>
    /// Shows the tooltip and sets its initial parameters.
    /// </summary>
    public void Show(string title, string description, Vector2 initialPosition)
    {
        var titleLabel = FindChild("TooltipRichTextTitleLabel") as RichTextLabel;
        var richText = FindChild("TooltipRichTextLabel") as RichTextLabel;

        titleLabel.Text = title;
        richText.Text = description;
        
        SetPosition(initialPosition);
        Show();
    }

    /// <summary>
    /// Sets the position of the tooltip.
    /// </summary>
    public void SetPosition(Vector2 position)
    {
        Position = position + GetOffset();
    }

    /// <summary>
    /// Gets the total offset based on the proximity to screen edges
    /// and predefined offset.
    /// </summary>
    private Vector2 GetOffset()
    {
        Vector2 offset = BaseOffset;
        Vector2 screenSize = GetViewportRect().Size;
        var mousePosition = GetViewport().GetMousePosition();

        GD.Print($"Mouse Position: {mousePosition}, Tooltip Size: {Size}, Screen Size: {screenSize}");

        // Adjust horizontal position if going off the right edge
        if (mousePosition.X + Size.X + BaseOffset.X > screenSize.X)
        {
            offset.X = -Size.X - BaseOffset.X;
        }

        // Adjust vertical position if going off the bottom edge
        if (mousePosition.Y + Size.Y + BaseOffset.Y > screenSize.Y)
        {
            offset.Y = -Size.Y - BaseOffset.Y;
        }

        return offset;
    }
}
