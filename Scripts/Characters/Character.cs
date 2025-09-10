using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    /// A boolean indicating if the character is dead.
    /// </summary>
    public bool IsDead { get; set; } = false;

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
    /// The action queue for the character.
    /// This queue contains the actions that the character will perform in the next damage phase.
    /// </summary>
    public List<ActionQueueEntry> ActionQueue { get; set; } = new();

    /// <summary>
    /// The sprite that represents the character.
    /// </summary>
    protected Sprite2D CharacterSprite { get; set; }

    /// <summary>
    /// A progress bar that represents the character's health.
    /// </summary>
    protected TextureProgressBar HealthBar { get; set; }

    /// <summary>
    /// A label that shows the status effects applied to the character.
    /// </summary>
    private Label StatusEffectLabel { get; set; }

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
    public async Task Setup(CharacterData characterData)
    {
        SetCharacterData(characterData);
        InitializeNodes(characterData);
        await UpdateHealthBar();
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
        // TODO: Fix error in this method
        if (StatusEffects == null || StatusEffects.Count == 0)
        {
            return;
        }

        var expiredStatusEffects = new List<StatusEffect>();
        foreach (var effect in StatusEffects)
        {
            effect.Duration--;
            if (effect.Duration <= 0)
            {
                expiredStatusEffects.Add(effect);
            }
        }

        foreach (var effect in expiredStatusEffects)
        {
            ClearEffect(effect.Type);
        }

        // Process the effects at the end of the turn
        // Maybe move this up?
        foreach (var effect in StatusEffects)
        {
            effect.Behaviour.ProcessEffectEndTurn(this);
        }

        UpdateStatusEffectLabel();
    }

    /// <summary>
    /// Damage the character. Takes into account the weaknesses and resistances of the character.
    /// </summary>
    /// <param name="damage">The damage to apply.</param>
    public async Task<int> Damage(ResolveResult damage)
    {
        if (damage.TotalBaseAmount <= 0 || CurrentHealth <= 0)
        {
            return 0;
        }

        var totalDamage = 0f;
        foreach (var dmg in damage.Damages)
        {
            if (dmg.StatusEffect != StatusEffectType.None)
            {
                ApplyEffect(dmg.StatusEffect, 2);
            }

            totalDamage += GetModifiedDamage(dmg);
        }

        // Apply the total damage to the character's health
        CurrentHealth -= totalDamage;

        GD.Print($"{CharacterData.Name} takes {totalDamage} damage. Current health: {CurrentHealth}/{CharacterData.MaxHealth}");

        await UpdateHealthBar();
        if (CurrentHealth <= 0)
        {
            await Die();
        }

        return (int)totalDamage;
    }

    /// <summary>
    /// Heal the character.
    /// </summary>
    /// <param name="amount"></param>
    public async Task Heal(float amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > CharacterData.MaxHealth)
        {
            CurrentHealth = CharacterData.MaxHealth;
        }

        await UpdateHealthBar();
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
        }
        else
        {
            var newEffect = new StatusEffect(turns, effect);
            StatusEffects.Add(newEffect);
            newEffect.Behaviour.ProcessEffectOnApply(this);
        }

        UpdateStatusEffectLabel();
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

        UpdateStatusEffectLabel();
    }

    /// <summary>
    /// Resolves the actions in the character's action queue and returns the resulting damage packet.
    /// </summary>
    /// <returns>The damage packets containing the results of the resolved actions.</returns>
    public async virtual Task ResolveQueue()
    {
        foreach (var actionQueueEntry in ActionQueue)
        {
            actionQueueEntry.Action.GetBehaviour().Resolve(actionQueueEntry.Action.Data, [actionQueueEntry.Target]);
        }
    }

    /// <summary>
    /// Get the modified damage for a specific damage type.
    /// </summary>
    public float GetModifiedDamage(Damage baseDamage)
    {
        if (baseDamage == null)
        {
            GD.PrintErr("BaseDamage is null");
            return 0f;
        }

        // Get damage type and modifiers for that damage
        var damageType = baseDamage.Type;
        switch (damageType)
        {
            case DamageType.Physical:
                return CalculateModifiedDamage(baseDamage.Amount, PhysicalDamageModifiers);
            case DamageType.Dark:
                return CalculateModifiedDamage(baseDamage.Amount, DarkDamageModifiers);
            case DamageType.Light:
                return CalculateModifiedDamage(baseDamage.Amount, LightDamageModifiers);
            case DamageType.Fire:
                return CalculateModifiedDamage(baseDamage.Amount, FireDamageModifiers);
            case DamageType.Ice:
                return CalculateModifiedDamage(baseDamage.Amount, IceDamageModifiers);
            case DamageType.Lightning:
                return CalculateModifiedDamage(baseDamage.Amount, LightningDamageModifiers);
            case DamageType.Sanity:
                return CalculateModifiedDamage(baseDamage.Amount, SanityDamageModifiers);
            case DamageType.Disease:
                return CalculateModifiedDamage(baseDamage.Amount, DiseaseDamageModifiers);
            default:
                GD.PrintErr($"Unhandled damage type: {damageType}");
                return baseDamage.Amount;
        }
    }

    /// <summary>
    /// Sets up the character's sprite, health bar, and status effect label.
    /// </summary>
    protected virtual void InitializeNodes(CharacterData characterData)
    {
        CharacterSprite = GetNode<Sprite2D>("CharacterSprite");
        HealthBar = GetNode<TextureProgressBar>("HealthBar");
        StatusEffectLabel = GetNode<Label>("StatusEffectsLabel");

        if (CharacterSprite == null || HealthBar == null || StatusEffectLabel == null)
        {
            GD.PrintErr("CharacterSprite, HealthBar, or StatusEffectLabel is not set up in the scene.");
            return;
        }

        HealthBar.MaxValue = characterData.MaxHealth;
        HealthBar.Value = CurrentHealth;
        CharacterSprite.Texture = characterData.Image;
    }

    /// <summary>
    /// Kill the character.
    /// This method should be called when the character's health reaches 0 or when some instant death effect is applied.
    /// </summary>
    protected async virtual Task Die()
    {
        Managers.BattleLogManager.Log($"{CharacterData.Name} has died.");
        IsDead = true;

        await PlayDeathAnimation();
    }

    /// <summary>
    /// Plays the damage animation for the character.
    /// </summary>
    protected async virtual Task PlayDamageAnimation()
    {
        // Damage animation should be handled in derived classes
    }

    /// <summary>
    /// Plays the death animation for the character.
    /// </summary>
    protected async virtual Task PlayDeathAnimation()
    {
        // Death animation should be handled in derived classes
    }

    /// <summary>
    /// Update the health bar to reflect the current health of the character.
    /// </summary>
    protected virtual async Task UpdateHealthBar()
    {
        var tween = CreateTween();
        // Animate the rotation smoothly over 0.2 seconds
        tween.TweenProperty(HealthBar, "value", CurrentHealth, .2f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);

        await ToSignal(tween, "finished");
    }

    /// <summary>
    /// Update the status effect label to show the current status effects and their durations.
    /// </summary>
    protected virtual void UpdateStatusEffectLabel()
    {
        if (StatusEffectLabel == null)
        {
            GD.PrintErr("StatusEffectLabel is not set up in the scene.");
            return;
        }

        StatusEffectLabel.Text = string.Join(", ", StatusEffects.Select(e => $"{e.Type} ({e.Duration})"));
        if (StatusEffects.Count == 0)
        {
            StatusEffectLabel.Text = "";
        }
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
    /// Set the character data for this character.
    /// </summary>
    /// <param name="characterData"></param>
    private void SetCharacterData(CharacterData characterData)
    {
        CharacterData = characterData;
        CurrentHealth = characterData.MaxHealth;

        // Reset the damage modifiers to the base values
        PhysicalDamageModifiers.Clear();
        DarkDamageModifiers.Clear();
        LightDamageModifiers.Clear();
        FireDamageModifiers.Clear();
        IceDamageModifiers.Clear();
        LightningDamageModifiers.Clear();
        SanityDamageModifiers.Clear();
        DiseaseDamageModifiers.Clear();

        PhysicalDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, CharacterData.BasePhysicalDamageMultiplier));
        DarkDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, CharacterData.BaseDarkDamageMultiplier));
        LightDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, CharacterData.BaseLightDamageMultiplier));
        FireDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, CharacterData.BaseFireDamageMultiplier));
        IceDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, CharacterData.BaseIceDamageMultiplier));
        LightningDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, CharacterData.BaseLightningDamageMultiplier));
        SanityDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, CharacterData.BaseSanityDamageMultiplier));
        DiseaseDamageModifiers.Add(new DamageModifier(DamageModifierType.Multiplicative, CharacterData.BaseDiseaseDamageMultiplier));
    }
}
