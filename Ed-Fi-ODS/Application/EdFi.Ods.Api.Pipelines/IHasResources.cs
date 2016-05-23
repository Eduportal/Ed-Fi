using System.Collections.Generic;
using EdFi.Ods.Entities.Common;

namespace EdFi.Ods.Pipelines
{
    public interface IHasResources<TResourceModel>
        where TResourceModel : IHasETag
    {
        IList<TResourceModel> Resources { get; set; }
    }
}