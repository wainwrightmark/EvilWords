namespace EvilWords;

public record GameSettings(int WordLength, int MaxRounds, IReadOnlyList<string> PossibleHiddenWords,
    IReadOnlyList<string> PossibleGuesses, IReadOnlySet<string> PossibleTaunts)
{
    public static readonly GameSettings FiveLetter =
        new (5, 
            6,
            WordListHelper.FiveLetterPossibleHiddenWords,
            WordListHelper.FiveLetterPossibleGuesses,
            WordListHelper.FiveLetterPossibleTaunts);


    public IReadOnlyList<string> FilterHiddenWords(GuessResultOptimizer? gro)
    {
        var wordsToChooseFrom = gro is null ? PossibleHiddenWords : 
            PossibleHiddenWords.Where(gro.Allow).ToList();
        return wordsToChooseFrom;
    }

    public string GetRandomHiddenWord(GuessResultOptimizer? gro,
        Random random, bool prioritizeTaunts)
    {
        var wordsToChooseFrom = FilterHiddenWords(gro);

        if (prioritizeTaunts)
        {
            var intersection = PossibleTaunts.Intersect(wordsToChooseFrom).ToList();
            if (intersection.Any())
                wordsToChooseFrom = intersection;
        }

        return wordsToChooseFrom.RandomElement(random);
    }

    public string Pattern => $"[a-zA-Z]{WordLength}";
}

public static class WordListHelper
{

    private static IReadOnlyList<string> GetWords(string text, int expectedLength)
    {
        return text.Split('\r', '\n')
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Where(x=>x.Length == expectedLength)
            .ToList();
    }


    public static IReadOnlyList<string> FiveLetterPossibleHiddenWords { get; } =
        GetWords(Wordlist.FiveLetterSolutionWords, 5);
    

    public static IReadOnlyList<string> FiveLetterPossibleGuesses { get; } =
        FiveLetterPossibleHiddenWords.Concat(
            GetWords(Wordlist.FiveLetterGuessWords, 5)
            ).ToList();
    
    public static IReadOnlySet<string> FiveLetterPossibleTaunts { get; } =
        GetWords(Wordlist.FiveLetterTaunts, 5).ToHashSet();
}