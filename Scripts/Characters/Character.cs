using System.Collections.Generic;
using Godot;

/// <summary>
/// An entity that is a participant in a battle on either side.
/// </summary>
public partial class Character : Node
{
    /// <summary>
    /// The current health of the character.
    /// This value is between 0 and the max health of the character.
    /// </summary>
    public float CurrentHealth { get; private set; }

    /// <summary>
    /// The character data associated with this character.
    /// This data contains the base stats, personality traits, and other information about the character.
    /// </summary>
    [Export]
    public CharacterData CharacterData { get; set; }

    /// <summary>
    /// The current max health of the character.
    /// </summary>
    public float CurrentPhysicalDamageMultiplier => CharacterData.BasePhysicalDamageMultiplier;

    /// <summary>
    /// The current dark damage multiplier.
    /// </summary>
    public float CurrentDarkDamageMultiplier => CharacterData.BaseDarkDamageMultiplier;
    
    /// <summary>
    /// The current light damage multiplier.
    /// </summary>
    public float CurrentLightDamageMultiplier => CharacterData.BaseLightDamageMultiplier;
    
    /// <summary>
    /// The current fire damage multiplier.
    /// </summary>
    public float CurrentFireDamageMultiplier => CharacterData.BaseFireDamageMultiplier;
    
    /// <summary>
    /// The current ice damage multiplier.
    /// </summary>
    public float CurrentIceDamageMultiplier => CharacterData.BaseIceDamageMultiplier;
    
    /// <summary>
    /// The current lightning damage multiplier.
    /// </summary>
    public float CurrentLightningDamageMultiplier => CharacterData.BaseLightningDamageMultiplier;
    
    /// <summary>
    /// The current sanity damage multiplier.
    /// </summary>
    public float CurrentSanityDamageMultiplier => CharacterData.BaseSanityDamageMultiplier;
    
    /// <summary>
    /// The current disease damage multiplier.
    /// </summary>
    public float CurrentDiseaseDamageMultiplier => CharacterData.BaseDiseaseDamageMultiplier;

    /// <summary>
    /// The sprite that represents the character.
    /// </summary>
    private Sprite2D CharacterSprite { get; set; }

    /// <summary>
    /// A progress bar that represents the character's health.
    /// </summary>
    private TextureProgressBar HealthBar { get; set; }

    /// <summary>
    /// A dictionary of status effects applied to the character.
    /// The key is the status effect, and the value is the number of turns remaining.
    /// </summary>
    private Dictionary<StatusEffect, int> StatusEffects { get; set; } = new();

    public override void _Ready()
    {
        base._Ready();
        CurrentHealth = CharacterData.MaxHealth;
    }

    /// <summary>
    /// Set up the character with the given character data.
    /// This method initializes the character's health, sprite, and health bar.
    /// </summary>
    /// <param name="characterData"></param>
    public void Setup(CharacterData characterData)
    {
        CharacterData = characterData;
        CurrentHealth = characterData.MaxHealth;

        CharacterSprite = GetNode<Sprite2D>("CharacterSprite");
        HealthBar = GetNode<TextureProgressBar>("HealthBar");
        HealthBar.MaxValue = characterData.MaxHealth;
        HealthBar.Value = CurrentHealth;
        CharacterSprite.Texture = characterData.Image;
    }

    /// <summary>
    /// Damage the character. Take into account the weaknesses and resistances of the character.
    /// </summary>
    /// <param name="damage"></param>
    public void Damage(Damage damage)
    {
        if (damage.Amount <= 0)
        {
            return;
        }

        // TODO: make this more dynamic
        switch (damage.Type)
        {
            case DamageType.Physical:
                CurrentHealth -= damage.Amount * CurrentPhysicalDamageMultiplier;
                break;
            case DamageType.Dark:
                CurrentHealth -= damage.Amount * CurrentDarkDamageMultiplier;
                break;
            case DamageType.Light:
                CurrentHealth -= damage.Amount * CurrentLightDamageMultiplier;
                break;
            case DamageType.Fire:
                CurrentHealth -= damage.Amount * CurrentFireDamageMultiplier;
                break;
            case DamageType.Ice:
                CurrentHealth -= damage.Amount * CurrentIceDamageMultiplier;
                break;
            case DamageType.Lightning:
                CurrentHealth -= damage.Amount * CurrentLightningDamageMultiplier;
                break;
            case DamageType.Sanity:
                CurrentHealth -= damage.Amount * CurrentSanityDamageMultiplier;
                break;
            case DamageType.Disease:
                CurrentHealth -= damage.Amount * CurrentDiseaseDamageMultiplier;
                break;
        }

        UpdateHealthBar();
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Heal the character.
    /// </summary>
    /// <param name="amount"></param>
    public void Heal(float amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > CharacterData.MaxHealth)
        {
            CurrentHealth = CharacterData.MaxHealth;
        }

        UpdateHealthBar();
    }

    /// <summary>
    /// Apply a status effect to the character or increase the duration for the specified turns.
    /// </summary>
    public void ApplyEffect(StatusEffect effect, int turns)
    {
        if (StatusEffects.ContainsKey(effect))
        {
            StatusEffects[effect] += turns;
        }
        else
        {
            ManagerRepository.BattleLogManager.AddToLog($"{CharacterData.Name} is affected by {effect} for {turns} turns.");
            StatusEffects[effect] = turns;
        }
    }

    /// <summary>
    /// Check if the character has a specific status effect.
    /// </summary>
    public bool HasEffect(StatusEffect effect)
    {
        return StatusEffects.ContainsKey(effect);
    }

    /// <summary>
    /// Clear a specific status effect from the character.
    /// </summary>
    public void ClearEffect(StatusEffect effect)
    {
        if (StatusEffects.ContainsKey(effect))
        {
            StatusEffects.Remove(effect);
        }
    }

    /// <summary>
    /// Kill the character.
    /// This method should be called when the character's health reaches 0 or when some instant death effect is applied.
    /// </summary>
    private void Die()
    {
        ManagerRepository.BattleLogManager.AddToLog($"{CharacterData.Name} has died.");
    }

    /// <summary>
    /// Update the health bar to reflect the current health of the character.
    /// </summary>
    public void UpdateHealthBar()
    {
        var tween = CreateTween();
        // Animate the rotation smoothly over 0.2 seconds
        tween.TweenProperty(HealthBar, "value", CurrentHealth, .2f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
    }
}
