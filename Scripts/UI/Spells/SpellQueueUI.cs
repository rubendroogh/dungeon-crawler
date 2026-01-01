using System.Collections.Generic;
using Godot;

/// <summary>
/// Manages the UI for a spell in the spell queue.
/// </summary>
public partial class SpellQueueUI : Control
{
    /// <summary>
    /// The spell being displayed in the queue.
    /// </summary>
    private Action Spell { get; set; }

    /// <summary>
    /// The blessings that have been selected to cast this spell with.
    /// </summary>
    private List<Blessing> Blessings { get; set; }

    /// <summary>
    /// The label displaying the spell's name.
    /// </summary>
    [Export]
    private RichTextLabel NameLabel { get; set; }

    /// <summary>
    /// The TextureRect displaying the spell's icon.
    /// </summary>
    [Export]
    private TextureRect SpellIcon { get; set; }

    public override void _Ready()
    {
        // Initialization code can go here if needed
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }

    public override void _GuiInput(InputEvent @event)
    {
        // Input handling code can go here if needed
    }

    public void OnMouseEntered()
    {
        Managers.ManaSourceManager.HighlightBlessings(Blessings);
    }

    public void OnMouseExited()
    {
        Managers.ManaSourceManager.ClearHighlighted();
    }

    /// <summary>
    /// Sets up the SpellQueueUI to display the spell in the queue.
    /// </summary>
    public void Setup(Action spell, List<Blessing> blessings)
    {
        Spell = spell;
        Blessings = blessings;

        // Update UI elements
        NameLabel.Text = Spell.Data.Name;
        SpellIcon.Texture = Spell.Data.Image;
    }
}
