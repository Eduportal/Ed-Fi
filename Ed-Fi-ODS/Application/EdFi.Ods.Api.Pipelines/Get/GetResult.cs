using EdFi.Ods.Entities.Common;

namespace EdFi.Ods.Pipelines.Get
{
    public class GetResult<TResourceModel> : PipelineResultBase, IHasResource<TResourceModel> 
        where TResourceModel : IHasETag
    {
        public TResourceModel Resource { get; set; }
    }
}
