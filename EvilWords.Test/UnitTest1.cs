using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging.Xunit;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace EvilWords.Test;
public class UnitTest1
{
    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    public ITestOutputHelper TestOutputHelper { get; set; }

    [Theory]
    [InlineData("BRASS", "SOARE", "YRGYR")]
    [InlineData("TRASH", "BRASS", "RGGGR")]
    [InlineData("ONION", "NANNY", "YRYRR")]
    [InlineData("ONION", "NANYN", "YRRRG")]
    [InlineData("CIGAR", "CRAIG", "GYYYY")]
    public void TestColors(string hiddenWord, string guess, string expectedColors)
    {
        var r = GuessResult.ScoreWord(hiddenWord.ToUpperInvariant(), guess.ToUpperInvariant());

        r.ColorText().Should().Be(expectedColors.ToUpperInvariant());
    }

    [Fact]
    public void TestEliminations()
    {
        var settings = GameSettings.FiveLetter;

        foreach (var hiddenWord in settings.PossibleHiddenWords.Take(100)) //only try 100 to make it faster
        {
            foreach (var guess in settings.PossibleGuesses)
            {
                var guessResult = GuessResult.ScoreWord(hiddenWord, guess);
                var gro = guessResult.ToGro();

                gro.Allow(hiddenWord).Should().BeTrue($"'{hiddenWord}' should still be allowed after guessing '{guess}'");

                if (guess != hiddenWord)
                {

                    var r1= gro.Allow(guess);

                    if (r1)
                    {
                        r1.Should().BeFalse($"'{guess}' should eliminate itself if guessed under '{hiddenWord}'");
                    }

                    
                }
                    
            }
        }
    }
    
    
    
    [Theory]
    [InlineData("SOARE",  1, 1000, 123, true)]
    [InlineData("SOLVE",  2, 1000, 123, true)]
    [InlineData("ANGER", 4, 100, 123, true)]
    [InlineData("ANGER", 4, 100, 456, true)]
    [InlineData("ANGER", 4, null, 123, true)]
    [InlineData("ANGER", 4, null, 123, false)]
    [InlineData("ANGER", 4, null, 456, true)]
    [InlineData("ANGER", 4, null, 456, false)]
    
    public void TestGame(string hiddenWord,  int expectedRounds, int? maxSolutionsToTry, int seed, bool useFastChecking)
    {
        var settings = GameSettings.FiveLetter;
        var logger = new XunitLogger(TestOutputHelper, "TestGame");
        var solveSettings = new SolveSettings(
            seed, maxSolutionsToTry,
            true,
            SolveSettings.FiveLetterOptimalGuesses,
            new ConcurrentDictionary<GameState, string>()
        ){UseFastChecking = useFastChecking};
        
        var totalRounds = GameHelper.RunGame(hiddenWord.ToUpperInvariant(), settings, solveSettings, logger);
        
        totalRounds.Should().Be(expectedRounds);
    }

    [Theory]
    [InlineData(100, 10, 123, false)]
    [InlineData(100, 10, 456, false)]
    [InlineData(100, 10, 123, true)]
    [InlineData(100, 10, 456, true)]
    public void TestManyGames(int gamesToPlay, int? stochasticSolutions, int seed, bool useFastChecking)
    {
        var random = new Random(seed);
        var settings = GameSettings.FiveLetter;
        var totalRounds = 0;
        var maxRounds = 0;

        var solveSettings = new SolveSettings(
            seed,
            stochasticSolutions,
            true,
            SolveSettings.FiveLetterOptimalGuesses,
            new ConcurrentDictionary<GameState, string>()
        ){UseFastChecking = useFastChecking};

        for (var i = 0; i < gamesToPlay; i++)
        {
            var word = settings.GetRandomHiddenWord(null, random, false);
            var logger = new XunitLogger(TestOutputHelper,$"Game {i}: " + word);
            var rounds = GameHelper.RunGame(word, settings, solveSettings, logger);
            totalRounds += rounds;
            maxRounds = Math.Max(maxRounds, rounds);
        }

        var meanTotalRounds = ((double) totalRounds) / gamesToPlay;

        TestOutputHelper.WriteLine($"Mean Total Rounds: {meanTotalRounds}");
        TestOutputHelper.WriteLine($"Max Total Rounds: {maxRounds}");


    }

