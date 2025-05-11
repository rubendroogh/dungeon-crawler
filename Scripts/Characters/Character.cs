using System.Collections.Generic;
using Godot;

/// <summary>
/// An entity that is a participant in a battle on either side.
/// </summary>
public partial class Character : Node
{
    public float CurrentHealth { get; private set; }

    [Export]
    public CharacterData CharacterData { get; set; }

    private Sprite2D CharacterSprite { get; set; }

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
                CurrentHealth -= damage.Amount * CharacterData.BasePhysicalDamageMultiplier;
                break;
            case DamageType.Dark:
                CurrentHealth -= damage.Amount * CharacterData.BaseDarkDamageMultiplier;
                break;
            case DamageType.Light:
                CurrentHealth -= damage.Amount * CharacterData.BaseLightDamageMultiplier;
                break;
            case DamageType.Fire:
                CurrentHealth -= damage.Amount * CharacterData.BaseFireDamageMultiplier;
                break;
            case DamageType.Ice:
                CurrentHealth -= damage.Amount * CharacterData.BaseIceDamageMultiplier;
                break;
            case DamageType.Lightning:
                CurrentHealth -= damage.Amount * CharacterData.BaseLightningDamageMultiplier;
                break;
            case DamageType.Sanity:
                CurrentHealth -= damage.Amount * CharacterData.BaseSanityDamageMultiplier;
                break;
            case DamageType.Disease:
                CurrentHealth -= damage.Amount * CharacterData.BaseDiseaseDamageMultiplier;
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
    /// Apply a status effect to the character or increase the duration.
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="turns"></param>
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

    public bool HasEffect(StatusEffect effect)
    {
        return StatusEffects.ContainsKey(effect);
    }

    public void ClearEffect(StatusEffect effect)
    {
        if (StatusEffects.ContainsKey(effect))
        {
            StatusEffects.Remove(effect);
        }
    }

    private void Die()
    {
        ManagerRepository.BattleLogManager.AddToLog($"{CharacterData.Name} has died.");
    }

    public void UpdateHealthBar()
    {
        HealthBar.Value = CurrentHealth;
    }
}
