namespace BaseMod;

static class Util
{
    internal static string RStrip(this string s, string suffix)
    {
        if (s != null && suffix != null && s.EndsWith(suffix))
        {
            return s[..^suffix.Length];
        }
        return s;
    }
}