// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Linq;
using EdFi.Ods.Utilities.LoadGeneration.Persistence;
using EdFi.Ods.Utilities.LoadGeneration.ValueBuilders;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public class RandomStaffUniqueIdSelector : IRandomStudentUniqueIdSelector
    {
        private readonly IEducationOrganizationIdentifiersProvider _educationOrganizationIdentifiersProvider;
        private readonly IPersonEducationOrganizationCache _personEducationOrganizationCache;

        public RandomStaffUniqueIdSelector(IPersonEducationOrganizationCache personEducationOrganizationCache,
            IEducationOrganizationIdentifiersProvider educationOrganizationIdentifiersProvider)
        {
            _educationOrganizationIdentifiersProvider = educationOrganizationIdentifiersProvider;
            _personEducationOrganizationCache = personEducationOrganizationCache;
        }

        public bool TryGetRandomStudentUniqueId(int educationOrganizationId, out string studentUniqueId)
        {
            // Initialize out parameters
            studentUniqueId = null;

            var edOrgIds = _educationOrganizationIdentifiersProvider.GetEducationOrganizationIdentifiers();

            // Find the ed org provided and resolve to an LEA
            var locatedEdOrgIdentifier =
                (from x in edOrgIds
                    where (x.EducationOrganizationId == educationOrganizationId)
                    select x)
                    .SingleOrDefault();

            if (locatedEdOrgIdentifier == null)
            {
                throw new Exception(string.Format(
                    "Unable to locate Education Organization with the id of '{0}'.",
                    educationOrganizationId));
            }

            if (locatedEdOrgIdentifier.LocalEducationAgencyId == null)
            {
                // This condition would happen if we open up the EdOrg possibilities beyond LEAs and Schools for load generation.
                // (i.e. there would be more work to do here if it were possible that we'd received an ESC here)
                throw new Exception(string.Format(
                    "Education Organization id '{0}' does not have an associated Local Education Agency.",
                    educationOrganizationId));
            }

            // Get the students for the selected school
            var staffUniqueIds = _personEducationOrganizationCache.GetPeople("Staff", locatedEdOrgIdentifier.LocalEducationAgencyId.Value);

            if (!staffUniqueIds.Any())
                return false;

            // Radomly select one of the staff
            studentUniqueId = staffUniqueIds.GetRandomMember();
            return true;
        }
    }
}