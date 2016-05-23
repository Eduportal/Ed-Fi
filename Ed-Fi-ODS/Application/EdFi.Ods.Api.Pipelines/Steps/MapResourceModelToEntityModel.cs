using System;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines.Common;

namespace EdFi.Ods.Pipelines.Steps
{
    public class MapResourceModelToEntityModel<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TContext : IHasPersistentModel<TEntityModel>, IHasResource<TResourceModel>
        where TResult : PipelineResultBase
        where TResourceModel : class, IMappable, IHasETag
        where TEntityModel : class, new()
    {
        public void Execute(TContext context, TResult result)
        {
            try
            {
                if (context.Resource == default(TResourceModel)) return;

                var model = new TEntityModel();
                context.Resource.Map(model);
                context.PersistentModel = model;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
        }
    }
}