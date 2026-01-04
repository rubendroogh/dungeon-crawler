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
    public static Build GetSingleDomainBuild(Domain domain)
    {
        var build = new Build();

        // Get 4 starting blessings
        var startingBlessings = new List<Blessing>
        {
            new(Level.Minor, domain),
            new(Level.Minor, domain),
            new(Level.Minor, domain),
            new(Level.Minor, domain),
        };

        build.Blessings = startingBlessings;
        build.Alignment = [domain];
        return build;
    }

    /// <summary>
    /// Get the starting dual-domain build for the specified domains.
    /// </summary>
    public static Build GetDualDomainBuild(Domain domain1, Domain domain2)
    {
        var build = new Build();

        // Get 2 starting blessings of each domain
        var startingBlessings = new List<Blessing>
        {
            new(Level.Minor, domain1),
            new(Level.Minor, domain1),
            new(Level.Minor, domain2),
            new(Level.Minor, domain2),
        };

        build.Blessings = startingBlessings;
        build.Alignment = [domain1, domain2];
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