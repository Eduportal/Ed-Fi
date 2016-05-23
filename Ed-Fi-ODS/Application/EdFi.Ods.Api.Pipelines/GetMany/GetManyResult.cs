using System.Collections.Generic;
using EdFi.Ods.Entities.Common;

namespace EdFi.Ods.Pipelines.GetMany
{
    public class GetManyResult<TResourceModel> : PipelineResultBase, IHasResources<TResourceModel>
        where TResourceModel : IHasETag
    {
        public IList<TResourceModel> Resources { get; set; }
    }
}
