using System.Collections.Generic;
using System.Linq;

public static class EnumerableExtensions
{
    /// <summary>
    /// Subtracts the elements of one enumerable from another, taking into account the count of each element.
    /// </summary>
    public static IEnumerable<T> Subtract<T>(
        this IEnumerable<T> source,
        IEnumerable<T> toRemove)
    {
        var removeCounts = toRemove
            .GroupBy(x => x)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var item in source)
        {
            if (removeCounts.TryGetValue(item, out var count) && count > 0)
            {
                removeCounts[item] = count - 1;
                continue;
            }

            yield return item;
        }
    }
}