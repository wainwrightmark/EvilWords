using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using Generator.Equals;

namespace EvilWords;

[Equatable]
public partial record GameState([property: OrderedEquality] ImmutableArray<GuessResult> PreviousGuesses)
{

    public static GameState Empty { get; } = new (ImmutableArray<GuessResult>.Empty);

    /// <inheritdoc />
    public override string ToString()
    {
        if (PreviousGuesses.Any())
            return string.Join(", ", PreviousGuesses);
        return "Initial Game State";
    }

    public bool IsFirstRound => !PreviousGuesses.Any();

    public bool IsWin => PreviousGuesses.Any(x => x.IsCorrect);

    [Pure]
    public GameState Add(GuessResult guessResult) => new (PreviousGuesses.Add(guessResult));

    public GuessResultOptimizer? MakeGuessResultOptimizer()
    {
        GuessResultOptimizer? gro = null;
        foreach (var guess in PreviousGuesses)
        {
            var guessGro = GuessResultOptimizer.Create(guess);
            gro = gro is null ? guessGro : gro.Combine(guessGro);
        }

        return gro;
    }

}