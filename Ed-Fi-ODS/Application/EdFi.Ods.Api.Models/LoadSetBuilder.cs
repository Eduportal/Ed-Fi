namespace EdFi.Ods.BulkLoad.Common
{
    public class LoadSetBuilder
    {
        private OrderedCollectionOfSets<string>.SetBuilder Builder;

        public LoadSetBuilder(OrderedCollectionOfSets<string>.SetBuilder builder)
        {
            Builder = builder;
        }

        //Adds a single aggregate type name to the current set
        public LoadSetBuilder Aggregate<TAggregate>()
        {
            Builder.AddMember(typeof (TAggregate).Name);
            return this;
        }

    }
}