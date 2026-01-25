using DungeonRPG.Blessings.Enums;
using Godot;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// SpellBookManager is responsible for managing the list of spells available in the game.
/// </summary>
public partial class SpellBookManager : Node
{
    /// <summary>
    /// The list of spells that the player can cast.
    /// </summary>
    public List<Spell> AvailableSpells = [];

    /// <summary>
    /// A scene that represents a single spell item in the UI.
    /// </summary>
    [Export]
    private PackedScene SpellListItemScene;

    /// <summary>
    /// A preloader that loads spell resources from the project.
    /// </summary>
    [Export]
    private ResourcePreloader SpellPreloader;

    /// <summary>
    /// The ComponentExposer that exposes the spell book components.
    /// </summary>
    [Export]
    private ComponentExposer SpellBookExposer;

    /// <summary>
    /// The container that holds the spell list UI elements.
    /// </summary>
    private Node SpellListContainer => SpellBookExposer.GetComponent<Node>(Components.SpellListContainer);

    /// <summary>
    /// A dictionary that maps spell names to their respective behaviors.
    /// </summary>
    private Dictionary<string, IActionBehaviour> SpellBehaviours = new()
    {
        { "Fireball", new FireBallBehaviour() },
        { "Solidify", new SolidifyBehaviour() },
        { "Umbral Shield", new UmbralShieldBehaviour() },
        { "Plague" , new PlagueBehaviour() },
        { "Discombobulate", new DiscombobulateBehaviour() },
    };

    public override void _Ready()
    {
        UpdateSpellListUI();
    }

    /// <summary>
    /// Retrieves a spell from the available spell list by its name.
    /// </summary>
    public Spell GetSpell(string spellName)
    {
        foreach (var spell in AvailableSpells)
        {
            if (spell.Data.Name == spellName)
            {
                return spell;
            }
        }

        GD.PrintErr("Spell not found: " + spellName);
        return null;
    }

    /// <summary>
    /// Adds a spell to the list of available spells.
    /// </summary>
    public void AddSpell(ActionData spellData)
    {
        var spellBehaviour = GetSpellBehaviour(spellData.Name);
        if (spellBehaviour != null)
        {
            AvailableSpells.Add(new Spell(spellData, spellBehaviour));
            UpdateSpellListUI();
        }
        else
        {
            GD.PrintErr("No behavior defined for spell: " + spellData.Name);
        }
    }

    /// <summary>
    /// Gets the starting spells for the player based on their starting alignment.
    /// </summary>
    public List<Spell> GetStartingSpellsByDomain(Domain[] domains)
    {
        List<Spell> spellList = [];
        ResourcePreloader spellPreloader;

        // Get the correct preloader based on the provided domains
        if (domains.Length == 1 && domains.Contains(Domain.Zer))
        {
            spellPreloader = FindChild("ZerSpellsPreloader") as ResourcePreloader;
        }
        else if (domains.Length == 1 && domains.Contains(Domain.Calina))
        {
            spellPreloader = FindChild("CalinaSpellsPreloader") as ResourcePreloader;
        }
        else if (domains.Length == 1 && domains.Contains(Domain.Hamin))
        {
            spellPreloader = FindChild("HaminSpellsPreloader") as ResourcePreloader;
        }
        else if (domains.Length == 1 && domains.Contains(Domain.Jaddis))
        {
            spellPreloader = FindChild("JaddisSpellsPreloader") as ResourcePreloader;
        }
        else
        {
            GD.PrintErr($"No starting spell preloader found for build {domains}");
            return spellList;
        }

        // Get the spells from the preloader and initialize them
        foreach (var key in spellPreloader.GetResourceList())
        {
            ActionData spellData = spellPreloader.GetResource(key) as ActionData;
            IActionBehaviour spellBehaviour = GetSpellBehaviour(spellData.Name);

            spellData.Initialize();
            spellList.Add(new Spell(spellData, spellBehaviour));
        }

        return spellList;
    }

    /// <summary>
    /// Retrieves the spell behavior for a given spell name.
    /// If the spell name is not found, it returns a default behavior.
    /// </summary>
    private IActionBehaviour GetSpellBehaviour(string spellName)
    {
        if (SpellBehaviours.TryGetValue(spellName, out var behaviour))
        {
            return behaviour;
        }
        else
        {
            return new DefaultSpellBehaviour();
        }
    }

    /// <summary>
    /// Updates the spell list UI by clearing existing spell items
    /// and recreating them based on the current list of available spells.
    /// </summary>
    private void UpdateSpellListUI()
    {
        // Clear the existing UI elements
        foreach (var child in SpellListContainer.GetChildren())
        {
            if (child is SpellUI spellUI)
            {
                spellUI.QueueFree();
            }
        }

        // Recreate the UI for each spell
        foreach (var spell in AvailableSpells)
        {
            var spellUI = SpellListItemScene.Instantiate<SpellUI>();
            spellUI.Setup(spell.Data);
            SpellListContainer.AddChild(spellUI);
        }
    }
}
