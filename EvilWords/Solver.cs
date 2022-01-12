using System.Collections.Concurrent;

namespace EvilWords;

/// <summary>
/// An instance of the Solver class
/// </summary>
public class SolverService
{
    private CancellationTokenSource? _cts = null;

    private readonly ConcurrentDictionary<GameState, string?>? _resultCache = new();

    /// <summary>
    /// Gets the best guess to use in this game state
    /// </summary>
    public string? GetBestGuess(
        string serializedGameState
    )
    {
        _cts?.Cancel();

        _cts = new CancellationTokenSource();

        var gameState = GameStateSerialization.Deserialize(serializedGameState);
        var solveSettings =
                new SolveSettings()
                {
                    MaxSolutionsToSearch = 10,
                    OptimalGuessDictionary = SolveSettings.FiveLetterOptimalGuesses,
                    RandomSeed = null,
                    UseParallel = true
                }
            ;

        return Solver.GetBestGuess(gameState,
            GameSettings.FiveLetter,
            solveSettings,
            _resultCache,
            _cts.Token);
    }
}

public static class Solver
{
    public static int CountSolutionsEliminatedByGuess(GuessResultOptimizer? gro, string guessWord,
        IReadOnlyCollection<string> possibleHiddenWords)
    {
        var total = 0;
        foreach (var solution in possibleHiddenWords)
        {
            var guessResult = GuessResult.ScoreWord(solution, guessWord);
            var possibleGro = GuessResultOptimizer.Create(guessResult);
            var newGro = gro is null ? possibleGro : gro.Combine(possibleGro);

            var solutionsEliminated = possibleHiddenWords.Count(s => !newGro.Allow(s));
            total += solutionsEliminated;
        }

        return total;
    }

    public static int? CountSolutionsRemainingAfterGuess(
        GuessResultOptimizer? gro,
        string guessWord,
        IReadOnlyCollection<string> possibleHiddenWords,
        int? giveUpAfter)
    {
        var total = 0;
        foreach (var solution in possibleHiddenWords)
        {
            var guessResult = GuessResult.ScoreWord(solution, guessWord);
            var possibleGro = GuessResultOptimizer.Create(guessResult);
            var newGro = gro is null ? possibleGro : gro.Combine(possibleGro);

            var solutionsRemaining = possibleHiddenWords.Count(newGro.Allow);
            total += solutionsRemaining;

            if (total > giveUpAfter)
                return null;
        }

        return total;
    }

    public static string? GetBestGuess(
        GameState state,
        GameSettings gameSettings,
        SolveSettings solveSettings,
        ConcurrentDictionary<GameState, string?>? resultCache,
        CancellationToken? cancellationToken = null)
    {
        var ct = cancellationToken ?? CancellationToken.None;
        Func<GameState, string?> func = solveSettings.UseParallel
            ? s =>
                GetBestGuessParallel(s, gameSettings, solveSettings, ct)
            : s =>
                GetBestGuessSingleThread(s, gameSettings, solveSettings, ct);

        if (resultCache is not null)
            return resultCache.GetOrAdd(state, func);

        var finalResult = func.Invoke(state);
        return finalResult;
    }

