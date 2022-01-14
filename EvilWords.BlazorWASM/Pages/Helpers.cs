﻿using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using Generator.Equals;
using MudBlazor;
using MudBlazor.Extensions;

namespace EvilWords.BlazorWASM.Pages;

public static class Helpers
{
    public static Color GetColor(this ResultColor resultColor)
    {
        return resultColor switch
        {
            ResultColor.Green => Color.Success,
            ResultColor.Yellow => Color.Warning,
            ResultColor.Red => Color.Error,
            ResultColor.Blue => Color.Info,
            _ => throw new ArgumentOutOfRangeException(nameof(resultColor), resultColor, null)
        };
    }

    public static GuessResult GetFakeGuessResult(this GuessResultOptimizer? gro, string word)
    {
        if (gro is null)
            return new GuessResult(word.Select(c => new CharResult(c, ResultColor.Yellow)).ToImmutableArray());

        return new GuessResult(word.Select((c, i) =>

            new CharResult(c, gro.GetResultColor(i, c) ?? ResultColor.Blue)
        ).ToImmutableArray());
    }

    public static Color GetColor(this GuessResultOptimizer? gro, int i, char c)
    {
        if (gro is null) return Color.Info;

        var r = gro.GetResultColor(i, c);
        return r switch
        {
            ResultColor.Green => Color.Success,
            ResultColor.Yellow => Color.Warning,
            ResultColor.Red => Color.Error,
            ResultColor.Blue => Color.Info,
            _ => Color.Info
        };
    }
    
}

[Equatable]
public partial record RunHistory([property: OrderedEquality] IReadOnlyList<GameResult> Results)
{
    public static RunHistory Empty { get; } = new (ImmutableList<GameResult>.Empty);

    [Pure]
    public RunHistory Add(GameResult gameResult) => this with { Results = Results.ToImmutableList().Add(gameResult) };
}

public readonly record struct GameResult(int? Guesses, string HiddenWord)
{
    public bool WasWin() => Guesses.HasValue;
}

public readonly record struct SuggestionData(string BestWord, IReadOnlyList<string> RemainingPossibilities)
{
    public string PossibilityText
    {
        get
        {
            if (RemainingPossibilities.Count == 0)
                return "No Possible Solutions";
            if (RemainingPossibilities.Count == 1)
                return $"Must be '{RemainingPossibilities.Single()}'";
            if (RemainingPossibilities.Count <= 4)
                return $"'{string.Join(", ", RemainingPossibilities)}'";
            return $"{RemainingPossibilities.Count} Possible Solutions";
        }
    }
}