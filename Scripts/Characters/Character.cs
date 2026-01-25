using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// An entity that is a participant in a battle on either side.
/// </summary>
public partial class Character : Node2D
{
    /// <summary>
    /// The current health of the character.
    /// This value is between 0 and the max health of the character.
    /// </summary>
    public float Health { get; private set; }

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
    public Queue<ActionQueueEntry> ActionQueue { get; protected set; } = new();

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
    /// The label that shows the character's name.
    /// </summary>
    private RichTextLabel CharacterName { get; set; }

    /// <summary>
    /// A dictionary of status effects applied to the character.
    /// The key is the status effect, and the value is the number of turns remaining.
    /// </summary>
    private List<StatusEffect> StatusEffects { get; set; } = new();

    public override void _Ready()
    {
        base._Ready();
        Health = CharacterData.MaxHealth;
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
    public async Task StartTurn()
    {
        // Process the effects at the start of the turn
        foreach (var effect in StatusEffects)
        {
            await effect.Behaviour.ProcessEffectStartTurn(this);
        }
    }

    /// <summary>
    /// Process the effects at the end of the opponent's turn.
    /// </summary>
    public async Task EndTurn()
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
            await ClearEffect(effect.Type);
        }

        // Process the effects at the end of the turn
        // Maybe move this up?
        foreach (var effect in StatusEffects)
        {
            await effect.Behaviour.ProcessEffectEndTurn(this);
        }

