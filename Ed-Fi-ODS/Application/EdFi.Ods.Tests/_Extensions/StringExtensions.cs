namespace EdFi.Ods.Tests._Extensions
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Trims the input string at any of the specified delimiter characters, if found.
        /// </summary>
        /// <param name="input">The string to be trimmed.</param>
        /// <param name="delimiter">The character or characters used to trim the string.</param>
        /// <returns>The trimmed string if a delimiter is found; otherwise the original string.</returns>
        public static string TrimAt(this string input, params char[] delimiter)
        {
            return input.Split(delimiter)[0];
        }
    }
}