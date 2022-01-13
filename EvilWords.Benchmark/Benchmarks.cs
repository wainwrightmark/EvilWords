using System.Collections.Concurrent;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Extensions.Logging.Abstractions;

namespace EvilWords.Benchmark;

[SimpleJob(RunStrategy.Monitoring, invocationCount: 1, targetCount: 2)]
public class Benchmarks
{
    [Params(true)]
    public bool UseParallel { get; set; }
    
    [Params(true, false)]
    public bool UseFastElimination { get; set; }
    
    [Params(true, false)]
    public bool GamesAreEvil { get; set; }

    public int NumberOfGames { get; set; } = 100;

    [Params(123)]
    public int Seed { get; set; }

    //[Benchmark]
    public void RunGames()
    {
        var random = new Random(Seed);
        var gameSettings = GameSettings.FiveLetter;

        var solveSettings = new SolveSettings(
            Seed,
            null,
            UseParallel,
            SolveSettings.FiveLetterOptimalGuesses
        ){EliminateUselessGuesses = UseFastElimination};

        var dict = new ConcurrentDictionary<GameState, string?>();

        Solver.GetBestGuess(new GameState(ImmutableArray<GuessResult>.Empty),
            gameSettings,
            solveSettings,
            dict
        );

        for (var i = 0; i < NumberOfGames; i++)
        {

            string? word;
            if (GamesAreEvil)
                word = null;
            else 
                word = gameSettings.GetRandomHiddenWord(null, random, false);

            GameHelper.RunGame(word, gameSettings, solveSettings, dict, NullLogger.Instance);
        }
    }


    [Benchmark]
    public void TestBestFirstWord()
    {
        var dict = new ConcurrentDictionary<GameState, string>();
        var solveSettings = new SolveSettings(
            Seed,
            NumberOfGames,
            UseParallel,
            new Dictionary<string, IReadOnlyList<string>>()
        ){};


        Solver.GetBestGuess(new GameState(ImmutableArray<GuessResult>.Empty),
            GameSettings.FiveLetter,
            solveSettings,
            dict
        );
    }
    
}
