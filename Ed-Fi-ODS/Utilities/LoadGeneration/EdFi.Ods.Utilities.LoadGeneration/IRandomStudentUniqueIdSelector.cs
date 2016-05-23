namespace EdFi.Ods.Utilities.LoadGeneration
{
    public interface IRandomStudentUniqueIdSelector
    {
        /// <summary>
        /// Attempts to retrieve an existing UniqueId for a student associated with a school at the specified <paramref name="educationOrganizationId"/>.
        /// </summary>
        /// <param name="educationOrganizationId">The school or local education agency to which the student must be associated in order to be selected.</param>
        /// <param name="studentUniqueId">An outbound student unique Id associated with the specified organization.</param>
        /// <returns><b>true</b> if a matching unique id is found; otherwise <b>false</b>.</returns>
        bool TryGetRandomStudentUniqueId(int educationOrganizationId, out string studentUniqueId);
    }
}