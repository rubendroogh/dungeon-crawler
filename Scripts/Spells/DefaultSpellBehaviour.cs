using System.Collections.Generic;
using Godot;

/// <summary>
/// DefaultSpellBehaviour is a default implementation of the ISpellBehaviour interface.
/// It only uses damage types and modifiers to calculate the damage dealt by the spell, and does not apply any effects or heal.
/// </summary>
public partial class DefaultSpellBehaviour : ISpellBehaviour
{
    public virtual SpellCastResult Cast(List<Card> cards, SpellData spellData, Character target)
    {
        // Caoculate the modifier based on the cards selected
        float modifier = 1;
        foreach (var card in cards)
        {
            modifier *= Mathf.Pow(spellData.ModifierMultiplier, (float)card.Rank);
        }

        GD.Print("Modifier: " + modifier);

        // Add all damage types together
        List<Damage> damages = new();
        foreach (var damageType in spellData.DamageTypes)
        {
            float damage = CalculateDamage(damageType, modifier, spellData);
            damages.Add(new Damage(damage, damageType, target));
        }

        return new SpellCastResult
        {
            Damages = damages,
        };
    }

    protected float CalculateDamage(DamageType damageType, float modifier, SpellData spellData)
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

