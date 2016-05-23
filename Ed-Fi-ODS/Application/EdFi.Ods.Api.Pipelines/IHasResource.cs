using EdFi.Ods.Entities.Common;

namespace EdFi.Ods.Pipelines
{
    public interface IHasResource<TResourceModel>
        where TResourceModel : IHasETag
    {
        TResourceModel Resource { get; set; }
    }
}