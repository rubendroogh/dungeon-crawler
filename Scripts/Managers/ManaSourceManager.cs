using Godot;
using System.Collections.Generic;
using System.Linq;
using DungeonRPG.Blessings.Enums;

/// <summary>
/// The manager responsible for handling mana sources and their related functionalities.
/// For now, this is only the blessing bars.
/// </summary>
public partial class ManaSourceManager : Node
{
	// TODO: Add event for when mana sources change (e.g., blessings are added/removed).

	public Dictionary<Domain, BlessingBar> BlessingBars { get; private set; } = new Dictionary<Domain, BlessingBar>();

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

	private int GetRemainingMana(Domain domain)
	{
		// Get the blessing bar for the domain
		if (!BlessingBars.TryGetValue(domain, out var blessingBar))
		{
			return 0;
		}

		// Get the available blessings and get the total mana from them by summing their levels
		var availableBlessings = blessingBar.AvailableBlessings;
		var count = availableBlessings.Sum(b => (int)b.Level);

		return count;
	}

	private void ShowOrHideBlessingBars()
	{

	}
}

/// <summary>
/// Represents a blessing bar that tracks the current and maximum mana for a specific blessing.
/// </summary>
public class BlessingBar
{
	/// <summary>
	/// The domain of the blessing bar.
	/// Each blessing bar corresponds to a specific domain.
	/// </summary>
	public Domain Domain { get; set; }

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
	public int MaxMana { get; set; }

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
		if (blessing.Domain != Domain)
		{
			GD.PrintErr("Cannot add blessing of domain " + blessing.Domain + " to blessing bar of domain " + Domain);
			return;
		}

		// Check if there is enough space
		if (AvailableBlessings.Count + (int)blessing.Level < MaxMana)
		{
			AvailableBlessings.Add(blessing);
		}
	}
}

/// <summary>
/// Represents a blessing with a rank and domain (which is its type).
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
	/// TODO: Maybe remove this and track the domain via the blessing bar instead?
	/// </summary>
	public Domain Domain { get; set; }
}

namespace DungeonRPG.Blessings.Enums
{
	/// <summary>
	/// Represents the rank of a card in a standard deck.
	/// The ranks are ordered from Minor (1) to Major (4).
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