        UpdateStatusEffectLabel();
    }

    /// <summary>
    /// Checks if the character has the value for the given personality trait.
    /// </summary>
    public bool CheckPersonalityTrait(int valueToCheck, PersonalityTraitType trait)
    {
        if (CharacterData == null)
        {
            return false;
        }

        // TODO: Add support for less than checks.
        // TODO: Add support for temporary trait modifiers.
        return CharacterData.GetBasePersonalityTraitValue(trait) >= valueToCheck;
    }

    /// <summary>
    /// Damage the character using a single damage instance.
    /// Takes into account the weaknesses and resistances of the character.
    /// </summary>
    /// <param name="damage">The damage to apply.</param>
    /// <returns>The total damage applied after modifiers</returns>
    public async Task<int> Damage(Damage damage)
    {
        var resolveResult = new ResolveResult();
        resolveResult.Damages.Add(damage);

        return await Damage(resolveResult);
    }

    /// <summary>
    /// Damage the character. Takes into account the weaknesses and resistances of the character.
    /// </summary>
    /// <param name="damage">The damage to apply.</param>
    /// <returns>The total damage applied after modifiers</returns>
    public async Task<int> Damage(ResolveResult damage)
    {
        if (damage.TotalDamageAmount <= 0 || Health <= 0)
        {
            return 0;
        }

        var totalDamage = 0f;
        foreach (var dmg in damage.Damages)
        {
            if (dmg.StatusEffect != StatusEffectType.None)
            {
                await ApplyEffect(dmg.StatusEffect, 2);
            }

            ApplyModifiedDamage(dmg);
            totalDamage += dmg.Amount;
        }

        // If total damage is 0, do nothing.
        if (totalDamage <= 0)
        {
            return 0;
        }

        // Apply the total damage to the character's health
        // And show animations
        Health -= totalDamage;
        await PlayDamageAnimation();
        await UpdateHealthBar();
        if (Health <= 0)
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
        Health += amount;
        if (Health > CharacterData.MaxHealth)
        {
            Health = CharacterData.MaxHealth;
        }

        await UpdateHealthBar();
    }

    /// <summary>
    /// Apply a status effect to the character or increase the duration for the specified turns.
    /// </summary>
    public async Task ApplyEffect(StatusEffectType effect, int turns)
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
            await newEffect.Behaviour.ProcessEffectOnApply(this);
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
    public async Task ClearEffect(StatusEffectType effect)
    {
        var existingEffect = StatusEffects.Find(e => e.Type == effect);
        if (existingEffect == null)
        {
            return;
        }

        await existingEffect.Behaviour.ProcessEffectOnRemove(this);
        StatusEffects.Remove(existingEffect);

        UpdateStatusEffectLabel();
    }

    /// <summary>
    /// Resolves the actions in the character's action queue, executing all queued actions, applying their effects, and showing animations.
    /// </summary>
    public async Task ResolveQueue()
    {
        if (ActionQueue.Count == 0)
        {
            GD.PrintErr("No actions in queue to resolve.");
            return;
        }

		Managers.ActionManager.KeywordContext.ResetKeywordContext();

        // Process each action in the queue in order.
        while (ActionQueue.Count > 0)
        {
            var entry = ActionQueue.Dequeue();
            if (entry.Target.IsDead)
            {
                GD.Print("Target is dead, ending queue.");
                ActionQueue.Clear();
                break;
            }

            await ResolveQueueEntry(entry);
            
		    Managers.SpellQueueManager.UpdateSpellQueue();
        }

        // Reset mana after resolving.
        Managers.ManaSourceManager.ResetAllMana();
    }

    /// <summary>
    /// Uses the current active damage modifiers to calculate the modified damage for the given base damage.
    /// </summary>
    public void ApplyModifiedDamage(Damage baseDamage)
    {
        if (baseDamage == null)
        {
            GD.PrintErr("BaseDamage is null");
            return;
        }

        // Get damage type and modifiers for that damage
        var damageType = baseDamage.Type;
        switch (damageType)
        {
            case DamageType.Physical:
                baseDamage.ApplyModifiers(PhysicalDamageModifiers);
                break;
            case DamageType.Dark:
                baseDamage.ApplyModifiers(DarkDamageModifiers);
                break;
            case DamageType.Light:
                baseDamage.ApplyModifiers(LightDamageModifiers);
                break;
            case DamageType.Fire:
                baseDamage.ApplyModifiers(FireDamageModifiers);
                break;
            case DamageType.Ice:
                baseDamage.ApplyModifiers(IceDamageModifiers);
                break;
            case DamageType.Lightning:
                baseDamage.ApplyModifiers(LightningDamageModifiers);
                break;
            case DamageType.Sanity:
                baseDamage.ApplyModifiers(SanityDamageModifiers);
                break;
            case DamageType.Disease:
                baseDamage.ApplyModifiers(DiseaseDamageModifiers);
                break;
            default:
                GD.PrintErr($"Unhandled damage type: {damageType}");
                break;
        }
    }

    /// <summary>
    /// Plays the damage animation for the character.
    /// </summary>
    public async virtual Task PlayDamageAnimation()
    {
        // Damage animation should be handled in derived classes
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Sets up the character's sprite, health bar, and status effect label.
    /// </summary>
    protected virtual void InitializeNodes(CharacterData characterData)
    {
        CharacterSprite = GetNode<Sprite2D>("CharacterSprite");
        HealthBar = GetNode<TextureProgressBar>("HealthBar");
        StatusEffectLabel = GetNode<Label>("StatusEffectsLabel");
        CharacterName = GetNode<RichTextLabel>("CharacterName");

        if (CharacterSprite == null || HealthBar == null || StatusEffectLabel == null || CharacterName == null)
        {
            GD.PrintErr("CharacterSprite, HealthBar, StatusEffectLabel, or CharacterName is not set up in the scene.");
            return;
        }

        HealthBar.MaxValue = characterData.MaxHealth;
        HealthBar.Value = Health;
        CharacterSprite.Texture = characterData.Image;
        CharacterName.Text = characterData.Name;
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
    /// Plays the death animation for the character.
    /// </summary>
    protected async virtual Task PlayDeathAnimation()
    {
        // Death animation should be handled in derived classes
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Update the health bar to reflect the current health of the character.
    /// </summary>
    protected virtual async Task UpdateHealthBar()
    {
        var tween = CreateTween();
        // Animate the change smoothly over 0.2 seconds
        tween.TweenProperty(HealthBar, "value", Health, .2f)
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
    /// Resolve a single entry in the action queue, executing the action, applying its effects, and showing animations.
    /// </summary>
    protected async Task ResolveQueueEntry(ActionQueueEntry entry)
    {
        if (entry.Action == null || entry.Target == null)
        {
            GD.PrintErr("Invalid action or target in action queue.");
            return;
        }

        // Update the keyword context for this action.
        Managers.ActionManager.KeywordContext.UpdateKeywordContext(entry.Action, this, entry.Target);

        await this.Delay(300);
        ResolveResult actionResolveResult;
        IActionBehaviour actionBehaviour;

        // Resolve the spell or action
        if (entry.Action is Spell spell)
        {
            actionBehaviour = spell.GetBehaviour();
            actionResolveResult = await (actionBehaviour as ISpellBehaviour).Resolve(entry.Blessings, spell.Data, entry.Target);
        }
        else
        {
            actionBehaviour = entry.Action.GetBehaviour();
            actionResolveResult = await actionBehaviour.Resolve(entry.Action.Data, entry.Target);
        }

        // Animate the action and target damage
        await actionBehaviour.AnimateAction(entry.Action.Data, entry.Target, this);

        // Apply the resolve result (damage, healing, status effects, etc.)
        await Managers.ActionManager.ApplyResolveResult(actionResolveResult);
        await this.Delay(300);

        Managers.BattleManager.CastSpellsThisTurn++;
    }

    /// <summary>
    /// Set the character data for this character.
    /// </summary>
    /// <param name="characterData"></param>
    private void SetCharacterData(CharacterData characterData)
    {
        CharacterData = characterData;
        Health = characterData.MaxHealth;

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
