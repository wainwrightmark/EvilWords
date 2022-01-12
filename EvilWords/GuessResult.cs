using System.Diagnostics.Contracts;
using Generator.Equals;

namespace EvilWords;


[Equatable]
public partial record GuessResult([property: OrderedEquality] IReadOnlyList<CharResult> Results)
{
    /// <summary>
    /// Was this a successful guess
    /// </summary>
    public bool IsCorrect => Results.All(x => x.ResultColor == ResultColor.Green);

    /// <inheritdoc />
    public override string ToString() => Word() + " " + ColorText();

    public string Word() => new(Results.Select(x => x.Character).ToArray());
    public string ColorText() => new(Results.Select(x => x.ResultColor.ToChar()).ToArray());

    public GuessResultOptimizer ToGro() => GuessResultOptimizer.Create(this);

    
    public static GuessResult GetWorstCase(string guess, GameState state, GameSettings settings)
    {
        var gro = state.MakeGuessResultOptimizer()?? GuessResultOptimizer.Create(settings);

        var possibleHiddenWords = settings.PossibleHiddenWords.Where(gro.Allow).ToList();

        if (!possibleHiddenWords.Any())
            return new GuessResult(guess.Select(x => new CharResult(x, ResultColor.Red)).ToArray());

        var worstCaseHiddenWord = possibleHiddenWords.Select(hiddenWord =>
            {
            var guessResult = ScoreWord(hiddenWord, guess);
            return (guessResult, hiddenWord);
            }
        
        ).GroupBy(x=>x.guessResult, x=>x.hiddenWord)
            
            .OrderByDescending(x=>x.Count())
            .ThenBy(x=>x.Key.Word() == guess)
            .First();

        return worstCaseHiddenWord.Key;
    }

    /// <summary>
    /// Scores the word.
    /// Assumes the correct word and the guess are both in the same case
    /// </summary>
    [Pure]
    public static GuessResult ScoreWord(string hiddenWord, string guess)
    {
        if (guess.Length != hiddenWord.Length)
            throw new Exception($"Guess is length {guess.Length} but word is length {hiddenWord.Length}");

        var results = new CharResult[hiddenWord.Length];

        for (var index = 0; index < guess.Length; index++)
        {
            ResultColor color;
            var guessChar = guess[index];
            var solutionChar = hiddenWord[index];
            if (guessChar == solutionChar)
            {
                color = ResultColor.Green;
            }
            else
            {
                var foundMatches = 0;
                for (var solIndex = 0; solIndex < hiddenWord.Length; solIndex++)
                {
                    if (solIndex == index) continue;
                    var solutionChar2 = hiddenWord[solIndex];
                    if (solutionChar2 == guessChar)
                    {
                        if (guess[solIndex] == guessChar) continue; //already matched

                        foundMatches++;
                    }
                }

                if (foundMatches > 0)
                {
                    var previousYellows = 0;
                    for (var guessIndex = 0; guessIndex < index; guessIndex++)
                    {
                        var guessChar2 = guess[guessIndex];
                        if (guessChar2 == guessChar)
                        {
                            if (hiddenWord[guessIndex] != guessChar2) //exclude green matches
                            {
                                previousYellows++;
                            }
                        }
                    }

                    color = foundMatches > previousYellows ? ResultColor.Yellow : ResultColor.Red;
                }
                else color = ResultColor.Red;
            }

            results[index] = new CharResult(guessChar, color);
        }

        return new GuessResult(results);
    }
}