using System;
using System.Runtime.Serialization;
using EdFi.Ods.Common;

namespace EdFi.Ods.Entities.NHibernate.Architecture
{
    public abstract class AggregateRootWithCompositeKey : EntityWithCompositeKey, 
        IHasIdentifier, IDateVersionedEntity
    {
        // =============================================================
        //                          AggregateId
        // -------------------------------------------------------------

        [DataMember]
        public virtual Guid Id { get; set; }
        // -------------------------------------------------------------


        // =============================================================
        //                         Versioning
        // -------------------------------------------------------------
        [IgnoreDataMember] // Populate from etag header processing, don't serialize out or in.
        public virtual DateTime LastModifiedDate { get; set; }
    }
}
