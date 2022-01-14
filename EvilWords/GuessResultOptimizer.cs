using Generator.Equals;

namespace EvilWords;

public class GROException : Exception
{
    public GROException(string message) : base(message)
    {

    }
}

[Equatable]
public partial record GuessResultOptimizer(int ExpectedLength,
    [property:SetEquality] (char c, int index)[] KnownCharacters,
    [property:SetEquality] ILookup<char, int> KnownBadCharacterIndexes,
    [property:SetEquality] (char c, int minCount)[] MinMultiplicities,
    [property:OrderedEquality] int?[] MaxMultiplicities)
{

    public ResultColor? GetResultColor(int index, char c)
    {
        if (!char.IsLetter(c)) return null;
        c = char.ToUpperInvariant(c);

        if (MaxMultiplicities[c - 'A'] == 0) return ResultColor.Red;
        if (KnownCharacters.Contains((c, index))) return ResultColor.Green;
        if (KnownBadCharacterIndexes[c].Contains(index)) return ResultColor.Red;


        if (MinMultiplicities.Any(x => x.c == c && x.minCount > 
            KnownCharacters.Count(k=> k.c == c))) return ResultColor.Yellow;
        return null;
    }

    /// <summary>
    /// Create an empty GuessResultOptimizer
    /// </summary>
    public static GuessResultOptimizer Create(GameSettings settings)
        => new (settings.WordLength,
            Array.Empty<(char c, int index)>(),
            new List<(char, int)>().ToLookup(x=>x.Item1, x => x.Item2),
            Array.Empty<(char c, int minCount)>(),
            Enumerable.Range(0, 'Z' - 'A' + 1)
                .Select(_ => null as int?).ToArray()
            
            );

    public static GuessResultOptimizer Create(GuessResult gr)
    {
        var knownCharacters = gr.Results
            .Select((x, i) => (x, i))
            .Where(x => x.x.ResultColor == ResultColor.Green)
            .Select(x => (x.x.Character, x.i)).ToArray();

        var knownBadCharacterIndexes = gr.Results
            .Select((x, i) => (x, i))
            .Where(x => x.x.ResultColor == ResultColor.Yellow)
            .Select(x => (x.x.Character, x.i)).ToLookup(x => x.Character, x => x.i);

        var minMultiplicities = gr.Results
            .Where(x => x.ResultColor is ResultColor.Green or ResultColor.Yellow)
            .GroupBy(x => x.Character)
            .Select(x => (x.Key, x.Count()))
            .ToArray();

        var maxMultiplicities = Enumerable.Range(0, 'Z' - 'A' + 1)
            .Select(i => 'A' + i)
            .Select(c =>
            {
                if (gr.Results.Any(x => x.Character == c && x.ResultColor == ResultColor.Red))
                {
                    var max = gr.Results.Count(x => x.Character == c && x.ResultColor is ResultColor.Green or ResultColor.Yellow);
                    return max;
                }

                return null as int?;
            }).ToArray();

        return new GuessResultOptimizer(gr.Results.Count, knownCharacters,
            knownBadCharacterIndexes,
            minMultiplicities,
            maxMultiplicities);
    }

    /// <summary>
    /// Combine this with another guess result optimizer
    /// </summary>
    public GuessResultOptimizer Combine(GuessResultOptimizer gr2)
    {
        if (ExpectedLength != gr2.ExpectedLength)
            throw new GROException($"Cannot combine GROs with lengths {ExpectedLength} and {gr2.ExpectedLength}");

        var newKnownCharacters = KnownCharacters.Concat(gr2.KnownCharacters)
            .GroupBy(x => x.index, x => x.c)
            .Select(x =>
            {
                if (x.Distinct().Count() == 1)
                    return (x.First(), x.Key);

                throw new GROException($"Cannot have both {string.Join(" and ", x.Distinct())} at index {x.Key}");
            }).ToArray();

        ILookup<char, int> newKnownBadCharacterIndexes;

        if (KnownBadCharacterIndexes.Any())
        {
            if (gr2.KnownBadCharacterIndexes.Any())
            {
                newKnownBadCharacterIndexes = KnownBadCharacterIndexes
                    .Concat(gr2.KnownBadCharacterIndexes)
                    .SelectMany(x => x.Select(index => (x.Key, index)))
                    .ToLookup(x => x.Key, x => x.index);
            }
            else
            {
                newKnownBadCharacterIndexes = KnownBadCharacterIndexes;
            }
        }
        else
        {
            newKnownBadCharacterIndexes = gr2.KnownBadCharacterIndexes;
        }


        var newMinMultiplicities = MinMultiplicities.Concat(gr2.MinMultiplicities)
            .GroupBy(x => x.c, x => x.minCount)
            .Select(x => (x.Key, x.Max())).ToArray();

        var newMaxMultiplicities = MaxMultiplicities.Zip(gr2.MaxMultiplicities)
            .Select(pair =>
            {
                if (pair.First.HasValue)
                {
                    if (pair.Second.HasValue && pair.Second.Value != pair.First.Value)
                        throw new GROException($"Char has different max multiplicities {pair.First} and {pair.Second}");
                    return pair.First;
                }

                return pair.Second;
            }).ToArray();

        return new GuessResultOptimizer(ExpectedLength,
            newKnownCharacters,
            newKnownBadCharacterIndexes,
            newMinMultiplicities,
            newMaxMultiplicities);
    }

    /// <summary>
    /// Returns whether a guess is useful
    /// A guess is useless if:
    /// There is at least one impossible letter
    /// All but one letters is either impossible or already in its fixed position
    /// </summary>
    public bool IsUseful(string guess)
    {
        if (guess.Length != ExpectedLength) return false;

        var nonGreenCharacters = 0;
        var redCharacters = 0;

        foreach (var grouping in guess.Select((c, i) => (c, i))
                     .Except(KnownCharacters) //Exclude green characters
                     .GroupBy(x => x.c, x => x.Item2))
        {
            var size = grouping.Count();
            nonGreenCharacters += size;
            var max = MaxMultiplicities[grouping.Key - 'A'];
            if (max < size)
            {
                redCharacters += (size - max.Value);
            }
        }

        if (redCharacters > 0 && redCharacters + 1 >= nonGreenCharacters)
            return false; //We already know that every character but one will be either red or green for the correct solution

        return true;
    }

    /// <summary>
    /// Check if this GuessResult eliminates a possible solution
    /// </summary>
    public bool Allow(string possibleSolution)
    {
        if (possibleSolution.Length != ExpectedLength)
            throw new GROException(
                $"{possibleSolution} was length {possibleSolution.Length} but expected length {ExpectedLength}");

        foreach (var (c, index) in KnownCharacters)
            if (possibleSolution[index] != c)
                return false;

        var remainingMinMultiplicities = MinMultiplicities.ToList();

        foreach (var grouping in possibleSolution.Select((c,i)=> (c,i)).GroupBy(x => x.c,x=>x.i)) //TODO maybe optimize this
        {
            var keyChar = grouping.Key;
            var number = grouping.Count();

            var max = MaxMultiplicities[keyChar - 'A'];
            if (max < number) return false;

            var minMultIndex = remainingMinMultiplicities.FindIndex(x => x.c == keyChar);

            if (minMultIndex >= 0)
            {
                if (number < remainingMinMultiplicities[minMultIndex].minCount)
                    return false;
                remainingMinMultiplicities.RemoveAt(minMultIndex);

                if (KnownBadCharacterIndexes[keyChar].Intersect(grouping).Any())//At least one of these characters is in a bad place
                    return false;
            }
        }

        if (remainingMinMultiplicities.Any()) return false;

        return true;
    }


}