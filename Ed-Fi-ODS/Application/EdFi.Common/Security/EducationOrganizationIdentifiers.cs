// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

namespace EdFi.Common.Security
{
    public class EducationOrganizationIdentifiers
    {
        private EducationOrganizationIdentifiers(int educationOrganizationId)
        {
            EducationOrganizationId = educationOrganizationId;
        }

        public EducationOrganizationIdentifiers(int educationOrganizationId, string educationOrganizationType, int? stateEducationAgencyId, int? educationServiceCenterId, int? localEducationAgencyId, int? schoolId)
        {
            EducationOrganizationId = educationOrganizationId;
            EducationOrganizationType = educationOrganizationType;
            StateEducationAgencyId = stateEducationAgencyId;
            EducationServiceCenterId = educationServiceCenterId;
            LocalEducationAgencyId = localEducationAgencyId;
            SchoolId = schoolId;
        }

        public int EducationOrganizationId { get; private set; }
        public string EducationOrganizationType { get; private set; }
        public int? StateEducationAgencyId { get; private set; }
        public int? EducationServiceCenterId { get; private set; }
        public int? LocalEducationAgencyId { get; private set; }
        public int? SchoolId { get; private set; }

        public static EducationOrganizationIdentifiers CreateLookupInstance(int educationOrganizationId)
        {
            return new EducationOrganizationIdentifiers(educationOrganizationId);
        }
    }
}