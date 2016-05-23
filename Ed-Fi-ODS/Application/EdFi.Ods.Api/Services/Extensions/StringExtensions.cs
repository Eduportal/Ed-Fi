namespace EdFi.Ods.Api.Services.Extensions
{
    public static class StringExtensions
    {
        public static string Quoted(this string text)
        {
            return "\"" + text + "\"";
        }

        public static string Unquoted(this string text)
        {
            if (text == null)
                return null;

             return text.Trim('"');
        }
    }
}