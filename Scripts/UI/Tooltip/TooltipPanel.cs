using Godot;

public partial class TooltipPanel : PanelContainer
{
    /// <summary>
    /// The offset to apply to the tooltip position.
    /// </summary>
    private Vector2 Offset = new(8, -8);

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
        // TODO: Control offsets
        Position = position + Offset - new Vector2(0, Size.Y);
    }
}
