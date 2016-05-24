using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines;

namespace EdFi.Ods.Api.Pipelines.GetByMatch
{
    public class GetByMatchResult<TResourceModel> : PipelineResultBase, IHasResources<TResourceModel>
        where TResourceModel : IHasETag
    {
        public IList<TResourceModel> Resources { get; set; }
    }
}
