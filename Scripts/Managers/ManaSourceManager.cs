using Godot;
using System.Collections.Generic;
using System.Linq;
using DungeonRPG.Blessings.Enums;

/// <summary>
/// The manager responsible for handling mana sources and their related functionalities.
/// For now, this is only the blessing bars.
/// TODO: Split this file into multiple classes (blessingBar, enums, mana source manager, etc.)
/// TODO: Add event for when mana sources change (e.g., blessings are added/removed).
/// </summary>
public partial class ManaSourceManager : Node
{
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
	/// The component exposer for the blessing bar UI elements.
	/// </summary>
	[Export]
    private ComponentExposer BlessingsBarExposer;

	/// <summary>
	/// The scene used to instantiate blessing UI elements.
	/// </summary>
    [Export]
    private PackedScene BlessingUIScene;

	/// <summary>
	/// Checks if the current mana sources can pay for the given spell cost.
	/// </summary>
	/// <param name="spellCost">The cost of the spell.</param>
	/// <returns>True if the mana sources can pay for the spell, false otherwise.</returns>
	public bool CanPay(SpellCost spellCost)
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
			if (GetRemainingMana(domainCost.Key) < domainCost.Value)
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
		if (BlessingBar.AvailableBlessings.Count + (int)blessing.Level >= BlessingBar.MaxMana)
		{
			GD.PrintErr("Not enough space in the blessing bar to add a new blessing.");
			return false;
		}

		var blessingContainerNode = BlessingsBarExposer.GetComponent<Node>(Components.BlessingContainer);
		var blessingNode = BlessingUIScene.Instantiate<BlessingUI>().Setup(blessing);

		// Add the node to the scene
		blessingContainerNode.AddChild(blessingNode);
		BlessingBar.AllBlessings.Add(blessing);

		return true;
	}

	/// <summary>
	/// Gets the remaining mana for the given domain.
	/// </summary>
	private int GetRemainingMana(Domain domain)
	{
		// Get the available blessings and get the total mana from them by summing their levels
		var availableBlessings = BlessingBar.AvailableBlessings.Where(b => b.Domain == domain).ToList();
		var count = availableBlessings.Sum(b => (int)b.Level);

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
	/// The maximum amount of mana the blessing bar can hold.
	/// </summary>
	public int MaxMana { get; set; } = 10;

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
	public Color GetColor()
	{
		return Domain switch
		{
			Domain.Calina => Colors.Red,
			Domain.Hamin => Colors.Blue,
			Domain.Jaddis => Colors.Green,
			Domain.Zer => Colors.Purple,
			_ => Colors.White,
		};
	}
}

namespace DungeonRPG.Blessings.Enums
{
	/// <summary>
	/// Represents the rank of a card in a standard deck.
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

