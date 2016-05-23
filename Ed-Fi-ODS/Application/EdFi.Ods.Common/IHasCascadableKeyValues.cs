using System.Collections.Specialized;

namespace EdFi.Ods.Common
{
    /// <summary>
    /// Defines a method for getting and setting new primary key values (for performing 
    /// cascading updates) using an <see cref="OrderedDictionary"/>.
    /// </summary>
    public interface IHasCascadableKeyValues
    {
        /// <summary>
        /// Gets or sets the <see cref="OrderedDictionary"/> capturing the new key values that have
        /// not been modified directly on the entity.
        /// </summary>
        OrderedDictionary NewKeyValues { get; set; }
    }
}