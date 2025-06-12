using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// DefaultSpellBehaviour is a default implementation of the ISpellBehaviour interface.
/// It only uses damage types and modifiers to calculate the damage dealt by the spell, and does not apply any effects or heal.
/// </summary>
public partial class DefaultSpellBehaviour : ISpellBehaviour
{
    public virtual DamagePacket Resolve(List<Card> cards, ActionData spellData, List<Character> targets)
    {
        if (targets == null || targets.Count == 0)
        {
            GD.PrintErr("No targets selected.");
            return new DamagePacket();
        }

        // Calculate the modifier based on the cards selected
        float modifier = 1f;
        foreach (var card in cards)
        {
            modifier *= Mathf.Pow(spellData.ModifierMultiplier, (float)card.Rank);
        }

        // Add all damage types together
        List<Damage> damages = new();
        foreach (var damageType in spellData.DamageTypes)
        {
            float damage = CalculateDamage(damageType, modifier, spellData);
            damages.Add(new Damage(damage, damageType, targets.First()));
        }

        return new DamagePacket
        {
            Damages = damages,
        };
    }

    public DamagePacket Resolve(ActionData actionData, List<Character> targets)
    {
        return Resolve(new List<Card>(), actionData, targets);
    }

    /// <summary>
    /// Calculates the damage dealt by the spell based on the damage type, modifier, and spell data.
    /// </summary>
    protected float CalculateDamage(DamageType damageType, float modifier, ActionData spellData)
    {
        return damageType switch
        {
            DamageType.Physical => spellData.BasePhysicalDamage * modifier,
            DamageType.Dark => spellData.BaseDarkDamage * modifier,
            DamageType.Light => spellData.BaseLightDamage * modifier,
            DamageType.Fire => spellData.BaseFireDamage * modifier,
            DamageType.Ice => spellData.BaseIceDamage * modifier,
            DamageType.Lightning => spellData.BaseLightningDamage * modifier,
            DamageType.Sanity => spellData.BaseSanityDamage * modifier,
            DamageType.Disease => spellData.BaseDiseaseDamage * modifier,
            _ => 0
        };
    }
}

