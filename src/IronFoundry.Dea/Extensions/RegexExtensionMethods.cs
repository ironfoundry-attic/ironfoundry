namespace System.Text.RegularExpressions
{
    public static class RegexExtensionMethods
    {
        public static string Postmatch(this Match match, string target)
        {
            int unmatchedIdx = match.Index + match.Length;
            return target.Substring(unmatchedIdx);
        }
    }
}