namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core
{
    using global::EdFi.Ods.Common;
    using global::EdFi.Ods.Entities.Common;
    using global::EdFi.Ods.Pipelines;
    using global::EdFi.Ods.Pipelines.Common;
    using global::EdFi.Ods.Pipelines.Put;

    public class CreateOrUpdatePipelineStub<TResourceModel, TEntityModel> : ICreateOrUpdatePipeline<TResourceModel, TEntityModel>   where TResourceModel : IHasETag
                                                                                                              where TEntityModel : class, IHasIdentifier
    {
        private readonly IStep<PutContext<TResourceModel, TEntityModel>, PutResult> step;

        public CreateOrUpdatePipelineStub(IStep<PutContext<TResourceModel, TEntityModel>, PutResult> step)
        {
            this.step = step;
        }

        public PutResult Process(PutContext<TResourceModel, TEntityModel> context)
        {
            var pipeline = new PutPipeline<TResourceModel, TEntityModel>( new[] { this.step } );

            return pipeline.Process(context);
        }
    }
}