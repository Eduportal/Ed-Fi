using EdFi.Ods.BulkLoad.Common;

namespace EdFi.Ods.BulkLoad.Core.Controllers.Aggregates
{
    public interface IHaveInterchangeSpecificLoaderCollections
    {
        OrderedCollectionOfSets<ILoadAggregates> GetCollectionFor(string interchangeName);
    }
}