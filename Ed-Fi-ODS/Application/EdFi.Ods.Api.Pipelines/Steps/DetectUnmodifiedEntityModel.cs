using System;
using EdFi.Ods.Common.Exceptions;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines.Common;
using EdFi.Ods.Api.Common;

namespace EdFi.Ods.Pipelines.Steps
{
    public class DetectUnmodifiedEntityModel<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult> 
        where TContext : IHasPersistentModel<TEntityModel>, IHasETag
        where TResult : PipelineResultBase 
        where TEntityModel : class
    {
        private readonly IETagProvider etagProvider;

        public DetectUnmodifiedEntityModel(IETagProvider etagProvider)
        {
            this.etagProvider = etagProvider;
        }

        public void Execute(TContext context, TResult result)
        {
            try
            {
                // Don't process model if ETag isn't present
                if (context.ETag == null)
                    return;

                // Check the ETag for no modifications
                if (context.ETag == etagProvider.GetETag(context.PersistentModel))
                {
                    // TODO: Consider using System.Net.HttpResult for these results instead of exceptions
                    throw new NotModifiedException();
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
        }
    }
}
