using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines.Common;

namespace EdFi.Ods.Pipelines.GetByKey
{
    public class GetByKeyPipeline<TResourceModel, TEntityModel> : PipelineBase<GetByKeyContext<TResourceModel, TEntityModel>, GetByKeyResult<TResourceModel>>
        where TResourceModel : IHasETag
        where TEntityModel : class
    {
        public GetByKeyPipeline(IStep<GetByKeyContext<TResourceModel, TEntityModel>, GetByKeyResult<TResourceModel>>[] steps)
            : base(steps)
        {
        }
    }
}
