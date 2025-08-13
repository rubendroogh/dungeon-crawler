using Godot;
using System.Collections.Generic;

/// <summary>
/// SpellListManager is responsible for managing the list of spells available in the game.
/// </summary>
public partial class SpellListManager : Node
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
    /// The container that holds the spell list UI elements.
    /// </summary>
    private VBoxContainer SpellListContainer;

    /// <summary>
    /// A dictionary that maps spell names to their respective behaviors.
    /// </summary>
    private Dictionary<string, IActionBehaviour> SpellBehaviours = new()
    {
        { "Fireball", new FireBallBehaviour() },
        { "Brain Freeze", new BrainFreezeBehaviour() },
    };

    public override void _Ready()
    {
        SpellListContainer = GetTree().Root.GetNode<VBoxContainer>("Root/UI/HUD/SpellList/SpellListPanelContainer/Margin/SpellListHorizontalContainer/SpellList");
        InitializeSpells();
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
    /// Initializes the spells by loading them from the preloader and setting up their UI.
    /// This method iterates through the available spells, retrieves their data and behavior,
    /// and creates UI elements for each spell.
    /// </summary>
    private void InitializeSpells()
    {
        foreach (var key in SpellPreloader.GetResourceList())
        {
            ActionData spellData = SpellPreloader.GetResource(key) as ActionData;
            IActionBehaviour spellBehaviour = GetSpellBehaviour(spellData.Name);

            if (spellBehaviour != null)
            {
                AvailableSpells.Add(new Spell(spellData, spellBehaviour));
            }
            else
            {
                GD.Print("No behavior defined for spell: " + spellData.Name);
            }
        }

        // Update the UI to reflect the available spells
        UpdateSpellListUI();
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
