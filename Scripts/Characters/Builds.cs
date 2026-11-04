using System;
using System.Collections.Generic;
using DungeonRPG.Blessings.Enums;

namespace DungeonRPG.Characters;

/// <summary>
/// Static class containing predefined character builds.
/// </summary>
public static class Builds
{
    /// <summary>
    /// Get the starting build for the specified domain.
    /// </summary>
    public static Build GetSingleDomainBuild(Domain domain, int startingMana)
    {
        if (startingMana < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(startingMana));
        }

        var build = new Build();
        var startingBlessings = new List<Blessing>();
        for (var index = 0; index < startingMana; index++)
        {
            startingBlessings.Add(new Blessing(Level.Minor, domain));
        }

        build.Blessings = startingBlessings;
        build.Alignment = [domain];
        build.Spells = SpellBookManager.Instance.GetStartingSpellsByDomain([domain]);

        return build;
    }

    /// <summary>
    /// Splits the starting mana between two domains, favoring the dominant domain for odd totals.
    /// </summary>
    public static Build GetDualDomainBuild(Domain domain1, Domain domain2, int startingMana)
    {
        if (startingMana < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(startingMana));
        }

        var build = new Build();
        var startingBlessings = new List<Blessing>();
        var primaryMana = (startingMana + 1) / 2;
        for (var index = 0; index < startingMana; index++)
        {
            var domain = index < primaryMana ? domain1 : domain2;
            startingBlessings.Add(new Blessing(Level.Minor, domain));
        }

        build.Blessings = startingBlessings;
        build.Alignment = [domain1, domain2];
        build.Spells = SpellBookManager.Instance.GetStartingSpellsByDomain([domain1, domain2]);

        return build;
    }
}

/// <summary>
/// A character (starting) build consisting of blessings and spells.
/// </summary>
public class Build
{
    /// <summary>
    /// The starting divine alignment.
    /// </summary>
    public Domain[] Alignment;

    /// <summary>
    /// The blessings granted by this build.
    /// </summary>
    public List<Blessing> Blessings = new();

    /// <summary>
    /// The spells granted by this build.
    /// </summary>
    public List<Spell> Spells = new();
}
