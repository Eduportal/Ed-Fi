// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;

namespace EdFi.TestObjects._Extensions
{
    public static class Specifications
    {
        public static bool IsEducationOrganizationIdentifier(this string valueName)
        {
            return 
                (valueName.Equals("educationOrganizationId", StringComparison.InvariantCultureIgnoreCase)
                || valueName.Equals("stateAgencyId", StringComparison.InvariantCultureIgnoreCase)
                || valueName.Equals("educationServiceCenterId", StringComparison.InvariantCultureIgnoreCase)
                || valueName.Equals("localEducationAgencyId", StringComparison.InvariantCultureIgnoreCase)
                || valueName.Equals("schoolId", StringComparison.InvariantCultureIgnoreCase)
                || valueName.Equals("educationOrganizationNetworkId", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}