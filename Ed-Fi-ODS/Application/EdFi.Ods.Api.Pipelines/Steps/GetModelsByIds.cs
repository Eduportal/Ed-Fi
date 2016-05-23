using System;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Exceptions;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines.Common;

namespace EdFi.Ods.Pipelines.Steps
{
    public class GetModelsByIds<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TContext : IHasPersistentModels<TEntityModel>, IHasIdentifiers<Guid>
        where TResult : PipelineResultBase
        where TResourceModel : IHasETag
        where TEntityModel : class, IHasIdentifier, IDateVersionedEntity
    {
        private readonly IGetEntitiesByIds<TEntityModel> repository;

        public GetModelsByIds(IGetEntitiesByIds<TEntityModel> repository)
        {
            this.repository = repository;
        }

        public void Execute(TContext context, TResult result)
        {
            try
            {
                // Load the entity
                var models = repository.GetByIds(context.Ids);

                // If it wasn't found, throw an exception
                if (context.Ids.Count == 1 && models.Count == 0)
                {
                    string message = string.Format(
                        "Entity of type '{0}' with id of '{1}' was not found.", 
                        typeof (TEntityModel).Name, context.Ids[0]);

                    // Capture exception information, but don't throw it for performance reasons
                    result.Exception = new NotFoundException(message, typeof(TEntityModel).Name, context.Ids[0].ToString());

                    return;
                }

                context.PersistentModels = models;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
        }
    }
}