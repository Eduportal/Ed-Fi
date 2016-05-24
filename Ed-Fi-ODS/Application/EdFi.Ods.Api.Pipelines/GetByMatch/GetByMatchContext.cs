using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines;

namespace EdFi.Ods.Api.Pipelines.GetByMatch
{
    public class GetByMatchContext<TResourceModel, TEntityModel> : IHasPersistentModel<TEntityModel>, IHasResource<TResourceModel>
        where TEntityModel : class
        where TResourceModel : IHasETag
    {
        public GetByMatchContext(TResourceModel resourceSpecification, string etag)
        {
            Resource = resourceSpecification;
            ETag = etag;
        }

        public TResourceModel Resource { get; set; }
        public TEntityModel PersistentModel { get; set; }
        public string ETag { get; set; }
    }
}
