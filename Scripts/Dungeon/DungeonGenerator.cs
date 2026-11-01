using System;
using System.Collections.Generic;

namespace Priest.Managers;

/// <summary>
/// The EventManager manages the random event loop that the
/// player goes through. It generates the list of events
/// the player will encounter.
/// 
/// Specific story lines should be managed
/// by their own managers.
/// </summary>
public partial class DungeonGenerator
{
    // Create a tree
    private Scene StartingScene { get; set; }

    public Scene Generate()
    {
        return new WackyStartingScene();
    }
}

public abstract class Scene
{
    public string Title { get; set; }

    public string Text { get; set; }

    public List<Choice> Choices { get; } = [];

    public virtual bool Prerequisite()
    {
        return true;
    }
}

public class Choice
{
    /// <summary>
    /// The display title of the choice.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// A function that returns true if the player has met
    /// the prerequisites to qualify for this choice.
    /// </summary>
    public Func<bool> Prerequisite { get; set; }

    /// <summary>
    /// A function containing what happens when the choice
    /// is selected.
    /// </summary>
    public Action Effect { get; set; }
}