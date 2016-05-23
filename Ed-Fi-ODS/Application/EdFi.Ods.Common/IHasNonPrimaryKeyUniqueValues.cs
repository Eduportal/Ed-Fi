using System.Collections.Specialized;

namespace EdFi.Ods.Common
{
    /// <summary>
    /// Defines an interface for obtaining primary key values in the form of an 
    /// <see cref="OrderedDictionary"/>.
    /// </summary>
    public interface IHasNonPrimaryKeyUniqueValues
    {
        /// <summary>
        /// Gets the unique key values which are not part of the primary key, in the form of an <see cref="OrderedDictionary"/>.
        /// </summary>
        /// <returns>The dictionary containing the name/value pairs of the non primary unique key fields.</returns>
        OrderedDictionary GetNonPrimaryUniqueKeyValues();
    }
}