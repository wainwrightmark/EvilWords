using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using Generator.Equals;

namespace EvilWords;


public static class GameStateSerialization
{
    public static string Serialize(this GameState gs)
    {
        if (gs.IsFirstRound) return "";

        var r = string.Join("_",
            gs.PreviousGuesses.Select(x => x.Word() + "-" + x.ColorText())
        );

        return r;
    }

    public static GameState Deserialize(string s)
    {
        s = s.Trim('\'').ToUpperInvariant();
        if(string.IsNullOrWhiteSpace(s))
            return GameState.Empty;

        try
        {
            var grs = s.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(DeserializeGr).ToImmutableList();
            return new GameState(grs);
        }
        catch (Exception e)
        {
            return GameState.Empty;
        }

        

        static GuessResult DeserializeGr(string s)
        {
            var i = s.IndexOf('-');
            var word = s.Substring(0, i);
            var color = s.Substring(i + 1);

            var colors = word.Zip(color).Select(x => CharResult.Create(x.First, x.Second)).ToImmutableList();

            return new GuessResult(colors);
        }
    }
}

[Equatable]
public partial record GameState([property: OrderedEquality] IReadOnlyList<GuessResult> PreviousGuesses)
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
    public GameState Add(GuessResult guessResult) => new (PreviousGuesses.ToImmutableList().Add(guessResult).ToList());

    public GuessResultOptimizer? MakeGuessResultOptimizer()
    {
        try
        {
            GuessResultOptimizer? gro = null;
            foreach (var guess in PreviousGuesses)
            {
                var guessGro = GuessResultOptimizer.Create(guess);
                gro = gro is null ? guessGro : gro.Combine(guessGro);
            }

            return gro;
        }
        catch (Exception) //Might be an exception
        {
            return null;
        }

        
    }

}