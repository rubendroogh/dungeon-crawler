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
    Storm, // Repeats the spell for each spell previously cast this turn.
    Cruel, // Deals double damage if the target is below half health.
}