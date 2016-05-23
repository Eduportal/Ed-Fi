// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using EdFi.TestObjects;
using EdFi.TestObjects.Builders;

namespace EdFi.Ods.Utilities.LoadGeneration.ValueBuilders
{
    /// <summary>
    /// Provides a string value builder that generates strings that are 20 characters in length
    /// but have explicit knowledge of specific occurrences of strings with maximum lengths that
    /// are less than 20 characters (due to the absence of available metadata through Swagger
    /// and the generated REST API SDK about the maximum lengths of the values).
    /// </summary>
    public class EdFiAwareStringValueBuilder : GuidBasedExplicitLengthStringValueBuilderBase
    {
        private Dictionary<string, int> _lengthsByClassAndPropertyName;

        public EdFiAwareStringValueBuilder()
        {
            _lengthsByClassAndPropertyName = new Dictionary<string, int>(
                GetPropertiesWithMaximumLengthLessThan20(),
                StringComparer.InvariantCultureIgnoreCase);
        }

        protected override bool TryGetLength(BuildContext buildContext, out int length)
        {
            string key = buildContext.GetContainingTypeName() + "." + buildContext.GetPropertyName();

            if (_lengthsByClassAndPropertyName.TryGetValue(key, out length))
                return true;

            // Not explicitly defined as less than 20, so supply our default length
            length = 20;
            return true;
        }

        /// <summary>
        /// Returns a dictionary of maximum string lengths by property name (using the format
        /// {ClassName}.{PropertyName}) for kstrings with maximum lengths less than 20 characters.
        /// </summary>
        /// <returns></returns>
        protected virtual IDictionary<string, int> GetPropertiesWithMaximumLengthLessThan20()
        {
            return new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase)
            {
                {"CurrentStaffEducationOrgEmploymentAssociation.Department", 3},
                {"StaffEducationOrganizationEmploymentAssociation.Department", 3},
                {"StudentAddress.CountyFIPSCode", 5},
                {"EducationOrganizationAddress.CountyFIPSCode", 5},
                {"StaffAddress.CountyFIPSCode", 5},
                {"ParentAddress.CountyFIPSCode", 5},
                {"EducationContent.Version", 10},
                {"StudentAddress.PostalCode", 17},
                {"ParentAddress.PostalCode", 17},
                {"StaffAddress.PostalCode", 17},
                {"EducationOrganizationAddress.PostalCode", 17},
                // TODO: Move these TN-specific property definitions to an extension assembly
                {"StudentSchoolAssociation.HomelessMcKinneyServedIndicator", 1},
                {"StudentSchoolAssociation.HomelessUnaccompaniedIndicator", 1},
                {"StudentSchoolAssociation.SesAppliedIndicator", 1},
                {"StudentSchoolAssociation.SesReceivingIndicator", 1},
                {"StudentSectionAssociation.VocationalOutsideIEPIndicator", 1},
                {"CourseTranscript.PrivateOrOutOfStateIndicator", 1},
                {"DisciplineAction.SpecialEdModifiedIndicator", 1},
                {"StaffSectionAssociation.FederallyFundedIndicator", 1},
            };

            /*
--------------------------------------------------------------------
-- Query to execute against the ODS to get all string lengths < 20
--------------------------------------------------------------------
select c.TABLE_SCHEMA, c.TABLE_NAME, c.COLUMN_NAME, c.CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS c
where c.DATA_TYPE = 'nvarchar'
	and c.TABLE_SCHEMA in ('edfi', 'extension')
	and C.CHARACTER_MAXIMUM_LENGTH < 20
order by c.CHARACTER_MAXIMUM_LENGTH; 

--------------------------------------------------------------------
-- Query to the occurrence of various string lengths in the ODS
--------------------------------------------------------------------
select c.CHARACTER_MAXIMUM_LENGTH, count(*) as Instances
FROM INFORMATION_SCHEMA.COLUMNS c
where c.DATA_TYPE = 'nvarchar'
	and c.TABLE_SCHEMA in ('edfi', 'extension')
group by c.CHARACTER_MAXIMUM_LENGTH
order by c.CHARACTER_MAXIMUM_LENGTH
             */
        }
    }
}