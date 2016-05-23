using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines.Common;

namespace EdFi.Ods.Pipelines.GetMany
{
    public class GetManyPipeline<TResourceModel, TEntityModel> : PipelineBase<GetManyContext<TResourceModel, TEntityModel>, GetManyResult<TResourceModel>>
        where TResourceModel : IHasETag
        where TEntityModel : class
    {
        public GetManyPipeline(IStep<GetManyContext<TResourceModel, TEntityModel>, GetManyResult<TResourceModel>>[] steps)
            : base(steps)
        {
        }
    }
}
