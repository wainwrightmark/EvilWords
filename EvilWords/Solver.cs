namespace EvilWords;

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

    public static string GetBestGuess(
        GameState state,
        GameSettings gameSettings,
        SolveSettings solveSettings)
    {
        if (solveSettings.ResultCache is not null)
            return solveSettings.ResultCache.GetOrAdd(state, GetBestGuess1());

        var finalResult = GetBestGuess1();
        return finalResult;

        string GetBestGuess1()
        {
            var gro = state.MakeGuessResultOptimizer();

            IReadOnlyCollection<string> remainingHiddenWords =
                gro is null
                    ? gameSettings.PossibleHiddenWords
                    : gameSettings.PossibleHiddenWords.Where(gro.Allow).ToHashSet();

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
                .TryGetValue(previousGuessKey, out var pgl)
                ? pgl
                : gameSettings.PossibleGuesses;


            string bestGuess;

            if (solveSettings.UseFastChecking)
            {
                var leastTotal = int.MaxValue;

                if (solveSettings.UseParallel)
                {
                    bestGuess = possibleGuesses
                        .AsParallel()
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
                }
                else
                {
                    bestGuess = possibleGuesses
                        .Select(guessWord =>
                        {
                            var possibilitiesRemaining =
                                CountSolutionsRemainingAfterGuess(gro, guessWord, remainingHiddenWords, leastTotal);

                            if (possibilitiesRemaining < leastTotal)
                                leastTotal = possibilitiesRemaining.Value;

                            return (guessWord, possibilitiesRemaining);

                        }).Where(x => x.possibilitiesRemaining.HasValue)
                        .OrderBy(x => x.possibilitiesRemaining)
                        .ThenByDescending(x => remainingHiddenWords.Contains(x.guessWord))
                        .Select(x => x.guessWord)
                        .FirstOrDefault("");
                }
            }
            else
            {
                (string guessWord, int total)[] eliminations;

                if (solveSettings.UseParallel)
                {
                    eliminations = possibleGuesses
                        .AsParallel()
                        .Select(guessWord => (guessWord,
                            total: CountSolutionsEliminatedByGuess(gro, guessWord, remainingHiddenWords)))
                        .ToArray();
                }
                else
                {
                    eliminations = possibleGuesses
                        .Select(guessWord => (guessWord,
                            total: CountSolutionsEliminatedByGuess(gro, guessWord, remainingHiddenWords)))
                        .ToArray();
                }

                var bestGuesses =
                    eliminations.OrderByDescending(x => x.total)
                        .ThenByDescending(x => remainingHiddenWords.Contains(x.guessWord))
                        .ToList();

                bestGuess = bestGuesses.First().guessWord;
            }

            return bestGuess;

        }
    }
}