    [Theory]
    [InlineData("SOLVE", "SOARE", 1)]
    [InlineData("SORRY", "SOARE", 1)]
    [InlineData("SOAPY", "SOARE", 1)]
    [InlineData("SPORE", "SOARE", 6)]
    [InlineData("FERAL", "SOARE", 61)]
    [InlineData("ANGER", "SOARE", 61)]
    [InlineData("JAUNT", "JUJUS", 1)]
    [InlineData("GUESS", "JUJUS", 1)]
    [InlineData("SHAVE", "JUJUS", 466)]
    [InlineData("ARTSY", "JUJUS", 466)]
    public void TestRemainingWords(string hiddenWord, string guess, int expected)
    {
        var settings = GameSettings.FiveLetter;
        var guessResult = GuessResult.ScoreWord(hiddenWord.ToUpperInvariant(), guess.ToUpperInvariant());
        var gro = guessResult.ToGro();

        var remaining = settings.FilterHiddenWords(gro);

        remaining.Should().Contain(hiddenWord);

        if (remaining.Count != expected)
        {
            if(remaining.Count <= 10)
                TestOutputHelper.WriteLine(string.Join(", ", remaining));
            else
            {
                TestOutputHelper.WriteLine(string.Join(", ", remaining.Take(10)) + "...");
            }

            remaining.Count.Should().Be(expected);
        }
        
        
    }

    [Theory]
    [InlineData("JUJUS")]
    [InlineData("SOARE")]
    public void TestTotals(string guess)
    {
        var settings = GameSettings.FiveLetter;
        var sw = Stopwatch.StartNew();

        var total = Solver.CountSolutionsEliminatedByGuess(
            null,
            guess.ToUpperInvariant(),
            settings.PossibleHiddenWords);

        var fullTotal = settings.PossibleHiddenWords.Count * settings.PossibleHiddenWords.Count;

        var totalRemaining = fullTotal - total;

        var average = total / settings.PossibleHiddenWords.Count;
        var averageRemaining = totalRemaining / settings.PossibleHiddenWords.Count;

        sw.Stop();
        
        TestOutputHelper.WriteLine(sw.Elapsed.ToString());
        TestOutputHelper.WriteLine($"{guess}");
        TestOutputHelper.WriteLine($"Total: {total}");
        TestOutputHelper.WriteLine($"TotalRemaining: {totalRemaining}");
        TestOutputHelper.WriteLine($"Average: {average}");
        TestOutputHelper.WriteLine($"AverageRemaining: {averageRemaining}");
    }
    
    
    [Theory]
    [InlineData(10, 123)]
    [InlineData(100, 123)]
    [InlineData(100, 456)]
    [InlineData(200, 456)]
    [InlineData(400, 456)]
    [InlineData(null, 456)]
    public void TestBestFirstGuess(int? possiblesToTake, int seed)
    {
        var sw = Stopwatch.StartNew();
        var state = new GameState(ImmutableArray<GuessResult>.Empty);

        var solveSettings = new SolveSettings(seed, possiblesToTake,
            true,
            new Dictionary<string, IReadOnlyList<string>>(),
            null
        ){UseFastChecking = true};

        var bestGuess = Solver.GetBestGuess(state, GameSettings.FiveLetter, solveSettings);
        sw.Stop();
        
        TestOutputHelper.WriteLine(sw.Elapsed.ToString());
        TestOutputHelper.WriteLine(bestGuess);
    }

