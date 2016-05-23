using System.Collections.Generic;

namespace EdFi.Ods.Utilities.LoadGeneration.Persistence
{
    /// <summary>
    /// Defines a method for persisting a resource.
    /// </summary>
    public interface IResourcePersister
    {
        /// <summary>
        /// Persists a resource using the suppied context, and returns an outbound resource reference.
        /// </summary>
        /// <param name="resource">The resource to be saved.</param>
        /// <param name="context">Context to be used to perform any necessary additional steps related to persisting the resource (such as education organization context), or <b>null</b>.</param>
        /// <param name="resourceReference">An instance of a the corresponding resource's reference.</param>
        void PersistResource(object resource, IDictionary<string, object> context, out object resourceReference);
    }
}