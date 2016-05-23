using System;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines.Common;
using EdFi.Ods.Pipelines.Put;
using EdFi.Ods.Api.Common;

namespace EdFi.Ods.Pipelines.Steps
{
    public class PersistEntityModel<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TContext : PutContext<TResourceModel, TEntityModel> // TODO: Is there a PersistenceContext? Maybe only when supporting PUT creational semantics?
        where TResult : PutResult
        where TEntityModel : class, IHasIdentifier, IDateVersionedEntity
        where TResourceModel : IHasETag
    {
        private readonly IUpsertEntity<TEntityModel> upsertEntity;
        private readonly IETagProvider etagProvider;

        public PersistEntityModel(IUpsertEntity<TEntityModel> upsertEntity, IETagProvider etagProvider)
        {
            this.upsertEntity = upsertEntity;
            this.etagProvider = etagProvider;
        }

        public void Execute(TContext context, TResult result)
        {
            try
            {
                bool isCreated, isModified;

                var updatedEntity = upsertEntity.Upsert(context.PersistentModel, 
                    context.EnforceOptimisticLock, out isModified, out isCreated);

                context.PersistentModel = updatedEntity;

                // Set the resulting resource's identifier
                if (result.ResourceId == null)
                    result.ResourceId = updatedEntity.Id;

                result.ResourceWasCreated = isCreated;
                result.ResourceWasUpdated = isModified;
                result.ResourceWasPersisted = true;

                // Set the etag value
                result.ETag = etagProvider.GetETag(updatedEntity);
            }

            catch (Exception ex)
            {
                result.Exception = ex;
            }
        }
    }
}