namespace EdFi.Ods.Api.Common
{
    /// <summary>
    /// Defines properties that indicate the result of an operation on a persistent resource.
    /// </summary>
    public interface IHasResourceChangeDetails
    {
        /// <summary>
        /// Indicates whether the operation resulted in the creation of a new resource.
        /// </summary>
        bool ResourceWasCreated { get; set; }

        /// <summary>
        /// Indicates whether the operation resulted in the update of an existing resource.
        /// </summary>
        bool ResourceWasUpdated { get; set; }

        /// <summary>
        /// Indicates whether the operation resulted in the deletion of an existing resource.
        /// </summary>
        bool ResourceWasDeleted { get; set; }
    }
}