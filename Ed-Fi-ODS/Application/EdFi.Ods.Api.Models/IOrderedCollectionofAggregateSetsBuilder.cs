using System.Collections.Generic;

namespace EdFi.Ods.BulkLoad.Common
{
    public interface IOrderedCollectionofAggregateSetsBuilder
    {
        OrderedCollectionOfSets<string> BuildOrderedCollectionOfSetsFrom(IEnumerable<string> aggregateTypeNames);
    }
}