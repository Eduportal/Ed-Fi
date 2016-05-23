using EdFi.Ods.Common;

namespace EdFi.Ods.Api.Data.Repositories
{
    public abstract class QueryCriteriaBase : IQueryCriteriaBase
    {
        public string PropertyName { get; set; }
    }
}