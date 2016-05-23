using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Exceptions;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines.Common;
using EdFi.Ods.Pipelines.GetByKey;

namespace EdFi.Ods.Pipelines.Steps
{
    public class GetEntityModelByKey<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TContext : GetByKeyContext<TResourceModel, TEntityModel>
        where TResult : PipelineResultBase
        where TResourceModel : IHasETag
        where TEntityModel : class, IHasIdentifier, IDateVersionedEntity
    {
        private readonly IGetEntityByKey<TEntityModel> repository;

        public GetEntityModelByKey(IGetEntityByKey<TEntityModel> repository)
        {
            this.repository = repository;
        }

        public void Execute(TContext context, TResult result)
        {
            try
            {
                // Load the entities
                var model = repository.GetByKey(context.PersistentModel);

                if (model == null)
                {
                    string message = string.Format(
                        "Entity of type '{0}' with the specified key was not found.",
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
