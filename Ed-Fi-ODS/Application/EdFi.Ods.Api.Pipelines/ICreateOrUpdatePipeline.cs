using EdFi.Ods.Common;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines.Put;

namespace EdFi.Ods.Pipelines
{
    public interface ICreateOrUpdatePipeline<TResourceModel, TEntityModel>
        where TResourceModel : IHasETag
        where TEntityModel : class, IHasIdentifier
    {
        PutResult Process(PutContext<TResourceModel, TEntityModel> context);
    }

}
