using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using Generator.Equals;
using MudBlazor;

namespace EvilWords.Blazor.Pages;

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

    
}

[Equatable]
public partial record RunHistory([property: OrderedEquality] ImmutableList<GameResult> Results)
{
    public static RunHistory Empty { get; } = new (ImmutableList<GameResult>.Empty);

    [Pure]
    public RunHistory Add(GameResult gameResult) => this with { Results = Results.Add(gameResult) };
}

public readonly record struct GameResult(int? Guesses, string HiddenWord)
{
    public bool WasWin => Guesses.HasValue;
}