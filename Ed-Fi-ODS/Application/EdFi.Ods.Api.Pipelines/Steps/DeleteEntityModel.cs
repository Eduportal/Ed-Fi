using System;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Pipelines.Common;
using EdFi.Ods.Pipelines.Delete;

namespace EdFi.Ods.Pipelines.Steps
{
    public class DeleteEntityModel<TContext, TResult, TResourceModel, TEntityModel> : IStep<DeleteContext, DeleteResult>
        where TEntityModel : IHasIdentifier
    {
        private readonly IDeleteEntityById<TEntityModel> repository;

        public DeleteEntityModel(IDeleteEntityById<TEntityModel> repository)
        {
            this.repository = repository;
        }

        public void Execute(DeleteContext context, DeleteResult result)
        {
            try
            {
                repository.DeleteById(context.Id, context.ETag);
                result.ResourceWasDeleted = true;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
        }
    }
}
