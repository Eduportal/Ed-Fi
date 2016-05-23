using System.Collections.Specialized;

namespace EdFi.Ods.Common
{
    /// <summary>
    /// Defines an interface for obtaining primary key values in the form of an 
    /// <see cref="OrderedDictionary"/>.
    /// </summary>
    public interface IHasPrimaryKeyValues
    {
        /// <summary>
        /// Gets the primary key values in the form of an <see cref="OrderedDictionary"/>.
        /// </summary>
        /// <returns>The dictionary containing the name/value pairs of the primary key fields.</returns>
        OrderedDictionary GetPrimaryKeyValues();
    }
}