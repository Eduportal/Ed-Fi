using System;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines.Common;
using EdFi.Ods.Pipelines.GetMany;

namespace EdFi.Ods.Pipelines.Steps
{
    public class GetEntityModelsBySpecification<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TContext : GetManyContext<TResourceModel, TEntityModel>
        where TResult : PipelineResultBase
        where TResourceModel : IHasETag
        where TEntityModel : class, IHasIdentifier, IDateVersionedEntity
    {
        private readonly IGetEntitiesBySpecification<TEntityModel> repository;

        public GetEntityModelsBySpecification(IGetEntitiesBySpecification<TEntityModel> repository)
        {
            this.repository = repository;
        }

        public void Execute(TContext context, TResult result)
        {
            try
            {
                // Load the entities
                var models = repository.GetBySpecification(context.PersistentModel, context.QueryParameters);
                context.PersistentModels = models;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
        }
    }
}
