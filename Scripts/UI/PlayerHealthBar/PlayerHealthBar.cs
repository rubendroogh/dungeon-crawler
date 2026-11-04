using Godot;
using System;
using System.Threading.Tasks;

/// <summary>
/// Presents player health with immediate and delayed progress bars and the current block pool.
/// </summary>
public partial class PlayerHealthBar : TextureProgressBar
{
    [Export]
    private Label HealthText;

    [Export]
    private string HealthFormat;

    [Export]
    private Label BlockText;

    [Export]
    private string BlockFormat;

    /// <summary>
    /// The overlaid health bar representing current health.
    /// </summary>
    [Export]
    private TextureProgressBar HealthBarOver;

    /// <summary>
    /// The underlaid health bar representing delayed health change.
    /// </summary>
    private TextureProgressBar HealthBarUnder => this;

    /// <summary>
    /// Initializes the health bar with maximum and current health values.
    /// </summary>
    public void Initialize(float maxHealth, float currentHealth)
    {
        HealthText.Text = string.Format(HealthFormat, currentHealth, maxHealth);
        HealthBarOver.MaxValue = maxHealth;
        HealthBarUnder.MaxValue = maxHealth;

        HealthBarOver.Value = currentHealth;
        HealthBarUnder.Value = currentHealth;
        SetBlock(0);
    }

    public void SetBlock(float block)
    {
        BlockText.Text = string.Format(BlockFormat, block);
    }

    /// <summary>
    /// Sets the health value with animated transitions.
    /// </summary>
    public async Task SetHealth(float health)
    {
        HealthText.Text = string.Format(HealthFormat, health, HealthBarOver.MaxValue);
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
