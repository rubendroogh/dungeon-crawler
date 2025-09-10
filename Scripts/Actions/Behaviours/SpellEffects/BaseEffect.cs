using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Base class for all spell effect scenes.
/// Handles default animation flow:
///   - Play "Cast" animation (if present)
///   - Wait for it to finish
///   - Apply a default effect (no-op by default)
///   - Cleanup
/// </summary>
public partial class BaseEffect : Sprite2D
{
    /// <summary>
    /// Override this to add custom effect logic (multi-strikes, etc).
    /// </summary>
    protected virtual Task OnEffect(Character target)
    {
        // Default: do nothing
        return Task.CompletedTask;
    }

    /// <summary>
    /// Plays the effect animation and applies its effect to targets.
    /// </summary>
    public virtual async Task Play(Character target)
    {
        AnimationPlayer animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        if (animationPlayer != null && animationPlayer.HasAnimation("cast"))
        {
            animationPlayer.Play("cast");
            await ToSignal(animationPlayer, AnimationMixer.SignalName.AnimationFinished);
        }

        await OnEffect(target);

        QueueFree(); // cleanup when finished
    }

    /// <summary>
    /// Sets the sprite texture for the effect.
    /// </summary>
    public void SetSprite(Texture2D texture)
    {
        Texture = texture;
    }
}
