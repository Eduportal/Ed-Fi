using System;
using System.Threading.Tasks;

namespace EdFi.Ods.BulkLoad.Core
{
    public interface ILoadAggregates
    {
        Task<LoadAggregateResult> LoadFrom(IIndexedXmlFileReader reader);
        Type GetAggregateType();
    }
}