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
    public ResolveResult Resolve(ActionData actionData, List<Character> targets)
    {
        if (targets == null || targets.Count == 0)
        {
            GD.PrintErr("No targets selected.");
            return new ResolveResult();
        }

        // Add all damage types together
        List<Damage> damages = new();
        foreach (var damageType in actionData.DamageTypes)
        {
            float damage = GetDamage(damageType, actionData);
            damages.Add(new Damage(damage, damageType));
        }

        return new ResolveResult
        {
            Damages = damages,
            Target = targets.First()
        };
    }

    /// <summary>
    /// Animates the spell cast for the given targets.
    /// </summary>
    public async Task AnimateSpellCast(ActionData spellData, List<Character> targets)
    {
        // If this method is called from a spell, it will get overridden in DefaultSpellBehaviour
        // So for now we can just assume the target is the player since an action can only be cast by an AI.
        foreach (var target in targets)
        {
            // Do a screen shake
            // TODO: Move screen shake to player class, since it's the player that reacts to the spell cast
            // We can use this place to do the enemy's animation
        }
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