    private static string? GetBestGuessParallel(GameState state,
        GameSettings gameSettings,
        SolveSettings solveSettings,
        CancellationToken cancellationToken)
    {
        GuessResultOptimizer? gro;
        try
        {
            gro = state.MakeGuessResultOptimizer();
        }
        catch (GROException)
        {
            return null;
        }


        IReadOnlySet<string> remainingHiddenWords = gameSettings.FilterHiddenWords(gro).ToHashSet();

        if (solveSettings.MaxSolutionsToSearch.HasValue &&
            remainingHiddenWords.Count > solveSettings.MaxSolutionsToSearch)
        {
            var random = solveSettings.RandomSeed.HasValue
                ? new Random(solveSettings.RandomSeed.Value)
                : Random.Shared;
            remainingHiddenWords = remainingHiddenWords
                .OrderBy(_ => random.Next())
                .Take(solveSettings.MaxSolutionsToSearch.Value).ToHashSet();
        }

        var previousGuessKey = string.Join(";", state.PreviousGuesses.Select(x => x.Word()));

        var possibleGuesses = solveSettings.OptimalGuessDictionary
            .GetValueOrDefault(previousGuessKey, gameSettings.PossibleGuesses);

        if (solveSettings.EliminateUselessGuesses) possibleGuesses = FilterGuesses(possibleGuesses, gro);

        var leastTotal = int.MaxValue;

        var bestGuess = possibleGuesses
            .AsParallel()
            .WithCancellation(cancellationToken)
            .Select(guessWord =>
            {
                var possibilitiesRemaining =
                    CountSolutionsRemainingAfterGuess(gro, guessWord, remainingHiddenWords, leastTotal);

                if (possibilitiesRemaining < leastTotal)
                    leastTotal = possibilitiesRemaining.Value;

                return (guessWord, possibilitiesRemaining);
            })
            .Where(x => x.possibilitiesRemaining.HasValue)
            .OrderBy(x => x.possibilitiesRemaining)
            .ThenByDescending(x => remainingHiddenWords.Contains(x.guessWord))
            .Select(x => x.guessWord)
            .FirstOrDefault("");

        return bestGuess;
    }


    private static string? GetBestGuessSingleThread(GameState state,
        GameSettings gameSettings,
        SolveSettings solveSettings,
        CancellationToken cancellationToken)
    {
        GuessResultOptimizer? gro;
        try
        {
            gro = state.MakeGuessResultOptimizer();
        }
        catch (GROException)
        {
            return null;
        }


        IReadOnlySet<string> remainingHiddenWords = gameSettings.FilterHiddenWords(gro).ToHashSet();

        if (solveSettings.MaxSolutionsToSearch.HasValue &&
            remainingHiddenWords.Count > solveSettings.MaxSolutionsToSearch)
        {
            var random = solveSettings.RandomSeed.HasValue
                ? new Random(solveSettings.RandomSeed.Value)
                : Random.Shared;
            remainingHiddenWords = remainingHiddenWords
                .OrderBy(_ => random.Next())
                .Take(solveSettings.MaxSolutionsToSearch.Value).ToHashSet();
        }

        var previousGuessKey = string.Join(";", state.PreviousGuesses.Select(x => x.Word()));

        var possibleGuesses = solveSettings.OptimalGuessDictionary
            .GetValueOrDefault(previousGuessKey, gameSettings.PossibleGuesses);

        if (solveSettings.EliminateUselessGuesses) possibleGuesses = FilterGuesses(possibleGuesses, gro);

        var leastTotal = int.MaxValue;
        var bestGuess = possibleGuesses
            .Select(guessWord =>
            {
                var possibilitiesRemaining =
                    CountSolutionsRemainingAfterGuess(gro, guessWord, remainingHiddenWords, leastTotal);

                if (possibilitiesRemaining < leastTotal)
                    leastTotal = possibilitiesRemaining.Value;

                return (guessWord, possibilitiesRemaining);
            }).Where(x => x.possibilitiesRemaining.HasValue)
            .TakeWhile(_ => !cancellationToken.IsCancellationRequested)
            .OrderBy(x => x.possibilitiesRemaining)
            .ThenByDescending(x => remainingHiddenWords.Contains(x.guessWord))
            .Select(x => x.guessWord)
            .FirstOrDefault("");

        return bestGuess;
    }

    private static IReadOnlyList<string> FilterGuesses(IReadOnlyList<string> guesses, GuessResultOptimizer? gro)
    {
        if (gro is null) return guesses;

        var newGuesses = guesses.Where(gro.IsUseful).ToList();

        return newGuesses;
    }
}