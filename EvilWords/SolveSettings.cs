namespace EvilWords;

public record SolveSettings
{
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> FiveLetterOptimalGuesses
        = new Dictionary<string, IReadOnlyList<string>>
        {
            { "", new List<string>() { "ARISE" } },
            {
                "ARISE", new List<string>()
                {
                    "RADAR",
                    "RORTY",
                    "STORM",
                    "COULD",
                    "OBANG",
                    "SLOBS",
                    "CANTY",
                    "GAULT",
                    "POTCH",
                    "LETCH",
                    "LORRY",
                    "DENET",
                    "PLANT",
                    "CADGY",
                    "CHYND",
                    "PITCH",
                    "COUNT",
                    "POYNT",
                    "ABASE",
                    "PATLY",
                    "DEMPT",
                    "LINED",
                    "SEWEN",
                    "LINTY",
                    "NATAL",
                    "CLOGS",
                    "FIRST",
                    "GONCH",
                    "TWEEP",
                    "KLONG",
                    "GLINT",
                    "BACHS",
                    "NOULD",
                    "SWORN",
                    "CEDER",
                    "SHIRK",
                    "ARGUE",
                    "CLINT",
                    "PUTON",
                    "LINGO",
                    "CLAPT",
                    "UNAPT",
                    "ISLET",
                    "ROUGH",
                    "SNAIL",
                    "KNELT",
                    "FLOUT",
                    "AMASS",
                    "DROPT",
                    "BETEL",
                    "TICED",
                    "DONUT",
                    "SLASH",
                    "ORBIT",
                    "VINED",
                    "KNOTS",
                    "SLOTH",
                    "LAUCH",
                    "SHOUT",
                    "ASIDE",
                    "SIEGE",
                    "ALAND",
                    "SPIRE",
                    "CLOTH",
                    "MONTH",
                    "ABRAM",
                    "THACK",
                    "RIDGE",
                    "CHANT",
                    "STAIR",
                    "WORTH",
                    "EXIST",
                    "HANDY",
                    "FLING",
                    "GLYPH",
                    "ALIEN",
                    "NYMPH",
                    "BLOCK",
                    "SAINT",
                    "SWAMP",
                    "MARSH",
                    "SHIED",
                    "INANE",
                    "QUASI",
                    "ARISE",
                    "PLEON",
                    "BUILT",
                    "RENEW",
                    "ARSON",
                    "PUTTY",
                    "PEARL",
                    "DUTCH",
                    "PROSE",
                    "AIDER",
                    "ASCOT",
                    "IRATE",
                    "WORST",
                    "WAIVE",
                    "MEDIA",
                    "DAISY",
                    "ARENA",
                    "SEPIA",
                    "ACRID",
                    "POISE",
                    "AROSE",
                    "LOFTY",
                    "AFIRE",
                    "ERASE",
                    "PARSE",
                    "AISLE",
                    "RAISE",
                    "AMISS",
                    "SKIER",
                    "RINSE",
                    "VERSO",
                    "ARTSY"
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