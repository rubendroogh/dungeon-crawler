using System;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// Connects a reusable choice scene to its asynchronous dungeon action.
/// </summary>
public partial class SceneChoiceButton : Button
{
    private Func<Task> Effect { get; set; }

    public override void _Ready()
    {
        Pressed += OnPressed;
    }

    /// <summary>
    /// Configures a choice before its scene is added to the choices container.
    /// </summary>
    public void Setup(string title, Func<Task> effect, bool enabled)
    {
        Text = title;
        Effect = effect;
        Disabled = !enabled;
    }

    private async void OnPressed()
    {
        if (Effect != null && !IsQueuedForDeletion())
        {
            await SceneManager.Instance.ResolveChoice(Effect);
        }
    }

    public override void _ExitTree()
    {
        Pressed -= OnPressed;
    }
}
