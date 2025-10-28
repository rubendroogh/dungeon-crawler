using Godot;
using System;
using System.Threading.Tasks;

public partial class PlayerHealthBar : TextureProgressBar
{
    /// <summary>
    /// The overlaid health bar representing current health.
    /// </summary>
    private TextureProgressBar HealthBarOver => GetNode<TextureProgressBar>("HealthBarOver");

    /// <summary>
    /// The underlaid health bar representing delayed health change.
    /// </summary>
    private TextureProgressBar HealthBarUnder => this;

    /// <summary>
    /// Initializes the health bar with maximum and current health values.
    /// </summary>
    public void Initialize(float maxHealth, float currentHealth)
    {
        HealthBarOver.MaxValue = maxHealth;
        HealthBarUnder.MaxValue = maxHealth;

        HealthBarOver.Value = currentHealth;
        HealthBarUnder.Value = currentHealth;
    }

    /// <summary>
    /// Sets the health value with animated transitions.
    /// </summary>
    public async Task SetHealth(float health)
    {
        var tween = CreateTween();
        tween.TweenProperty(HealthBarOver, "value", health, .2f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);

        tween.TweenProperty(HealthBarUnder, "value", health, .5f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);

        await ToSignal(tween, "finished");
    }
}
