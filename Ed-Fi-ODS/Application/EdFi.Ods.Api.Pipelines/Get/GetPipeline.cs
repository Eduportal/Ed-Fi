using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines.Common;

namespace EdFi.Ods.Pipelines.Get
{
    public class GetPipeline<TResourceModel, TEntityModel> : PipelineBase<GetContext<TEntityModel>, GetResult<TResourceModel>>
        where TResourceModel : IHasETag
        where TEntityModel : class
    {
        public GetPipeline(IStep<GetContext<TEntityModel>, GetResult<TResourceModel>>[] steps)
            : base(steps)
        {
        }
    }
}