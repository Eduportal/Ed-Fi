using System;
using System.Collections.Generic;
using System.Linq;

namespace EdFi.Ods.BulkLoad.Console
{
    public enum TokenType
    {
        Unknown,
        Help,
        DatabaseName,
        SourceFolder,
        ConnectionString,
        Manifest,
    }

    public class CommandLineParser
    {
        private class Option
        {
            public TokenType Token { get; set; }
            public string Value { get; set; }
        }

        public Dictionary<TokenType, string> Parse(IEnumerable<string> args)
        {
            var argsArray = args as string[] ?? args.ToArray();
            var list = new List<Option>();
            Option currentOption = null;

            foreach (var arg in argsArray)
            {
                if (arg.IsToken())
                {
                    var token = arg.Token();
                    currentOption = new Option { Token = token, Value = string.Empty };
                    list.Add(currentOption);
                    continue;
                }

                if (currentOption == null)
                    throw new ArgumentException(string.Format("Bad commandline options [{0}]", string.Join(" ", argsArray)));

                // rejoin a connection string if it has spaces
                var spacer = string.IsNullOrEmpty(currentOption.Value) ? string.Empty : " ";
                currentOption.Value = string.Format("{0}{1}{2}", currentOption.Value, spacer, arg);
            }

            var parsed = new Dictionary<TokenType, string>();
            foreach (var option in list)
            {
                parsed[option.Token] = option.Value;
            }
            return parsed;
        }
    }
}