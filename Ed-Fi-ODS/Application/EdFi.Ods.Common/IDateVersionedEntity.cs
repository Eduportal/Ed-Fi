using System;

namespace EdFi.Ods.Common
{
    public interface IDateVersionedEntity
    {
        //int Version { get; set; }
        DateTime LastModifiedDate { get; set; }
    }
}
