using Godot;
using System;

public partial class TooltipPanel : PanelContainer
{
    /// <summary>
    /// Shows the tooltip and sets its initial parameters.
    /// </summary>
    public void Show(string title, string description, Vector2 initialPosition)
    {
        var richText = FindChild("TooltipRichTextLabel") as RichTextLabel;
        richText.Text = $"{title}: {description}";
        
        SetPosition(initialPosition);

        Show();
    }

    /// <summary>
    /// Sets the position of the tooltip.
    /// </summary>
    public void SetPosition(Vector2 position)
    {
        // TODO: Control offsets
        Position = position;
    }
}
