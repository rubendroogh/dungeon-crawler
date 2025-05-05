using System.Collections.Generic;
using Godot;

public partial class FireBallBehaviour : DefaultSpellBehaviour
{
    public override float CalculateTotalDamage(List<Card> cards, SpellData spellData)
    {
        float modifier = 1;
        foreach (var card in cards)
        {
            modifier *= ((float)card.Rank) * spellData.ModifierMultiplier;
        }

        return spellData.BasePhysicalDamage * modifier;
    }
}
