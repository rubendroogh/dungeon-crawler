using System.Collections.Generic;

/// <summary>
/// Represents the mana cost of an action or spell.
/// </summary>
public class ManaCost(Suit type, int amount)
{
    /// <summary>
    /// The type of mana required.
    /// </summary>
    public Suit Type { get; set; } = type;

    /// <summary>
    /// The amount of mana required.
    /// </summary>
    public int Amount { get; set; } = amount;
}

/// <summary>
/// Represents the total mana cost for a spell, which can consist of multiple ManaCost entries.
/// </summary>
public class SpellCost
{
    /// <summary>
    /// The list of mana costs required to cast the spell.
    /// </summary>
    public List<ManaCost> Costs { get; set; } = new List<ManaCost>();

    /// <summary>
    /// Calculates the total mana cost by summing up all individual costs.
    /// </summary>
    public int TotalCost
    {
        get
        {
            int total = 0;
            foreach (var cost in Costs)
            {
                total += cost.Amount;
            }
            return total;
        }
    }

    /// <summary>
    /// Adds a new mana cost to the spell.
    /// </summary>
    public void AddCost(Suit type, int amount)
    {
        Costs.Add(new ManaCost(type, amount));
    }

    /// <summary>
    /// Removes a mana cost of a specific type from the spell.
    /// </summary>
    public void RemoveCost(Suit type)
    {
        Costs.RemoveAll(c => c.Type == type);
    }

    /// <summary>
    /// Clears all mana costs from the spell.
    /// </summary>
    public void ClearCosts()
    {
        Costs.Clear();
    }

    /// <summary>
    /// Checks if the spell has any mana costs defined.
    /// </summary>
    /// <returns>True if there are costs defined, otherwise false.</returns>
    public bool HasCosts()
    {
        return Costs.Count > 0;
    }
}