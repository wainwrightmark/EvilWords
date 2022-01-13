namespace EvilWords;

public record SolveSettings
{
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> FiveLetterOptimalGuesses
        = new Dictionary<string, IReadOnlyList<string>>
        {
            { "", new List<string>() { "SOARE" } },
            {
                "SOARE", new List<string>()
                {
                    "RIYAL",
                    "LITED",
                    "THILK",
                    "CLINT",
                    "GLEBE",
                    "MYTHI",
                    "LIMAN",
                    "LINTY",
                    "SPREE",
                    "LETCH",
                    "DHUTI",
                    "MEYNT",
                    "GITCH",
                    "DENET",
                    "MARON",
                    "RICHT",
                    "CRUFT",
                    "CLOOT",
                    "CULTY",
                    "BELCH",
                    "MARCH",
                    "DEATH",
                    "PUTID",
                    "CLUNG",
                    "RATEL",
                    "BUNDT",
                    "SOWER",
                    "CULET",
                    "CYTON",
                    "PITHY",
                    "SPILT",
                    "GLINT",
                    "LATHY",
                    "TEPAL",
                    "BACHS",
                    "GUILT",
                    "CAROL",
                    "TRUED",
                    "LIGHT",
                    "REWTH",
                    "PUTON",
                    "BUTCH",
                    "CLING",
                    "THICK",
                    "FUNGI",
                    "DAULT",
                    "PUTTY",
                    "TANGY",
                    "CLINK",
                    "COULD",
                    "POWND",
                    "GUILD",
                    "FITCH",
                    "BENCH",
                    "NEWIE",
                    "CHANT",
                    "SOLVE",
                    "PELON",
                    "THUMP",
                    "LUSTY",
                    "GAULT",
                    "FETED",
                    "THUMB",
                    "SOLAR",
                    "SHIRE",
                    "HATCH",
                    "ABACK",
                    "HOARD",
                    "SAUTE",
                    "KNELT",
                    "THERE",
                    "BALMY",
                    "STAIR",
                    "SCOUR",
                    "GULCH",
                    "PILUM",
                    "LOSER",
                    "RETRO",
                    "WHELP",
                    "ONSET",
                    "USURP",
                    "MARSH",
                    "OMBRE",
                    "ARISE",
                    "DELTA",
                    "CHAOS",
                    "LOATH",
                    "ARSON",
                    "DIGIT",
                    "COUNT",
                    "SORRY",
                    "PROSE",
                    "MASON",
                    "OVARY",
                    "BRAVO",
                    "AZURE",
                    "OCEAN",
                    "WORSE",
                    "INERT",
                    "WORST",
                    "SALON",
                    "ROAST",
                    "ADORE",
                    "ROACH",
                    "PIANO",
                    "FLING",
                    "AROSE",
                    "LOFTY",
                    "STERN",
                    "POESY",
                    "ERASE",
                    "OVATE",
                    "SAVOR",
                    "COBRA",
                    "OPERA",
                    "SOAPY",
                    "VERSO"
                }
            }
        };

    public SolveSettings(){}

    public SolveSettings(
        int? randomSeed,
        int? maxSolutionsToSearch,
        bool useParallel,
        IReadOnlyDictionary<string, IReadOnlyList<string>> optimalGuessDictionary
    )
    {
        RandomSeed = randomSeed;
        MaxSolutionsToSearch = maxSolutionsToSearch;
        UseParallel = useParallel;
        OptimalGuessDictionary = optimalGuessDictionary;
    }

    public int? RandomSeed { get; init; } 
    public int? MaxSolutionsToSearch { get; init; }
    public bool UseParallel { get; init; } = true;

    public bool EliminateUselessGuesses { get; set; } = true;

    public bool GroupGuessResultsDuringCount { get; set; } = true;

    public IReadOnlyDictionary<string, IReadOnlyList<string>> OptimalGuessDictionary { get; init; }
}