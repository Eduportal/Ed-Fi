namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers
{
    using global::EdFi.Ods.BulkLoad.Core.Controllers.Aggregates;
    using global::EdFi.Ods.Common;
    using global::EdFi.Ods.Entities.Common;
    using global::EdFi.Ods.Pipelines.Common;
    using global::EdFi.Ods.Pipelines.Put;
    using global::EdFi.Ods.XmlShredding;

    public class TestAggregateLoader<T, TEntity> : AggregateLoaderBase<T, TEntity> where T : IHasETag where TEntity : class, IHasIdentifier
    {
        public TestAggregateLoader(IResourceFactory<T> factory, IStep<PutContext<T, TEntity>, PutResult> step)
            : base(factory, new CreateOrUpdatePipelineStub<T, TEntity>(step))
        {
        }
    }
}