using EdFi.Ods.Api.Common;

namespace EdFi.Ods.Api.Data.Repositories
{
    public class TextCriteria : QueryCriteriaBase
    {
        public string Value { get; set; }
        public TextMatchMode MatchMode { get; set; }
    }
}