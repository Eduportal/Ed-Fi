using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdFi.Ods.Api.Pipelines.GetByMatch;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Exceptions;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines;
using EdFi.Ods.Pipelines.Common;

namespace EdFi.Ods.Api.Pipelines.Steps
{
    public class GetEntityModelByMatch<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TContext : GetByMatchContext<TResourceModel, TEntityModel>
        where TResult : PipelineResultBase
        where TResourceModel : IHasETag
        where TEntityModel : class, IHasIdentifier, IDateVersionedEntity
    {
        private readonly IGetEntityByKey<TEntityModel> repository;

        public GetEntityModelByMatch(IGetEntityByKey<TEntityModel> repository)
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
