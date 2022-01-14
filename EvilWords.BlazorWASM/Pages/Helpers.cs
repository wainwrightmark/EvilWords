using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using Generator.Equals;
using MudBlazor;

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
            _ => throw new ArgumentOutOfRangeException(nameof(resultColor), resultColor, null)
        };
    }

    public static Color GetColor(this GameState gameState, int i, char c)
    {
        var gro = gameState.MakeGuessResultOptimizer();
        if (gro is null) return Color.Info;

        var r = gro.GetResultColor(i, c);
        return r switch
        {
            ResultColor.Green => Color.Success,
            ResultColor.Yellow => Color.Warning,
            ResultColor.Red => Color.Error,
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