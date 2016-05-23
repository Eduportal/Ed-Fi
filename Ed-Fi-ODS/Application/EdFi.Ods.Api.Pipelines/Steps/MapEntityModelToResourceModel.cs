using System;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines.Common;

namespace EdFi.Ods.Pipelines.Steps
{
    public class MapEntityModelToResourceModel<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TContext : IHasPersistentModel<TEntityModel>
        where TResult : PipelineResultBase, IHasResource<TResourceModel>
        where TResourceModel : IHasETag, new()
        where TEntityModel : class, IMappable
    {
        public void Execute(TContext context, TResult result)
        {
            try
            {
                var resource = new TResourceModel();

                context.PersistentModel.Map(resource);

                result.Resource = resource;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
        }
    }
}