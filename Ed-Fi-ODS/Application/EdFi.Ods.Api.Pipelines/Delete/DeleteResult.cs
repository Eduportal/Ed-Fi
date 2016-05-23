using EdFi.Ods.Pipelines.Common;
using EdFi.Ods.Api.Common;

namespace EdFi.Ods.Pipelines.Delete
{
    public class DeleteResult : PipelineResultBase, IHasResourceChangeDetails
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
        /// Indicates whether the operation resulted in the deletion of an existing resource.
        /// </summary>
        public bool ResourceWasDeleted { get; set; }
    }
}