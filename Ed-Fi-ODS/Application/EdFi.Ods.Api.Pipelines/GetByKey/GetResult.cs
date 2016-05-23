using EdFi.Ods.Entities.Common;

namespace EdFi.Ods.Pipelines.GetByKey
{
    public class GetByKeyResult<TResourceModel> : PipelineResultBase, IHasResource<TResourceModel> 
        where TResourceModel : IHasETag
    {
        public TResourceModel Resource { get; set; }
    }
}
