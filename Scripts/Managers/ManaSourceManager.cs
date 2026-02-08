using Godot;
using System.Collections.Generic;
using System.Linq;
using DungeonRPG.Blessings.Enums;
using System;

/// <summary>
/// The manager responsible for handling mana sources and their related functionalities.
/// For now, this is only the blessing bars.
/// TODO: Split this file into multiple classes (blessingBar, enums, mana source manager, etc.)
/// TODO: Add event for when mana sources change (e.g., blessings are added/removed).
/// </summary>
public partial class ManaSourceManager : Node
{
	public static ManaSourceManager Instance { get; private set; }

	/// <summary>
	/// The blessing bar containing the player's blessings.
	/// </summary>
	public BlessingBar BlessingBar { get; private set; } = new BlessingBar();
	
	/// <summary>
	/// The width of the blessing bar in pixels.
	/// </summary>
	public int Width
	{
		get {
			return (int)BlessingsBarExposer.GetComponent<Control>(Components.BlessingBarOverlay).Size.X;
		}
	}

	/// <summary>
    /// True if the player is able to select mana for an action.
    /// </summary>
	public bool ManaSelectionMode { get; private set; }

	/// <summary>
    /// Signal that is emitted when the state of blessings changes (e.g., when mana is reserved or spent).
    /// </summary>
	[Signal]
	public delegate void BlessingStateChangedEventHandler();

	/// <summary>
    /// Signal that is emitted when the mana selection mode changes.
    /// </summary>
    /// <param name="isEnabled">Indicates whether mana selection mode is enabled.</param>
	[Signal]
	public delegate void ManaSelectionModeChangedEventHandler(bool isEnabled);

	/// <summary>
	/// The component exposer for the blessing bar UI elements.
	/// </summary>
    private ComponentExposer BlessingsBarExposer;

	/// <summary>
	/// The scene used to instantiate blessing UI elements.
	/// </summary>
    [Export]
    private PackedScene BlessingUIScene;

	public override void _Ready()
	{
		Instance = this;
		BlessingsBarExposer = GetTree().Root.FindChild("BlessingBar", true, false) as ComponentExposer;
		CallDeferred(nameof(InitializeCustomSignals));
	}

	/// <summary>
	/// Initializes custom signals for the ManaSourceManager.
	/// </summary>
	public void InitializeCustomSignals()
	{
		ActionManager.Instance.SpellSelected += OnSpellSelectionChanged;
	}

	/// <summary>
    /// Handles changes in spell selection to update mana selection mode.
	/// If a spell is selected, mana selection mode is enabled; otherwise, it is disabled.
    /// </summary>
	public void OnSpellSelectionChanged(string spellName)
	{
		SetManaSelectionMode(!string.IsNullOrEmpty(spellName));
	}

	/// <summary>
    /// Checks if the player can pay for the given spell cost with their available mana.
    /// </summary>
    /// <param name="spellCost">The cost of the spell.</param>
    /// <returns>True if the player can pay for the spell, false otherwise.</returns>
	public bool CanPay(SpellCost spellCost)
    {
		// TODO: This is duplicated code with SelectedManaCanPay. Refactor to avoid duplication.
		// Aggregate all costs by domain
		var costByDomain = new Dictionary<Domain, int>();
		foreach (var cost in spellCost.Costs)
		{
			if (!costByDomain.ContainsKey(cost.Type))
			{
				costByDomain[cost.Type] = 0;
			}

			costByDomain[cost.Type] += cost.Amount;
		}

		// Check if we have enough mana in each domain
		foreach (var domainCost in costByDomain)
		{
			if (GetUnspentManaTotal(domainCost.Key) < domainCost.Value)
			{
				return false;
			}
		}

		return true;
    }

