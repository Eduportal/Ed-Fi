using System.Collections.Generic;
using EdFi.Ods.Common;
using EdFi.Ods.Entities.Common;

namespace EdFi.Ods.Pipelines.GetMany
{
    public class GetManyContext<TResourceModel, TEntityModel> : IHasPersistentModel<TEntityModel>, IHasPersistentModels<TEntityModel>, IHasResource<TResourceModel> // IHasETag, IHasIdentifier 
        where TResourceModel : IHasETag
        where TEntityModel : class
    {
        public GetManyContext(TResourceModel resourceSpecification, IQueryParameters queryParameters)
        {
            Resource = resourceSpecification;
            QueryParameters = queryParameters;
        }

        /// <summary>
        ///     Gets or sets the resource model to be used as a specification for the query.
        /// </summary>
        public TResourceModel Resource { get; set; }

        /// <summary>
        ///     Gets or sets additional query parameters to apply to the search/filter.
        /// </summary>
        public IQueryParameters QueryParameters { get; set; }

        /// <summary>
        ///     Gets or sets the persistent model to be used as a specification for the query.
        /// </summary>
        public TEntityModel PersistentModel { get; set; }

        /// <summary>
        ///     Gets or sets the list of persistent models retrieved from storage.
        /// </summary>
        public IList<TEntityModel> PersistentModels { get; set; }
    }
}