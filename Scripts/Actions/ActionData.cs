using System.Collections.Generic;
using Godot;
using Godot.Collections;

[GlobalClass] // Makes it creatable from the "New Resource" menu
public partial class ActionData : Resource
{
    /// <summary>
    /// The name of the spell.
    /// </summary>
    [Export]
    [ExportGroup("Basic spell info")]
    public string Name { get; set; } = "Default spell";

    /// <summary>
    /// The description of the spell in the spellbook.
    /// </summary>
    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; } = "You forgot to fill this in!";

    /// <summary>
    /// The image representing the spell in the spellbook.
    /// </summary>
    [Export]
    public Texture2D Image { get; set; }

    /// <summary>
    /// The max amount of mana charges this spell can have.
    /// </summary>
    [Export]
    public int MaxManaCharges { get; set; } = 4;

    /// <summary>
    /// The base physical damage dealt by the spell.
    /// </summary>
    [Export]
    [ExportGroup("Spell damage info")]
    public float BasePhysicalDamage { get; set; } = 100;
    
    /// <summary>
    /// The base dark damage dealt by the spell.
    /// </summary>
    [Export]
    public float BaseDarkDamage { get; set; } = 0;

    /// <summary>
    /// The base light damage dealt by the spell.
    /// </summary>
    [Export]
    public float BaseLightDamage { get; set; } = 0;

    /// <summary>
    /// The base fire damage dealt by the spell.
    /// </summary>
    [Export]
    public float BaseFireDamage { get; set; } = 0;

    /// <summary>
    /// The base ice damage dealt by the spell.
    /// </summary>
    [Export]
    public float BaseIceDamage { get; set; } = 0;

    /// <summary>
    /// The base lightning damage dealt by the spell.
    /// </summary>
    [Export]
    public float BaseLightningDamage { get; set; } = 0;

    /// <summary>
    /// The base sanity damage dealt by the spell.
    /// </summary>
    [Export]
    public float BaseSanityDamage { get; set; } = 0;

    /// <summary>
    /// The base disease damage dealt by the spell.
    /// </summary>
    [Export]
    public float BaseDiseaseDamage { get; set; } = 0;

    /// <summary>
    /// A list of keywords that modify the behavior of the spell.
    /// </summary>
    [Export]
    [ExportGroup("Advanced spell info")]
    public Array<Keyword> Keywords { get; set; } = new Array<Keyword>();

    /// <summary>
    /// This multiplier affects the effect of mana power (the card rank) on the spell damage.
    /// </summary>
    [Export]
    public float ModifierMultiplier = 1.01f;

    /// <summary>
    /// The scene containing the animation played when the spell is cast.
    /// </summary>
    [Export]
    public PackedScene CastEffectScene { get; set; }

    /// <summary>
    /// If <see cref="CastEffectScene"/> is not set, this texture will override the cast effect sprite.
    /// </summary>
    [Export]
    public Texture2D DefaultCastEffectTexture { get; set; }

    /// <summary>
    /// A value between 1 and 5 that indicates the rarity of the spell.
    /// Signifies both power, potential, and complexity.
    /// TODO: Not implemented yet.
    /// </summary>
    [Export]
    public int Rarity = 1;

    /// <summary>
    /// A list of all the damage types that this spell can deal.
    /// </summary>
    public DamageType[] DamageTypes { get {
        var damageTypes = new List<DamageType>();
        if (BasePhysicalDamage > 0) damageTypes.Add(DamageType.Physical);
        if (BaseDarkDamage > 0) damageTypes.Add(DamageType.Dark);
        if (BaseLightDamage > 0) damageTypes.Add(DamageType.Light);
        if (BaseFireDamage > 0) damageTypes.Add(DamageType.Fire);
        if (BaseIceDamage > 0) damageTypes.Add(DamageType.Ice);
        if (BaseLightningDamage > 0) damageTypes.Add(DamageType.Lightning);
        if (BaseSanityDamage > 0) damageTypes.Add(DamageType.Sanity);
        if (BaseDiseaseDamage > 0) damageTypes.Add(DamageType.Disease);

        return [.. damageTypes];
    } }
}