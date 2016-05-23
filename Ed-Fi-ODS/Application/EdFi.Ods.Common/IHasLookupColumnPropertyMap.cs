using System.Collections.Generic;

namespace EdFi.Ods.Common
{
    /// <summary>
    /// Provides a mapping from the lookup column property name without the Id suffix (which is not persisted to the database) to the corresponding persisted property (with the 'Id' suffix).
    /// </summary>
    public interface IHasLookupColumnPropertyMap
    {
        Dictionary<string, LookupColumnDetails> IdPropertyByLookupProperty { get; }
    }

    /// <summary>
    /// Provides the lookup column's entity type, and the property name for a lookup column.
    /// </summary>
    public class LookupColumnDetails
    {
        /// <summary>
        /// Gets or sets the name of the Id property.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the entity Type name of the lookup type with which the property is associated.
        /// </summary>
        public string LookupTypeName { get; set; }
    }
}