namespace ArkaZilla.Technology;

public static class Searching
{
    public const int DiscordSuggestionsLimit = 25;

    // ReSharper disable once CognitiveComplexity
    internal static int CalculateFuzzyScore(string source, string query)
    {
        if (string.IsNullOrEmpty(query)) return 1;

        if (ulong.TryParse(source, out ulong sourceId) && ulong.TryParse(query, out ulong queryId))
        {
            string sourceStr = sourceId.ToString();
            string queryStr = queryId.ToString();
            if (sourceStr.StartsWith(queryStr))
            {
                return 1000 - (sourceStr.Length - queryStr.Length);
            }
            return 0;
        }

        source = source.ToLowerInvariant();
        query = query.ToLowerInvariant();

        if (source == query) return 1000;

        if (source.StartsWith(query))
        {
            return 800 - source.Length;
        }

        int score = 0;
        int queryIndex = 0;
        bool firstMatchFound = false;
        int consecutiveMatches = 0;

        for (int sourceIndex = 0; sourceIndex < source.Length; sourceIndex++)
        {
            if (queryIndex >= query.Length || source[sourceIndex] != query[queryIndex])
            {
                consecutiveMatches = 0;
                continue;
            }

            if (!firstMatchFound)
            {
                score += 200 - (sourceIndex * 10);
                firstMatchFound = true;
            }

            consecutiveMatches++;
            score += 50 * consecutiveMatches;

            queryIndex++;
        }

        if (queryIndex == query.Length)
        {
            return Math.Max(1, score - source.Length);
        }

        List<char> sourceChars = new(source);
        foreach (int index in query.Select(c => sourceChars.IndexOf(c)))
        {
            if (index == -1)
            {
                return 0;
            }
            sourceChars.RemoveAt(index);
        }

        return Math.Max(1, 50 - source.Length);
    }
}
