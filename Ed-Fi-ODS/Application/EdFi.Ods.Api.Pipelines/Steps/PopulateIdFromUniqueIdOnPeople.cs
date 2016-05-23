using System;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Specifications;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Entities.Common.Caching;
using EdFi.Ods.Pipelines.Common;
using EdFi.Ods.Pipelines._Installers;

namespace EdFi.Ods.Pipelines.Steps
{
    [RegistrationContext("UniqueIdIntegration")] // This is used to exclude the step from the general container registration
    public class PopulateIdFromUniqueIdOnPeople<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TContext : IHasPersistentModel<TEntityModel>, IHasResource<TResourceModel>
        where TResourceModel : IHasETag
        where TEntityModel : class
        where TResult : PipelineResultBase
    {
        private readonly IPersonUniqueIdToIdCache _personUniqueIdToIdCache;

        public PopulateIdFromUniqueIdOnPeople(IPersonUniqueIdToIdCache personUniqueIdToIdCache)
        {
            _personUniqueIdToIdCache = personUniqueIdToIdCache;
        }

        public void Execute(TContext context, TResult result)
        {
            if (!PersonEntitySpecification.IsPersonEntity(typeof (TResourceModel))) return;

            var idResource = context.PersistentModel as IHasIdentifier;
            if (idResource == null || idResource.Id != default(Guid)) return;

            var uniqueId = ((IIdentifiablePerson) context.Resource).UniqueId;
            var id = _personUniqueIdToIdCache.GetId((typeof(TResourceModel).Name),uniqueId);

            idResource.Id = id;
        }
    }
}