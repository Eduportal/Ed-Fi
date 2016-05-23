using System;
using EdFi.Ods.Common;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Api.Common;

namespace EdFi.Ods.Pipelines.Put
{
    public class PutContext<TResourceModel, TEntityModel> : IHasPersistentModel<TEntityModel>, IHasResource<TResourceModel>, IHasIdentifier
        where TResourceModel : IHasETag
        where TEntityModel : class, IHasIdentifier
    {
        private readonly ValidationState validationState;

        public PutContext(TResourceModel resource, ValidationState validationState)
            : this(resource, validationState, null)
        {
        }

        public PutContext(TResourceModel resource, ValidationState validationState, string etagValue)
        {
            this.validationState = validationState;
            Resource = resource;
            ETagValue = etagValue;
        }

        public string ETagValue { get; set; }
        public TResourceModel Resource { get; set; }
        public TEntityModel PersistentModel { get; set; }

        public bool? IsValid
        {
            get { return validationState.IsValid; }
            set { validationState.IsValid = value; }
        }

        public bool EnforceOptimisticLock
        {
            get { return Resource.ETag != null; }
        }

        public Guid Id
        {
            get { return PersistentModel.Id; }
            set { throw new NotImplementedException("Cannot set the identifier of the persistent model through the context."); }
        }
    }
}