using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace EvilWords;

public static class GameHelper
{
    public static char ToChar(this ResultColor resultColor)
    {
        return resultColor switch
        {
            ResultColor.Green => 'G',
            ResultColor.Yellow => 'Y',
            ResultColor.Red => 'R',
            _ => throw new ArgumentOutOfRangeException(nameof(resultColor), resultColor, null)
        };
    }

    public static T RandomElement<T>(this IReadOnlyList<T> list, Random random) => list[random.Next(list.Count)];

    public static ResultColor CycleToNext(this ResultColor resultColor)
    {
        return resultColor switch
        {
            ResultColor.Green => ResultColor.Yellow,
            ResultColor.Yellow => ResultColor.Red,
            ResultColor.Red => ResultColor.Green,
            _ => throw new ArgumentOutOfRangeException(nameof(resultColor), resultColor, null)
        };
    }

    
    

    public static int RunGame(string hiddenWord, GameSettings gameSettings, SolveSettings solveSettings, ILogger logger)
    {
        var sw = Stopwatch.StartNew();
        var gs = new GameState(ImmutableArray<GuessResult>.Empty);

        var rounds = 0;
        while (true)
        {
            rounds++;
            var guess = Solver.GetBestGuess(gs, GameSettings.FiveLetter, solveSettings);
            var guessResult = GuessResult.ScoreWord(hiddenWord, guess);
            var gro = gs.MakeGuessResultOptimizer();

            var remainingSolutions = gro is null ? gameSettings.PossibleHiddenWords.Count : gameSettings.PossibleHiddenWords.Count(gro.Allow);

            logger.LogInformation($"{sw.Elapsed}: Round {rounds} {guessResult.Word()} {guessResult.ColorText()} Remaining Solutions: {remainingSolutions}");

            if (guessResult.IsCorrect)
                return rounds;
            gs = gs.Add(guessResult);
        }
    }
}