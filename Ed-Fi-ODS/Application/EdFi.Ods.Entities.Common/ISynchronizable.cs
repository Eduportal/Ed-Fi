namespace EdFi.Ods.Entities.Common
{
    /// <summary>
    /// Defines a method for synchronizing changes from one entity to another, preserving domain identity
    /// (e.g. primary key values) and adding and removing contents of member lists to synchronize the two
    /// instances.
    /// </summary>
    public interface ISynchronizable
    {
        /// <summary>
        /// Synchronizes the current object to the specified target object, preserving domain identity
        /// and adding/removing child list items appropriately.
        /// </summary>
        /// <param name="target">A pre-existing instance (of the same abstract entity type) to be synchronized from the current object.</param>
        /// <returns><b>true</b> if any changes were made; otherwise <b>false</b>.</returns>
        bool Synchronize(object target);
    }
}