    [Theory]
    [InlineData("Soare")]
    [InlineData("anger")]  
    [InlineData("jujus")]  
    public void TestWorstCase(string guess)
    {
        guess = guess.ToUpperInvariant();
        var sw = Stopwatch.StartNew();
        var guessResult = GuessResult.GetWorstCase(guess, GameState.Empty, GameSettings.FiveLetter);

        sw.Stop();

        var possibleSolutions = GameSettings.FiveLetter.PossibleHiddenWords
            .Where(hw =>
            {
                var resultProduced = GuessResult.ScoreWord(hw, guess);

            return resultProduced.Equals(guessResult);
        }).ToList();


        TestOutputHelper.WriteLine($"{sw.Elapsed} {guessResult} {string.Join(",", possibleSolutions.Take(5))}   ({possibleSolutions.Count} options)");
    }

    [Theory]
    [InlineData("soare")]
    public void FindBestStage2Words(string firstGuess)
    {
        var settings = GameSettings.FiveLetter;
        var guessResults =
            settings.PossibleHiddenWords.Select(hiddenWord => 
                    GuessResult.ScoreWord(hiddenWord, firstGuess.ToUpperInvariant()))
                .Distinct().ToList();

        var solveSettings = new SolveSettings(
            null, 
            null, 
            true,
            new Dictionary<string, IReadOnlyList<string>>(),
            null);

        TestOutputHelper.WriteLine($"{guessResults.Count} possibleResults");

        var stage2WordsSoFar = new HashSet<string>();

        foreach (var guessResult in guessResults)
        {
            var gameState = new GameState(ImmutableArray<GuessResult>.Empty.Add(guessResult));
            var bestGuess = Solver.GetBestGuess(gameState, GameSettings.FiveLetter, solveSettings);

            if (stage2WordsSoFar.Add(bestGuess))
            {
                TestOutputHelper.WriteLine(bestGuess);
            }
        }

        TestOutputHelper.WriteLine($"{stage2WordsSoFar.Count} words");
    }

    [Theory]
    [InlineData("brass", "soare")]
    [InlineData("feral", "soare")]
    [InlineData("anger", "soare")]
    public void TestOptimizationStage2(string hiddenWord, string firstGuess)
    {
        var sw = Stopwatch.StartNew();
        var gr1 = GuessResult.ScoreWord(hiddenWord.ToUpperInvariant(), firstGuess.ToUpperInvariant());

        var state = new GameState(ImmutableArray<GuessResult>.Empty.Add(gr1));

        var bestGuess = Solver.GetBestGuess(state,
            GameSettings.FiveLetter,
            new SolveSettings(null, null,true, new Dictionary<string, IReadOnlyList<string>>(), null)
        );
        sw.Stop();
        
        TestOutputHelper.WriteLine(sw.Elapsed.ToString());
        TestOutputHelper.WriteLine(bestGuess);
    }


    [Theory]
    [InlineData(123,100)]
    public void TestFastChecking(int seed, int rounds)
    {
        var settings = GameSettings.FiveLetter;
        for (var i = 0; i < rounds; i++)
        {
            var random = new Random(seed + i);
            var solutionsToSearch = random.Next(20);
            var newSeed = random.Next();
            var settingsSlow = new SolveSettings(newSeed, solutionsToSearch, false,
                new Dictionary<string, IReadOnlyList<string>>(), null)
            {
                UseFastChecking = false
            };
            
            
            var settingsFast = new SolveSettings(newSeed, solutionsToSearch, false,
                new Dictionary<string, IReadOnlyList<string>>(), null)
            {
                UseFastChecking = true
            };

            var hiddenWord = settings.GetRandomHiddenWord(null, random, false);
            var guess1 = settings.GetRandomHiddenWord(null, random, false);
            var guess2 = settings.GetRandomHiddenWord(null, random, false);

            var guessResult1 = GuessResult.ScoreWord(hiddenWord, guess1);

            var gs = new GameState(ImmutableArray<GuessResult>.Empty.Add(guessResult1));

            var slowResult = Solver.GetBestGuess(gs, GameSettings.FiveLetter, settingsSlow);
            var fastResult = Solver.GetBestGuess(gs, GameSettings.FiveLetter, settingsFast);

            slowResult.Should().Be(fastResult,
                $"Guessing {guess2} for {hiddenWord} after {guessResult1.ColorText()} should produce {slowResult} (seed {seed + i}");

        }
    }
    
}