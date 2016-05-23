using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace EdFi.Common.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns a string that has the first character converted to lower-case.
        /// </summary>
        /// <param name="text">The text to be processed.</param>
        /// <returns>A string that has the first character converted to lower-case.</returns>
        public static string ToCamelCase(this string text)
        {
            if (String.IsNullOrWhiteSpace(text))
                return text;

            if (text.Length == 1)
                return text.ToLower();

            return Char.ToLower(text[0]) + text.Substring(1);
        }

        /// <summary>
        /// Returns a string that has the first character converted to upper-case.
        /// </summary>
        /// <param name="text">The text to be processed.</param>
        /// <returns>A string that has the first character converted to upper-case.</returns>
        public static string ToMixedCase(this string text)
        {
            if (String.IsNullOrWhiteSpace(text) || text.Length == 1)
                return text;

            return Char.ToUpper(text[0]) + text.Substring(1);
        }

        public static string[] SplitCamelCase(this string text)
        {
            return Regex.Replace(text, "([A-Z])", " $1", RegexOptions.Compiled).Trim().Split(' ');
        }

        /// <summary>
        /// Breaks the supplied text into individual words using mixed-casing conventions augmented by delimiters.
        /// </summary>
        /// <param name="compositeTerm">The text to process for display.</param>
        /// <param name="delimiters">Additional delimiters to use as word breaks.</param>
        /// <returns>The text processed for display.</returns>
        public static string NormalizeCompositeTermForDisplay(this string compositeTerm, params char[] delimiters)
        {
            string delimiterExpression = delimiters.Length == 0 
                ? string.Empty
                : "(?=[" + string.Join(string.Empty, delimiters.Select(c => @"\" + c)) + "])?";

            var parts = Regex.Matches(compositeTerm, string.Format(
                @"((?:[a-z]+|[A-Z]+)(?:[a-z0-9]+)?{0})",
                delimiters.Length == 0 ? string.Empty : delimiterExpression));

            string displayText = string.Empty;

            foreach (Match part in parts)
                displayText += part.Value.ToMixedCase() + " ";

            return displayText.TrimEnd(' ');
        }

        public static string SingleQuoted(this string text)
        {
            return "'" + text + "'";
        }

        public static string DoubleQuoted(this string text)
        {
            return "\"" + text + "\"";
        }

        public static string Parenthesize(this string text)
        {
            return "(" + text + ")";
        }

        public static bool TryTrimSuffix(this string text, string suffix, out string trimmedText)
        {
            trimmedText = null;

            if (text == null)
                return false;

            int pos = text.LastIndexOf(suffix);

            if (pos < 0)
                return false;

            if (text.Length - pos == suffix.Length)
            {
                trimmedText = text.Substring(0, pos);
                return true;
            };

            return false;
        }

        public static string TrimSuffix(this string text, string suffix)
        {
            string trimmedText;

            if (TryTrimSuffix(text, suffix, out trimmedText))
                return trimmedText;

            return text;
        }


        public static bool TryTrimPrefix(this string text, string prefix, out string trimmedText)
        {
            trimmedText = null;

            if (text == null)
                return false;

            if (text.StartsWith(prefix))
            {
                if (text.Length == prefix.Length)
                    trimmedText = string.Empty;
                else
                    trimmedText = text.Substring(prefix.Length);

                return true;
            }

            return false;
        }
        
        public static string TrimPrefix(this string text, string prefix)
        {
            string trimmedText;

            if (TryTrimPrefix(text, prefix, out trimmedText))
                return trimmedText;

            return text;
        }

        /// <summary>
        /// Provides an extension-based overload method for performing case-insensitive equality checks more succinctly.
        /// </summary>
        /// <param name="text">The string to be evaluated.</param>
        /// <param name="compareText">The string to compare against.</param>
        /// <returns><b>true</b> if the strings are equal (ignoring case); otherwise <b>false</b>.</returns>
        public static bool EqualsIgnoreCase(this string text, string compareText)
        {
            if (text == null)
                return compareText == null;

            return text.Equals(compareText, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool StartsWithIgnoreCase(this string text, string compareText)
        {
            if (text == null)
                return compareText == null;

            return text.StartsWith(compareText, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool EndsWithIgnoreCase(this string text, string compareText)
        {
            if (text == null)
                return compareText == null;

            return text.EndsWith(compareText, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool ContainsIgnoreCase(this string text, string compareText)
        {
            if (text == null || compareText == null)
                return false;

            return text.IndexOf(compareText, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }
    }
}
