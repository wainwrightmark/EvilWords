using System.Text.RegularExpressions;

namespace EvilWords;

public record GameSettings(int WordLength, int MaxRounds, IReadOnlyList<string> PossibleHiddenWords,
    IReadOnlyList<string> PossibleGuesses, IReadOnlyDictionary<string, string> PossibleTaunts)
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
            var intersection = PossibleTaunts.Keys.Intersect(wordsToChooseFrom).ToList();
            if (intersection.Any())
                wordsToChooseFrom = intersection;
        }

        return wordsToChooseFrom.RandomElement(random);
    }
}

public static class WordListHelper
{

    private static IReadOnlyDictionary<string, string> GetTaunts(string text, int expectedLength)
    {
        return text.Split('\r', '\n')
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(ParseLine)
            .Where(x => x.HasValue)
            .Select(x=>x!.Value)
            .Where(x => x.word.Length == expectedLength)
            .ToDictionary(x => x.word, x => x.message);


        static (string word, string message)? ParseLine(string line)
        {
            var match = TauntRegex.Match(line);
            if (!match.Success) return null;

            var word = match.Groups["word"].Value.Trim().ToUpperInvariant();
            var message = match.Groups["message"].Value.Trim();

            return (word, message);
        }
    }

    private static readonly Regex TauntRegex = new (@"(?<word>\w+)\s*//(?<message>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
    
    public static IReadOnlyDictionary<string, string> FiveLetterPossibleTaunts { get; } =
        GetTaunts(Wordlist.FiveLetterTaunts, 5);
}