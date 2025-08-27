using Godot;
using System.Collections.Generic;
using System.Linq;

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
    public DamagePacket Resolve(ActionData actionData, List<Character> targets)
    {
        if (targets == null || targets.Count == 0)
        {
            GD.PrintErr("No targets selected.");
            return new DamagePacket();
        }

        // Add all damage types together
        List<Damage> damages = new();
        foreach (var damageType in actionData.DamageTypes)
        {
            float damage = GetDamage(damageType, actionData);
            damages.Add(new Damage(damage, damageType));
        }

        return new DamagePacket
        {
            Damages = damages,
            Target = targets.First()
        };
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
