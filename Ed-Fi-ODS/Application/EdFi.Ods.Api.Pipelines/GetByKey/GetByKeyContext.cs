using EdFi.Ods.Entities.Common;

namespace EdFi.Ods.Pipelines.GetByKey
{
    public class GetByKeyContext<TResourceModel, TEntityModel> : IHasPersistentModel<TEntityModel>, IHasResource<TResourceModel>
        where TEntityModel : class
        where TResourceModel : IHasETag
    {
        public GetByKeyContext(TResourceModel resourceSpecification, string etag)
        {
            Resource = resourceSpecification;
            ETag = etag;
        }

        /// <summary>
        ///     Gets or sets the resource model to be used as a specification for the query.
        /// </summary>
        public TResourceModel Resource { get; set; }

        /// <summary>
        ///     Gets or sets the persistent model to be used as a specification for the query.
        /// </summary>
        public TEntityModel PersistentModel { get; set; }

        public string ETag { get; set; }
    }
}