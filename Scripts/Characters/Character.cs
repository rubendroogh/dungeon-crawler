using System.Collections.Generic;
using System.Linq;
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
    /// A boolean indicating if the character is a player.
    /// </summary>
    public bool IsPlayer { get; set; } = false;

    /// <summary>
    /// The current physical damage modifiers.
    /// It is a list of damage modifiers that is applied to the character's physical damage.
    /// </summary>
    public List<DamageModifier> PhysicalDamageModifiers { get; set; } = new();

    /// <summary>
    /// The current dark damage modifiers.
    /// It is a list of damage modifiers that is applied to the character's dark damage.
    /// </summary>
    public List<DamageModifier> DarkDamageModifiers { get; set; } = new();

    /// <summary>
    /// The current light damage modifiers.
    /// It is a list of damage modifiers that is applied to the character's light damage.
    /// </summary>
    public List<DamageModifier> LightDamageModifiers { get; set; } = new();

    /// <summary>
    /// The current fire damage modifiers.
    /// It is a list of damage modifiers that is applied to the character's fire damage.
    /// </summary>
    public List<DamageModifier> FireDamageModifiers { get; set; } = new();

    /// <summary>
    /// The current ice damage modifiers.
    /// It is a list of damage modifiers that is applied to the character's ice damage.
    /// </summary>
    public List<DamageModifier> IceDamageModifiers { get; set; } = new();

    /// <summary>
    /// The current lightning damage modifiers.
    /// It is a list of damage modifiers that is applied to the character's lightning damage.
    /// </summary>
    public List<DamageModifier> LightningDamageModifiers { get; set; } = new();

    /// <summary>
    /// The current sanity damage modifiers.
    /// It is a list of damage modifiers that is applied to the character's sanity damage.
    /// </summary>
    public List<DamageModifier> SanityDamageModifiers { get; set; } = new();

    /// <summary>
    /// The current disease damage modifiers.
    /// It is a list of damage modifiers that is applied to the character's disease damage.
    /// </summary>
    public List<DamageModifier> DiseaseDamageModifiers { get; set; } = new();

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
    private List<StatusEffect> StatusEffects { get; set; } = new();

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

        // Set the current damage multipliers to the initial values
        PhysicalDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, CharacterData.BasePhysicalDamageMultiplier));
        DarkDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, CharacterData.BaseDarkDamageMultiplier));
        LightDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, CharacterData.BaseLightDamageMultiplier));
        FireDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, CharacterData.BaseFireDamageMultiplier));
        IceDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, CharacterData.BaseIceDamageMultiplier));
        LightningDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, CharacterData.BaseLightningDamageMultiplier));
        SanityDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, CharacterData.BaseSanityDamageMultiplier));
        DiseaseDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, CharacterData.BaseDiseaseDamageMultiplier));
    }

    /// <summary>
    /// Process the effects at the start of the opponent's turn.
    /// </summary>
    public void StartTurn()
    {
        // Process the effects at the start of the turn
        foreach (var effect in StatusEffects)
        {
            effect.Behaviour.ProcessEffectStartTurn(this);
        }
    }

    /// <summary>
    /// Process the effects at the end of the opponent's turn.
    /// </summary>
    public void EndTurn()
    {
        foreach (var effect in StatusEffects)
        {
            effect.Duration--;
            if (effect.Duration <= 0)
            {
                ClearEffect(effect.Type);
            }
        }

        // Process the effects at the end of the turn
        foreach (var effect in StatusEffects)
        {
            effect.Behaviour.ProcessEffectEndTurn(this);
        }
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
                CurrentHealth -= CalculateModifiedDamage(damage.Amount, PhysicalDamageModifiers);
                break;
            case DamageType.Dark:
                CurrentHealth -= CalculateModifiedDamage(damage.Amount, DarkDamageModifiers);
                break;
            case DamageType.Light:
                CurrentHealth -= CalculateModifiedDamage(damage.Amount, LightDamageModifiers);
                break;
            case DamageType.Fire:
                CurrentHealth -= CalculateModifiedDamage(damage.Amount, FireDamageModifiers);
                break;
            case DamageType.Ice:
                CurrentHealth -= CalculateModifiedDamage(damage.Amount, IceDamageModifiers);
                break;
            case DamageType.Lightning:
                CurrentHealth -= CalculateModifiedDamage(damage.Amount, LightningDamageModifiers);
                break;
            case DamageType.Sanity:
                CurrentHealth -= CalculateModifiedDamage(damage.Amount, SanityDamageModifiers);
                break;
            case DamageType.Disease:
                CurrentHealth -= CalculateModifiedDamage(damage.Amount, DiseaseDamageModifiers);
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
    public void ApplyEffect(StatusEffectType effect, int turns)
    {
        if (HasEffect(effect))
        {
            var existingEffect = StatusEffects.Find(e => e.Type == effect);
            existingEffect.Duration += turns;
            ManagerRepository.BattleLogManager.AddToLog($"{CharacterData.Name} is now affected by {effect} for {existingEffect.Duration} turns.");
        }
        else
        {
            var newEffect = new StatusEffect(turns, effect);
            StatusEffects.Add(newEffect);
            newEffect.Behaviour.ProcessEffectOnApply(this);
            ManagerRepository.BattleLogManager.AddToLog($"{CharacterData.Name} is now affected by {effect} for {newEffect.Duration} turns.");
        }
    }

    /// <summary>
    /// Check if the character has a specific status effect.
    /// </summary>
    public bool HasEffect(StatusEffectType effect)
    {
        return StatusEffects.Find(e => e.Type == effect) != null;
    }

    /// <summary>
    /// Clear a specific status effect from the character.
    /// </summary>
    public void ClearEffect(StatusEffectType effect)
    {
        var existingEffect = StatusEffects.Find(e => e.Type == effect);
        if (existingEffect == null)
        {
            return;
        }
        
        existingEffect.Behaviour.ProcessEffectOnRemove(this);
        StatusEffects.Remove(existingEffect);
        ManagerRepository.BattleLogManager.AddToLog($"{CharacterData.Name} is no longer affected by {effect}.");
    }

    /// <summary>
    /// Calculate the modified damage based on the base damage and the list of damage modifiers.
    /// </summary>
    private float CalculateModifiedDamage(float baseDamage, List<DamageModifier> modifiers)
    {
        float modifiedDamage = baseDamage;

        foreach (var modifier in modifiers)
        {
            switch (modifier.Type)
            {
                case DamageModifierType.Additive:
                    modifiedDamage += modifier.Value;
                    break;
                case DamageModifierType.Multiplicative:
                    modifiedDamage *= modifier.Value;
                    break;
                case DamageModifierType.Percentage:
                    modifiedDamage *= 1 + modifier.Value / 100f;
                    break;
            }
        }

        return modifiedDamage;
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
    private void UpdateHealthBar()
    {
        var tween = CreateTween();
        // Animate the rotation smoothly over 0.2 seconds
        tween.TweenProperty(HealthBar, "value", CurrentHealth, .2f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
    }
}
