using System;
using System.Linq;
using EdFi.Ods.Utilities.LoadGeneration.Persistence;
using EdFi.Ods.Utilities.LoadGeneration.ValueBuilders;
using log4net;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    // TODO: Needs unit tests
    public class RandomStudentUniqueIdSelector : IRandomStudentUniqueIdSelector
    {
        private ILog _logger = LogManager.GetLogger(typeof(RandomStudentUniqueIdSelector));

        private readonly IEducationOrganizationIdentifiersProvider _educationOrganizationIdentifiersProvider;
        private readonly IPersonEducationOrganizationCache _personEducationOrganizationCache;

        public RandomStudentUniqueIdSelector(IPersonEducationOrganizationCache personEducationOrganizationCache,
            IEducationOrganizationIdentifiersProvider educationOrganizationIdentifiersProvider)
        {
            _educationOrganizationIdentifiersProvider = educationOrganizationIdentifiersProvider;
            _personEducationOrganizationCache = personEducationOrganizationCache;
        }

        public bool TryGetRandomStudentUniqueId(int educationOrganizationId, out string studentUniqueId)
        {
            _logger.DebugFormat("Attempting to find a StudentUniqueId associated with education organization '{0}'.", educationOrganizationId);

            // Initialize out parameters
            studentUniqueId = null;

            var edOrgIds = _educationOrganizationIdentifiersProvider.GetEducationOrganizationIdentifiers();

            var locatedEdOrgIdentifiers =
                (from x in edOrgIds
                    where (x.EducationOrganizationType == "LocalEducationAgency" 
                                && x.LocalEducationAgencyId == educationOrganizationId)
                          || x.SchoolId == educationOrganizationId
                    select x)
                    .ToList();

            if (locatedEdOrgIdentifiers.Count == 0)
            {
                throw new Exception(string.Format(
                    "Unable to locate any Local Education Agencies or Schools with the id of '{0}'.", 
                    educationOrganizationId));
            }

            int schoolId;

            if (locatedEdOrgIdentifiers.Count == 1)
            {
                schoolId = locatedEdOrgIdentifiers.Single().SchoolId.Value;
            }
            else
            {
                // Pick a school, randomly
                schoolId = locatedEdOrgIdentifiers.GetRandomMember().SchoolId.Value;
            }

            // Get the students for the selected school
            var studentUniqueIds = _personEducationOrganizationCache.GetPeople("Student", schoolId);

            if (!studentUniqueIds.Any())
            {
                _logger.DebugFormat("Unable to find any students at school id '{0}'.", schoolId);
                return false;
            }

            _logger.DebugFormat("{0} students were found for the selected school '{0}'.", schoolId);

            // Radomly select one of the students
            studentUniqueId = studentUniqueIds.GetRandomMember();
            return true;
        }
    }
}