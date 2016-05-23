using System;
using System.Collections.Generic;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines.Common;

namespace EdFi.Ods.Pipelines.Steps
{
    public class MapEntityModelsToResourceModels<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult> 
        where TContext : IHasPersistentModels<TEntityModel>
        where TResult : PipelineResultBase, IHasResources<TResourceModel>
        where TResourceModel : IHasETag //, new()
        where TEntityModel : class, IMappable
    {
        public void Execute(TContext context, TResult result)
        {
            try
            {
                // Map the persistent models to resources
                IList<TResourceModel> resources = new List<TResourceModel>();
                context.PersistentModels.MapListTo(resources);

                result.Resources = resources;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
        }
    }
}