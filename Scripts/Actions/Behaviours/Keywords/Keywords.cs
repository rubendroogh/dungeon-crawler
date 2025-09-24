using System;

public class Keywords
{
    public static KeywordBase GetKeywordBehaviour(Keyword keyword)
    {
        return keyword switch
        {
            Keyword.Storm => new StormKeyword(),
            Keyword.Cruel => new CruelKeyword(),
            _ => throw new ArgumentOutOfRangeException(nameof(keyword), $"No behaviour defined for keyword {keyword}"),
        };
    }
}

public enum Keyword
{
    /// <summary>
    /// Repeats the spell for each spell previously cast this turn.
    /// </summary>
    Storm,
    /// <summary>
    /// Deals double damage if the target is below half health.
    /// </summary>
    Cruel,
}