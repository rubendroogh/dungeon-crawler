using Godot;

/// <summary>
/// Resource data related to a character. A character is an entity that can be on either side of combat.
/// </summary>
[GlobalClass]
public partial class CharacterData : Resource
{
    /// <summary>
    /// The name.
    /// </summary>
    [Export]
    public string Name { get; set; } = "Default character";
    
    /// <summary>
    /// The sprite to show in combat.
    /// </summary>
    [Export]
    public Texture2D Image { get; set; }

    /// <summary>
    /// The base max health of the character.
    /// </summary>
    [Export]
    public float MaxHealth { get; set; } = 1200;

    /// <summary>
    /// 0-20 the Genuine personality stat.
    /// </summary>
    [Export]
    public int BaseGenuine { get; set; } = 10;

    /// <summary>
    /// 0-20 the Charming personality stat.
    /// </summary>
    [Export]
    public int BaseCharming { get; set; } = 10;

    /// <summary>
    /// 0-20 the Focused personality stat.
    /// </summary>
    [Export]
    public int BaseFocused { get; set; } = 10;

    /// <summary>
    /// 0-20 the Witty personality stat.
    /// </summary>
    [Export]
    public int BaseWitty { get; set; } = 10;

    /// <summary>
    /// 0-20 the Optimistic personality stat.
    /// </summary>
    [Export]
    public int BaseOptimistic { get; set; } = 10;

    /// <summary>
    /// 0-20 the Dominant personality stat.
    /// </summary>
    [Export]
    public int BaseDominant { get; set; } = 10;

    /// <summary>
    /// 0-20 the Fearless personality stat.
    /// </summary>
    [Export]
    public int BaseFearless { get; set; } = 10;

    /// <summary>
    /// 0-20 the Benevolent personality stat.
    /// </summary>
    [Export]
    public int BaseBenevolent { get; set; } = 10;

    /// <summary>
    /// 0-20 the Intelligent personality stat.
    /// </summary>
    [Export]
    public int BaseIntelligent { get; set; } = 10;

    /// <summary>
    /// The base physical damage multiplier.
    /// </summary>
    [Export]
    public float BasePhysicalDamageMultiplier { get; set; } = 0;
    
    /// <summary>
    /// The base dark damage multiplier.
    /// </summary>
    [Export]
    public float BaseDarkDamageMultiplier { get; set; } = 0;
    
    /// <summary>
    /// The base light damage multiplier.
    /// </summary>
    [Export]
   public float BaseLightDamageMultiplier { get; set; } = 0;
   
    /// <summary>
    /// The base fire damage multiplier.
    /// </summary>
    [Export]
    public float BaseFireDamageMultiplier { get; set; } = 0;
    
    /// <summary>
    /// The base ice damage multiplier.
    /// </summary>
    [Export]
    public float BaseIceDamageMultiplier { get; set; } = 0;
    
    /// <summary>
    /// The base lightning damage multiplier.
    /// </summary>
    [Export]
    public float BaseLightningDamageMultiplier { get; set; } = 0;
    
    /// <summary>
    /// The base sanity damage multiplier.
    /// </summary>
    [Export]
    public float BaseSanityDamageMultiplier { get; set; } = 0;

    /// <summary>
    /// The base disease damage multiplier.
    /// </summary>
    [Export]
    public float BaseDiseaseDamageMultiplier { get; set; } = 0;
}
