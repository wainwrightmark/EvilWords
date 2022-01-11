namespace EvilWords;

public readonly record struct CharResult(char Character, ResultColor ResultColor)
{
    public static CharResult Create(char c, char colorChar)
    {
        var rc = colorChar switch
        {
            'R' => ResultColor.Red,
            'G' => ResultColor.Green,
            'Y' => ResultColor.Yellow,
            _ => throw new Exception(colorChar + " is not a color")
        };
        return new CharResult(c, rc);
    }
}