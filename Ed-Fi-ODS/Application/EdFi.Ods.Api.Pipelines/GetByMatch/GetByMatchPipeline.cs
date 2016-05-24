using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines;
using EdFi.Ods.Pipelines.Common;

namespace EdFi.Ods.Api.Pipelines.GetByMatch
{
    public class GetByMatchPipeline<TResourceModel, TEntityModel> : PipelineBase<GetByMatchContext<TResourceModel, TEntityModel>, GetByMatchResult<TResourceModel>>
        where TResourceModel : IHasETag
        where TEntityModel : class
    {
        public GetByMatchPipeline(IStep<GetByMatchContext<TResourceModel, TEntityModel>, GetByMatchResult<TResourceModel>>[] steps) : base(steps) { }
    }
}
