using System.Collections.Generic;
using System.Text.RegularExpressions;
using EdFi.Ods.Common;
using EdFi.Ods.Api.Common;

namespace EdFi.Ods.Api.Data.Repositories
{
    public class QueryParameters : IQueryParameters
    {
        public QueryParameters(UrlQueryParametersRequest parameters)
        {
            QueryCriteria = new List<IQueryCriteriaBase>();

            this.Offset = parameters.Offset;
            this.Limit = parameters.Limit;
            this.Q = parameters.Q;
        }

        public QueryParameters()
        {
            QueryCriteria = new List<IQueryCriteriaBase>();
        }

        private static QueryParameters _empty = new QueryParameters();
        
        /// <summary>
        /// Gets an empty instance of the <see cref="QueryParameters"/> class.
        /// </summary>
        public static QueryParameters Empty { get { return _empty; } }

        public int? Offset { get; set; }
        public int? Limit { get; set; }
        // TODO: public string[] Fields { get; set; }

        private string _q;

        public string Q
        {
            get { return _q; }
            set
            {
                _q = value;

                if (_q == null)
                    return;

                var queryCriteria = new List<IQueryCriteriaBase>();

                var regex = new Regex(@"(?i)(?<PropertyName>[a-z]+)\:((?<NumericRange>(\[|\{)[\d]+\sto\s[\d]+(\]|\}))|(?<DateRange>\[\d{4}-\d{1,2}\-\d{1,2}\sto\s[\d\-]+\])|'(?<QuotedText>\*?[a-z\s]+\*?)'|(?<Text>\*?[a-z]+\*?))");

                var matches = regex.Matches(_q);

                foreach (Match match in matches)
                {
                    string propertyName = match.Groups["PropertyName"].Value;

                    // Supporting only non-quoted text searches for now
                    if (match.Groups["Text"].Success)
                    {
                        string text = match.Groups["Text"].Value;

                        bool leadingAsterisk = text[0] == '*';
                        bool trailingAsterisk = text[text.Length - 1] == '*';

                        string rawText = text.Trim('*');

                        var textCriteria = new TextCriteria { PropertyName = propertyName, Value = rawText };

                        if (leadingAsterisk && trailingAsterisk)
                            textCriteria.MatchMode = TextMatchMode.Anywhere;
                        else if (leadingAsterisk)
                            textCriteria.MatchMode = TextMatchMode.End;
                        else if (trailingAsterisk)
                            textCriteria.MatchMode = TextMatchMode.Start;
                        else
                            textCriteria.MatchMode = TextMatchMode.Exact;

                        queryCriteria.Add(textCriteria);
                    }
                }

                QueryCriteria = queryCriteria;
            }
        }

        public List<IQueryCriteriaBase> QueryCriteria { get; set; }
    }
}