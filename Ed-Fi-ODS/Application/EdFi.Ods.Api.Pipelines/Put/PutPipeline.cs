using EdFi.Ods.Common;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines.Common;

namespace EdFi.Ods.Pipelines.Put
{
    public class PutPipeline<TResourceModel, TEntityModel> : PipelineBase<PutContext<TResourceModel, TEntityModel>, PutResult>
        where TResourceModel : IHasETag
        where TEntityModel : class, IHasIdentifier
    {
        public PutPipeline(IStep<PutContext<TResourceModel, TEntityModel>, PutResult>[] steps)
            : base(steps)
        {
        }
    }
}