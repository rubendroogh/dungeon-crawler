using Godot;
using System.Collections.Generic;

/// <summary>
/// SpellListManager is responsible for managing the list of spells available in the game.
/// </summary>
public partial class SpellListManager : Node
{
    public List<Spell> AvailableSpells = [];

    [Export]
    public PackedScene SpellListItemScene;

    private ResourcePreloader SpellPreloader;

    private VBoxContainer SpellListContainer;

    /// <summary>
    /// A dictionary that maps spell names to their respective behaviors.
    /// </summary>
    private Dictionary<string, ISpellBehaviour> SpellBehaviours = new()
    {
        { "Fireball", new FireBallBehaviour() },
        { "Brain Freeze", new BrainFreezeBehaviour() },
    };

    public override void _Ready()
    {
        SpellPreloader = GetNode<ResourcePreloader>("SpellPreloader");
        SpellListContainer = GetNode<VBoxContainer>("../../SpellList/SpellListPanelContainer/SpellListHorizontalContainer/SpellList");
        InitializeSpells();
    }

    /// <summary>
    /// Retrieves a spell by its name.
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
    /// Initializes the spells by loading them from the preloader and setting up their UI.
    /// This method iterates through the available spells, retrieves their data and behavior,
    /// and creates UI elements for each spell.
    /// </summary>
    private void InitializeSpells()
    {
        foreach (var key in SpellPreloader.GetResourceList())
        {
            SpellData spellData = SpellPreloader.GetResource(key) as SpellData;
            ISpellBehaviour spellBehaviour = GetSpellBehaviour(spellData.Name);

            if (spellBehaviour != null)
            {
                AvailableSpells.Add(new Spell(spellData, spellBehaviour));

                // Create UI for the spell
                var spellUI = SpellListItemScene.Instantiate<SpellUI>();
                spellUI.Setup(spellData);
                SpellListContainer.AddChild(spellUI);
            }
            else
            {
                GD.Print("No behavior defined for spell: " + spellData.Name);
            }
        }
    }

    /// <summary>
    /// Retrieves the spell behavior for a given spell name.
    /// If the spell name is not found, it returns a default behavior.
    /// </summary>
    private ISpellBehaviour GetSpellBehaviour(string spellName)
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
}
