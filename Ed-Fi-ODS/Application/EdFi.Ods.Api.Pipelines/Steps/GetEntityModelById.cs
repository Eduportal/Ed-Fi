using System;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Exceptions;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines.Common;

namespace EdFi.Ods.Pipelines.Steps
{
    public class GetEntityModelById<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TContext : IHasIdentifier, IHasPersistentModel<TEntityModel>
        where TResult : PipelineResultBase
        where TResourceModel : IHasETag
        where TEntityModel : class, IHasIdentifier, IDateVersionedEntity
    {
        private readonly IGetEntityById<TEntityModel> repository;

        public GetEntityModelById(IGetEntityById<TEntityModel> repository)
        {
            this.repository = repository;
        }

        public void Execute(TContext context, TResult result)
        {
            try
            {
                // Load the entities
                var model = repository.GetById(context.Id);

                if (model == null)
                {
                    string message = string.Format(
                        "Entity of type '{0}' with the specified id was not found.",
                        typeof(TEntityModel).Name);

                    // Capture exception information, but don't throw it for performance reasons
                    result.Exception = new NotFoundException(message, typeof(TEntityModel).Name, null);
                    
                    return;
                }

                context.PersistentModel = model;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
        }
    }
}
