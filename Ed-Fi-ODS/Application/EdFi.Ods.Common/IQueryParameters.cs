using System.Collections.Generic;

namespace EdFi.Ods.Common
{
    public interface IQueryParameters
    {
        int? Offset { get; set; }
        int? Limit { get; set; }
        string Q { get; set; }
        List<IQueryCriteriaBase> QueryCriteria { get; set; }
    }
}