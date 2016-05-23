using System;
using System.Text.RegularExpressions;

namespace EdFi.Ods.Common.Utils.Extensions
{
    public static class StringExtensions
    {
        public static bool AsOptionalBool(this string s)
        {
            bool value;
            return Boolean.TryParse(s, out value) && value;
        }

        public static string ReplaceLastOccurrence(this string s, string find, string replace)
        {
            if (!s.Contains(find))
                return s;
            int Place = s.LastIndexOf(find, StringComparison.Ordinal);
            string result = s.Remove(Place, find.Length).Insert(Place, replace);
            return result;
        }

        public static string JoinWithCharacter(this string baseUri, string path, char joinCharacter)
        {
            baseUri = baseUri.TrimEnd(joinCharacter);
            path = path.TrimStart(joinCharacter);
            return baseUri + joinCharacter + path;
        }
    }
}