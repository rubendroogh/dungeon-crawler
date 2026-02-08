using System.Collections.Generic;
using System.Threading.Tasks;
using DungeonRPG.Blessings.Enums;
using Godot;

/// <summary>
/// DefaultSpellBehaviour is a default implementation of the ISpellBehaviour interface.
/// It only uses damage types and modifiers to calculate the damage dealt by the spell, and does not apply any effects or heal.
/// </summary>
public partial class DefaultSpellBehaviour : ISpellBehaviour
{
    public async virtual Task<ResolveResult> Resolve(List<Blessing> blessings, ActionData spellData, Character target)
    {
        if (blessings == null || blessings.Count == 0)
        {
            GD.PrintErr("No blessings selected.");
            return new ResolveResult();
        }

        if (target == null || target.IsDead)
        {
            GD.PrintErr($"No legal target for spell {spellData.Name}.");
            return new ResolveResult();
        }

        // Add all damage types together
        List<Damage> damages = new();
        foreach (var damageType in spellData.DamageTypes)
        {
            float damage = CalculateDamage(damageType, 1f, spellData);
            damages.Add(new Damage(damage, damageType));
        }

        // Process keywords
        foreach (var keyword in spellData.Keywords)
        {
            // Apply keyword effects
            var keywordEffect = Keywords.GetKeywordBehaviour(keyword);
            if (keywordEffect != null)
            {
                _ = keywordEffect.OnCast(); // TODO: Handle async properly
            }
        }

        // Apply damage modifiers from keywords
        foreach (var keywordModifier in ActionManager.Instance.CastingContext.DamageModifiers)
        {
            foreach (var damage in damages)
            {
                damage.ApplyModifier(keywordModifier);
            }
        }

        return new ResolveResult
        {
            Damages = damages,
            Target = target
        };
    }

    public async virtual Task PreCastQueue()
    {
        // Empty on purpose
    }

    public async Task<ResolveResult> Resolve(ActionData actionData, Character target)
    {
        return await Resolve(new List<Blessing>(), actionData, target);
    }

    public async Task AnimateAction(ActionData spellData, Character target, Character caster = null)
    {
        if (spellData.CastEffectScene != null)
        {
            var effectInstance = spellData.CastEffectScene.Instantiate<BaseEffect>();
            if (spellData.DefaultCastEffectTexture != null)
            {
                effectInstance.SetSprite(spellData.DefaultCastEffectTexture);
            }

            target.AddChild(effectInstance);
            await effectInstance.Play(target);
        }
        else
        {
            GD.PushWarning($"No cast effect scene defined for spell {spellData.Name}.");
        }
    }

    public bool CanCast(Character caster, Spell spell, List<Blessing> blessings)
    {
        if (caster == null || spell == null || blessings == null)
        {
            GD.PrintErr("Invalid parameters for CanCast.");
            return false;
        }

        // Add up the mana from the selected blessings
        var manaCounts = new Dictionary<Domain, int>();
        foreach (var blessing in blessings)
        {
            if (manaCounts.ContainsKey(blessing.Domain))
            {
                manaCounts[blessing.Domain]++;
            }
            else
            {
                manaCounts[blessing.Domain] = 1;
            }
        }

        // Check if all costs are covered
        foreach (var cost in spell.Data.Cost.Costs)
        {
            if (!manaCounts.ContainsKey(cost.Type) || manaCounts[cost.Type] < cost.Amount)
            {
                GD.PrintErr($"Not enough {cost.Type} mana to cast spell {spell.Data.Name}. Required: {cost.Amount}, available: {(manaCounts.ContainsKey(cost.Type) ? manaCounts[cost.Type] : 0)}.");
                return false;
            }
        }

        return true;
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
