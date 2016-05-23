using System.Collections.Generic;
using System.Linq;

namespace EdFi.Ods.BulkLoad.Common
{
    public abstract class OrderCollectionOfAggregateTypeNameSetsBuilder : IOrderedCollectionofAggregateSetsBuilder
    {
        private readonly OrderedCollectionOfSets<string> OrderCollectionOfAggregateTypeNameSets = new OrderedCollectionOfSets<string>(); 

        public OrderedCollectionOfSets<string> BuildOrderedCollectionOfSetsFrom(IEnumerable<string> aggregateTypeNames)
        {
            if (OrderCollectionOfAggregateTypeNameSets.Any()) return OrderCollectionOfAggregateTypeNameSets;
            SpecifyAnyRequiredLoadOrder();
            var remainingAggregates =
                aggregateTypeNames.Except(OrderCollectionOfAggregateTypeNameSets.GetAllContainedMembers());
            if (remainingAggregates.Any())
            {
                OrderCollectionOfAggregateTypeNameSets
                    .AddSet()
                    .AddMemberRange(remainingAggregates);
            }
            return OrderCollectionOfAggregateTypeNameSets;
        }

        //Creates a new set for the collection
        protected LoadSetBuilder LoadSet
        {
            get { return new LoadSetBuilder(OrderCollectionOfAggregateTypeNameSets.AddSet()); }
        }

        protected abstract void SpecifyAnyRequiredLoadOrder();
    }
}