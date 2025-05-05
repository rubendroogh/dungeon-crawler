using System.Collections.Generic;
using Godot;

[GlobalClass] // Makes it creatable from the "New Resource" menu
public partial class SpellData : Resource
{
    [Export]
    public string Name { get; set; } = "Default spell";

    [Export]
    public string Description { get; set; } = "You forgot to fill this in!";

    [Export]
    public Texture2D Image { get; set; }

    [Export]
    public float BasePhysicalDamage { get; set; } = 100;
    
    [Export]
    public float BaseDarkDamage { get; set; } = 0;

    [Export]
    public float BaseLightDamage { get; set; } = 0;

    [Export]
    public float BaseFireDamage { get; set; } = 0;

    [Export]
    public float BaseIceDamage { get; set; } = 0;

    [Export]
    public float BaseLightningDamage { get; set; } = 0;

    [Export]
    public float BaseSanityDamage { get; set; } = 0;

    [Export]
    public float BaseDiseaseDamage { get; set; } = 0;

    /// <summary>
    /// The max amount of mana charges this spell can have.
    /// </summary>
    [Export]
    public int MaxManaCharges { get; set; } = 4;

    [Export]
    public float ModifierMultiplier = 1.01f;

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

public enum DamageType
{
    Physical,
    Dark,
    Light,
    Fire,
    Ice,
    Lightning,
    Sanity,
    Disease,
}