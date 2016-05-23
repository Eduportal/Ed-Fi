using System;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public static class StringExtensions
       {
        public static bool LastSegmentEquals(this string text, string expectedLastSegment)
        {
            try
            {
                var parts = text.Split('.');
                return parts[parts.Length - 1].Equals(expectedLastSegment, StringComparison.InvariantCultureIgnoreCase);
            }
            catch { return false; }
        }
    }
}
