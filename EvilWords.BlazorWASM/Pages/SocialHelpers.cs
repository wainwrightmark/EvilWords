namespace EvilWords.BlazorWASM.Pages;

public static class SocialHelpers
{
    public const string MyURL = "https://wainwrightmark.github.io/EvilWords";

    public static string GetFacebookShareURL(string quote)
    {
        var url = System.Net.WebUtility.UrlEncode(MyURL);
        var quote1 = (System.Net.WebUtility.UrlEncode(quote)) ;

        return $"https://www.facebook.com/sharer/sharer.php?u={url}&quote={quote1}";
    }

    public static string GetTwitterShareUrl(string quote)
    {
        var url = System.Net.WebUtility.UrlEncode(MyURL);
        var quote1 = (System.Net.WebUtility.UrlEncode(quote)) ;

        return $"https://twitter.com/intent/tweet?url=@{url}&text=@{quote1}";
    }
}