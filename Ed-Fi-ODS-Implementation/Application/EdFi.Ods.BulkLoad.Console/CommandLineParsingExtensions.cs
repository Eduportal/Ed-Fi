using System;

namespace EdFi.Ods.BulkLoad.Console
{
    internal static class CommandLineParsingExtensions
    {
        public static bool IsToken(this string value)
        {
            return value.StartsWith("/") || value.StartsWith("-");
        }

        public static TokenType Token(this string value)
        {
            if (!value.IsToken()) return TokenType.Unknown;
            switch (value.ToLower()[1])
            {
                case 'h':
                    return TokenType.Help;
                case 'd':
                    return TokenType.DatabaseName;
                case 'c':
                    return TokenType.ConnectionString;
                case 'f':
                    return TokenType.SourceFolder;
                case 'm':
                    return TokenType.Manifest;
                default:
                    throw new ArgumentException(string.Format("Bad Command Line Switch: {0}", value));
            }
        }
    }
}