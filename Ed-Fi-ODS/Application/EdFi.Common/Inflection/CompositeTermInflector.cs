using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EdFi.Common.Inflection
{
    public static class CompositeTermInflector
    {
        private static readonly HashSet<string> IgnoredSuffixes = new HashSet<string>();

        static CompositeTermInflector()
        {
            AddIgnoredSuffix("Offered");
        }

        public static void AddIgnoredSuffix(string suffix)
        {
            IgnoredSuffixes.Add(suffix.ToLower());
        }

        private readonly static ConcurrentDictionary<string, string> PluralizedByTerm
            = new ConcurrentDictionary<string, string>();

        public static string MakePlural(string compositeTerm)
        {
            return PluralizedByTerm.GetOrAdd(
                compositeTerm,
                t =>
                {
                    List<string> result = new List<string>();

                    // Split the composite term based on mixed-case conventions
                    var matches = Regex.Matches(t, "((?:^[a-z]+|[A-Z]+)(?:[a-z0-9]+)?)");

                    bool isCompositeTermPluralized = false;

                    for (int i = matches.Count - 1; i >= 0; i--)
                    {
                        string term = matches[i].Value;

                        if (isCompositeTermPluralized || IgnoredSuffixes.Contains(term.ToLower()))
                        {
                            result.Insert(0, term);
                            continue;
                        }

                        // Pluralize the current term
                        string pluralizedTerm = Inflector.MakePlural(term);
                        result.Insert(0, pluralizedTerm);
                        isCompositeTermPluralized = true;
                    }

                    return string.Join(string.Empty, result);
                });
        }

        private readonly static ConcurrentDictionary<string, string> SingularizedByTerm
            = new ConcurrentDictionary<string, string>();

        public static string MakeSingular(string compositeTerm)
        {
            return SingularizedByTerm.GetOrAdd(
                compositeTerm,
                t =>
                {
                    List<string> result = new List<string>();

                    // Split the composite term based on mixed-case conventions
                    var matches = Regex.Matches(t, "((?:^[a-z]+|[A-Z]+)(?:[a-z0-9]+)?)");

                    bool isCompositeTermSingularized = false;

                    for (int i = matches.Count - 1; i >= 0; i--)
                    {
                        string term = matches[i].Value;

                        if (isCompositeTermSingularized || IgnoredSuffixes.Contains(term.ToLower()))
                        {
                            result.Insert(0, term);
                            continue;
                        }

                        // Pluralize the current term
                        string singularizedTerm = Inflector.MakeSingular(term);
                        result.Insert(0, singularizedTerm);
                        isCompositeTermSingularized = true;
                    }

                    return string.Join(string.Empty, result);
                });
        }
    }
}