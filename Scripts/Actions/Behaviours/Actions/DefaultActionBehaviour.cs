using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// DefaultActionBehaviour is a default implementation of the IActionBehaviour interface.
/// It just directly uses the damage types and modifiers to calculate the damage dealt by the action.
/// </summary>
public partial class DefaultActionBehaviour : IActionBehaviour
{
    /// <summary>
    /// Resolves the action data and returns a damage packet for the action.
    /// </summary>
    /// <param name="actionData">The action data containing the spell and target.</param>
    /// <param name="targets">The list of targets for the action.</param>
    /// <returns>A damage packet representing the result of the action.</returns>
    public ResolveResult Resolve(ActionData actionData, Character target)
    {
        if (target == null || target.IsDead)
        {
            GD.PrintErr("No legal target for action.");
            return new ResolveResult();
        }

        // Add all damage types together
        List<Damage> damages = new();
        foreach (var damageType in actionData.DamageTypes)
        {
            float damage = GetDamage(damageType, actionData);
            damages.Add(new Damage(damage, damageType));
        }

        // Process keywords
        foreach (var keyword in actionData.Keywords)
        {
            // Apply keyword effects
            var keywordEffect = Keywords.GetKeywordBehaviour(keyword);
            if (keywordEffect != null)
            {
                _ = keywordEffect.OnCast(); // TODO: Handle async properly
            }
        }

        return new ResolveResult
        {
            Damages = damages,
            Target = target
        };
    }

    /// <summary>
    /// Animates the spell cast for the given target.
    /// </summary>
    public virtual async Task AnimateAction(ActionData spellData, Character target, Character caster = null)
    {
        // If this method is called from a spell, it will get overridden in DefaultSpellBehaviour
        // So for now we can just assume the target is the player since an action can only be cast by an AI.
        if (target == null || target.IsDead)
        {
            GD.PrintErr("No legal target for action.");
            return;
        }

        if (caster == null)
        {
            return;
        }
            
        await caster.Delay(300);

        var originalPosition = caster.Position;
        var targetPosition = originalPosition + new Vector2(0, 10);

        var tween = caster.CreateTween();
        tween.TweenProperty(caster, "position", targetPosition, 0.1f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(caster, "position", originalPosition, 0.1f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.In);

        await caster.ToSignal(tween, "finished");
        await caster.Delay(300);
    }

    /// <summary>
    /// Gets the base damage for a specific damage type from the spell data.
    /// </summary>
    protected float GetDamage(DamageType damageType, ActionData spellData)
    {
        return damageType switch
        {
            DamageType.Physical => spellData.BasePhysicalDamage,
            DamageType.Dark => spellData.BaseDarkDamage,
            DamageType.Light => spellData.BaseLightDamage,
            DamageType.Fire => spellData.BaseFireDamage,
            DamageType.Ice => spellData.BaseIceDamage,
            DamageType.Lightning => spellData.BaseLightningDamage,
            DamageType.Sanity => spellData.BaseSanityDamage,
            DamageType.Disease => spellData.BaseDiseaseDamage,
            _ => 0
        };
    }
}