	/// <summary>
	/// Checks if the selected mana can pay for the given spell cost.
	/// </summary>
	/// <param name="spellCost">The cost of the spell.</param>
	/// <returns>True if the mana can pay for the spell using the currently selected mana, false otherwise.</returns>
	public bool SelectedManaCanPay(SpellCost spellCost)
	{
		// Aggregate all costs by domain
		var costByDomain = new Dictionary<Domain, int>();
		foreach (var cost in spellCost.Costs)
		{
			if (!costByDomain.ContainsKey(cost.Type))
			{
				costByDomain[cost.Type] = 0;
			}

			costByDomain[cost.Type] += cost.Amount;
		}

		// Check if we have enough mana in each domain
		foreach (var domainCost in costByDomain)
		{
			if (GetSelectedManaTotal(domainCost.Key) < domainCost.Value)
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Adds a blessing to the player's blessing bar.
	/// </summary>
	public bool AddBlessing(Blessing blessing)
	{
		if (BlessingBar.AllMana + (int)blessing.Level > BlessingBar.MaxMana)
		{
			GD.PrintErr("Not enough space in the blessing bar to add a new blessing.");
			return false;
		}

		var blessingContainerNode = BlessingsBarExposer.GetComponent<Node>(Components.BlessingContainer);
		var blessingNode = BlessingUIScene.Instantiate<BlessingUI>().Setup(blessing);

		// Add the node to the scene
		blessingContainerNode.AddChild(blessingNode);
		BlessingBar.AllBlessings.Add(blessing);
		EmitSignal(SignalName.BlessingStateChanged);

		return true;
	}

	/// <summary>
    /// Autoselects mana for a specified spellcost.
    /// </summary>
	public bool AutoselectMana(SpellCost cost)
    {
		// Deselect all mana first
		DeselectAllMana();

		// Get the most efficient combination of blessings to pay the cost
		var blessingsToSelect = GetMostEfficientManaCombination(cost, out bool canPay);

		// Mark the blessings as selected
		blessingsToSelect.ForEach(b => b.State = State.MarkedForUse);

		// Emit signal to display blessing state change in the UI.
		EmitSignal(SignalName.BlessingStateChanged);
		return true;
    }

	/// <summary>
	/// Gets the most efficient combination of blessings to pay for the given spell cost.
	/// </summary>
	public List<Blessing> GetMostEfficientManaCombination(SpellCost cost, out bool canPay)
	{
		var reservedBlessings = new List<Blessing>();

		// Loop through the mana costs to calculate the most effecient spending.
		foreach (var manaCost in cost.Costs)
		{
			int amountToReserve = manaCost.Amount;

			// Use the smallest blessing that can cover the cost first
			var availableBlessings = BlessingBar.AvailableBlessings
				.Subtract(reservedBlessings)
				.Where(b => b.Domain == manaCost.Type)
				.OrderBy(b => b.ManaAmount) // Lowest first
				.ToList();

			// If there is not enough mana at all, we can just stop now.
			if (availableBlessings.Sum(b => b.ManaAmount) < amountToReserve)
			{
				canPay = false;
				return reservedBlessings;
			}

			// If we have a blessing that is exactly the right size, we use that one.
			var exactMatchingBlessing = availableBlessings.Find(b => b.ManaAmount == amountToReserve);
			if (exactMatchingBlessing != null)
            {
				// Cost has been paid.
                reservedBlessings.Add(exactMatchingBlessing);
				continue;
            }

			// If the lowest value blessing has enough mana to pay, we use that one.
			var lowestValueBlessing = availableBlessings[0];
			if (lowestValueBlessing.ManaAmount > amountToReserve)
            {
				// Cost has been paid.
				reservedBlessings.Add(lowestValueBlessing);
				continue;
            }

			// For other cases, we add up from lowest to highest value until we have enough.
			var reservedMana = 0;
			foreach (var blessing in availableBlessings)
			{
				if (reservedMana >= amountToReserve)
                {
                    break;
                }

				reservedBlessings.Add(blessing);
				reservedMana += blessing.ManaAmount;
			}
		}

		canPay = true;
		return reservedBlessings;
	}

	/// <summary>
	/// Highlights the specified blessings in the blessing bar UI.
	/// </summary>
	public void HighlightBlessings(List<Blessing> blessingsToHighlight)
	{
		if (blessingsToHighlight == null || blessingsToHighlight.Count == 0)
		{
			return;
		}

		BlessingBar.AllBlessings.ForEach(b =>
		{
			var blessingUI = BlessingsBarExposer.GetComponent<Node>(Components.BlessingContainer)
				.GetChildren()
				.OfType<BlessingUI>()
				.FirstOrDefault(ui => ui.Blessing.ID == b.ID);

			blessingUI?.SetHighlight(blessingsToHighlight.Contains(b));
		});
	}

	/// <summary>
	/// Clears all highlighted blessings in the blessing bar UI.
	/// </summary>
	public void ClearHighlighted()
	{
		BlessingBar.AllBlessings.ForEach(b =>
		{
			var blessingUI = BlessingsBarExposer.GetComponent<Node>(Components.BlessingContainer)
				.GetChildren()
				.OfType<BlessingUI>()
				.FirstOrDefault(ui => ui.Blessing.ID == b.ID);

			blessingUI?.SetHighlight(false);
		});
	}

	/// <summary>
    /// Deselects all mana marked for use and marks them as available.
    /// </summary>
	public void DeselectAllMana()
    {
        BlessingBar.BlessingsMarkedForUse.ForEach(x => x.State = State.Available);
		EmitSignal(SignalName.BlessingStateChanged);
    }

	/// <summary>
    /// Marks all mana marked for use as spent.
    /// </summary>
	public void SpendSelectedMana()
    {
		BlessingBar.BlessingsMarkedForUse.ForEach(b => b.State = State.Spent);
		EmitSignal(SignalName.BlessingStateChanged);
    }

	/// <summary>
    /// Marks all mana as available.
    /// </summary>
	public void ResetAllMana()
    {
		BlessingBar.AllBlessings.ForEach(b => b.State = State.Available);
		EmitSignal(SignalName.BlessingStateChanged);
    }

	/// <summary>
    /// Set whether the player can select mana.
    /// </summary>
	public void SetManaSelectionMode(bool value)
    {
        ManaSelectionMode = value;
		EmitSignal(SignalName.ManaSelectionModeChanged, ManaSelectionMode);
    }

	/// <summary>
	/// Gets the remaining mana for the given domain. This is mana marked for use.
	/// </summary>
	private int GetSelectedManaTotal(Domain domain)
	{
		// Get the available blessings and get the total mana from them by summing their levels
		var markedForUse = BlessingBar.BlessingsMarkedForUse.Where(b => b.Domain == domain).ToList();
		var count = markedForUse.Sum(b => (int)b.Level);

		return count;
	}

	/// <summary>
    /// Gets the total available mana for the given domain.
    /// </summary>
	private int GetAvailableManaTotal(Domain domain)
	{
		// Get the available blessings and get the total mana from them by summing their levels
		var availableBlessings = BlessingBar.AvailableBlessings.Where(b => b.Domain == domain).ToList();
		var count = availableBlessings.Sum(b => (int)b.Level);

		return count;
	}

	/// <summary>
    /// Gets the total mana for the given domain which has not been spent.
    /// </summary>
	private int GetUnspentManaTotal(Domain domain)
	{
		// Get the blessings not spent
		var unspentBlessings = BlessingBar.AllBlessings
			.Where(b => b.Domain == domain && b.State != State.Spent)
			.ToList();
		var count = unspentBlessings.Sum(b => (int)b.Level);

		return count;
	}
}

/// <summary>
/// Represents the blessing bar that holds blessings (mana) for all domains for the player.
/// </summary>
public class BlessingBar
{
	/// <summary>
	/// All blessings that have been added to the blessing bar, including spent ones.
	/// </summary>
	public List<Blessing> AllBlessings { get; set; } = new List<Blessing>();

	/// <summary>
	/// Blessings that are marked for use in a queued action.
	/// TODO: Find a way to track which blessings are used by which action so they can be properly managed.
	/// </summary>
	public List<Blessing> BlessingsMarkedForUse => AllBlessings.Where(b => b.State == State.MarkedForUse).ToList();

	/// <summary>
	/// The current amount of unspent mana in the blessing bar.
	/// </summary>
	public List<Blessing> AvailableBlessings => AllBlessings.Where(b => b.State == State.Available).ToList();

	/// <summary>
    /// Gets the total amount of mana from all blessings, regardless of their state.
    /// </summary>
	public int AllMana => AllBlessings.Sum(x => (int)x.Level);

	/// <summary>
	/// The maximum amount of mana the blessing bar can hold.
	/// </summary>
	public int MaxMana { get; set; } = 4;

	/// <summary>
    /// Sets a specific blessing's state by ID.
    /// </summary>
	public void SetBlessingState(Guid ID, State state)
	{
		AllBlessings.Find(b => b.ID == ID).State = state;
		ManaSourceManager.Instance.EmitSignal("BlessingStateChanged");
	}

	/// <summary>
	/// Checks if the blessing bar can pay for the given mana cost.
	/// </summary>
	public bool CanPay(ManaCost cost)
	{
		int totalAvailable = AvailableBlessings.Count(b => b.Domain == cost.Type);
		return totalAvailable >= cost.Amount;
	}

	/// <summary>
	/// Adds a blessing to the blessing bar if there is enough space.
	/// </summary>
	public void AddBlessing(Blessing blessing)
	{
		// Check if there is enough space
		if (AvailableBlessings.Count + (int)blessing.Level < MaxMana)
		{
			AvailableBlessings.Add(blessing);
		}
	}
}

/// <summary>
/// Represents a blessing with a rank and domain.
/// This is used to cast spells in the game and fills the blessing bar.
/// </summary>
public class Blessing
{
    public Blessing(Level level, Domain domain)
    {
        Level = level;
		Domain = domain;
		ID = Guid.NewGuid();
    }

	/// <summary>
    /// The unique identifier to get specific blessings.
    /// </summary>
	public Guid ID { get; }

    /// <summary>
    /// The potency level of the blessing. This determines how much mana it provides.
    /// </summary>
    public Level Level { get; set; }

	/// <summary>
	/// The current state of the blessing (Available, MarkedForUse, Spent).
	/// </summary>
	public State State { get; set; } = State.Available;

	/// <summary>
	/// The domain of the blessing.
	/// </summary>
	public Domain Domain { get; set; }

	/// <summary>
    /// Gets the amount of mana provided by this blessing.
    /// </summary>
	public int ManaAmount => (int)Level;

	/// <summary>
    /// Returns a string representation of the blessing.
    /// </summary>
	public override string ToString()
	{
		var levelColor = GetLevelColor().ToHexString();
		var domainColor = GetDomainColor().ToHexString();

		return string.Format(
			"[color={0}]{1}[/color] [color={2}]{3}[/color] blessing",
			levelColor,
			Level,
			domainColor,
			Domain);
	}

	/// <summary>
    /// Gets the description of the blessing. This contains any effects, properties, or modifiers.
    /// </summary>
	public string GetDescription()
	{
		return "[color=\"gray\"]No modifiers.[/color]";
	}

	/// <summary>
	/// Gets the texture associated with the blessing's domain and level.
	/// </summary>
	public Texture2D GetTexture()
	{
		string texturePath = $"res://Assets/Textures/Blessings/{Domain}_{Level}.png";
		return GD.Load<Texture2D>(texturePath);
	}

	/// <summary>
	/// Gets the color associated with the blessing's domain.
	/// TODO: Store the colors in a more centralized way.
	/// </summary>
	public Color GetDomainColor()
	{
		var colour = Domain.GetColor();

		// Modify brightness based on level
		// Decrease brightness by 10% per level above Minor
		float brightnessModifier = 1f - ((int)Level - 1) * 0.1f;
		var modifiedColor = colour.ModifyBrightness(brightnessModifier);

		// Modify colour based on state
		modifiedColor = State switch
		{
			State.Available => modifiedColor,
			State.MarkedForUse => modifiedColor.ApplyGreyscale(0.7f),
			State.Spent => modifiedColor.ApplyGreyscale(1f),
			_ => modifiedColor,
		};

		return modifiedColor;
	}

	/// <summary>
    /// Gets the color associated with the blessing's level.
    /// </summary>
	public Color GetLevelColor()
	{
		return Level switch
		{
			Level.Minor => Colors.Gray,
			Level.Lesser => Colors.White,
			Level.Greater => Colors.Orange,
			Level.Major => Colors.Gold,
			Level.Superior => Colors.Purple,
			_ => Colors.White,
		};
	}
}

namespace DungeonRPG.Blessings.Enums
{
	/// <summary>
	/// Represents the level (potency) of a blessing.
	/// The ranks are ordered from Minor (1) to Superior (5).
	/// </summary>
	public enum Level { Minor = 1, Lesser = 2, Greater = 3, Major = 4, Superior = 5 }

	/// <summary>
	/// Represents the state of a blessing in the game.
	/// </summary>
	public enum State
	{
		/// <summary>
		/// The blessing is available for use.
		/// </summary>
		Available,

		/// <summary>
		/// The blessing is marked for use in a queued action.
		/// </summary>
		MarkedForUse,

		/// <summary>
		/// The blessing has been spent (used) and cannot be used again until refreshed by prayer.
		/// </summary>
		Spent
	}

	/// <summary>
	/// Represents the type of a blessing in a standard deck.
	/// </summary>
	public enum Domain
	{
		/// <summary>
		/// Focused on power and resilience.
		/// </summary>
		Calina,

		/// <summary>
		/// Focused on wisdom and calm.
		/// </summary>
		Hamin,

		/// <summary>
		/// Focused on the element of surprise and deception.
		/// </summary>
		Jaddis,

		/// <summary>
		/// Focused on pest and decay.
		/// </summary>
		Zer,
	}
}

