using System.Collections.Generic;

/// <summary>
/// DefaultSpellBehaviour is a default implementation of the ISpellBehaviour interface.
/// </summary>
public partial class DefaultSpellBehaviour : ISpellBehaviour
{
    public virtual float CalculateTotalDamage(List<Card> cards, SpellData spellData)
    {
        float modifier = 1;
        foreach (var card in cards)
        {
            modifier *= ((float)card.Rank) * spellData.ModifierMultiplier;
        }

        // Add all damage types together
        float totalDamage = 0;
        foreach (var damageType in spellData.DamageTypes)
        {
            totalDamage += CalculateDamage(damageType, modifier, spellData);
        }
        return totalDamage;
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

