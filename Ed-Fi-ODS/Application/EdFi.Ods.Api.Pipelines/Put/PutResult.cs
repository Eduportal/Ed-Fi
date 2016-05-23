using System;
using EdFi.Ods.Pipelines.Common;
using EdFi.Ods.Api.Common;

namespace EdFi.Ods.Pipelines.Put
{
    public class PutResult : PipelineResultBase, IHasResourceChangeDetails
    {
        /// <summary>
        /// Indicates whether the operation resulted in the creation of a new resource.
        /// </summary>
        public bool ResourceWasCreated { get; set; }

        /// <summary>
        /// Indicates whether the operation resulted in the update of an existing resource.
        /// </summary>
        public bool ResourceWasUpdated { get; set; }

        /// <summary>
        /// True if the resource was successfully handled by the PersistModel step.  This includes resources that are unchanged 
        /// because the value in the database already matches the value being persisted.
        /// </summary>
        public bool ResourceWasPersisted { get; set; }

        /// <summary>
        /// Indicates whether the operation resulted in the deletion of an existing resource.
        /// </summary>
        public bool ResourceWasDeleted { get; set; }

        /// <summary>
        /// Gets or sets the new ETag value for the resource.
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the resource that was created/updated.
        /// </summary>
        public Guid? ResourceId { get; set; }
    }
}