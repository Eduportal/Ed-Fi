using System.Collections.Generic;
using EdFi.Common.Extensions;
using EdFi.Ods.Security.AuthorizationStrategies.NHibernateConfiguration;

namespace EdFi.Ods.Security.AuthorizationStrategies.AssessmentMetadata
{
    public class AssessmentMetadataAuthorizationStrategyFilterConfigurator
        : INHibernateFilterConfigurator
    {
        /// <summary>
        /// Gets the authorization strategy's NHibernate filter definitions and a functional delegate for determining when to apply them. 
        /// </summary>
        /// <returns>A read-only list of filter application details to be applied to the NHibernate configuration and mappings.</returns>
        public IReadOnlyList<FilterApplicationDetails> GetFilters()
        {
            var filters = (new List<FilterApplicationDetails>
            {
                new FilterApplicationDetails(
                    "AssessmentMetadata", 
                    @"(Namespace IS NOT NULL AND (Namespace LIKE :Namespace) 
                        OR (Namespace IS NULL AND AssessmentFamilyTitle IN (
                            SELECT {0}.AssessmentFamilyTitle 
                            FROM edfi.AssessmentFamily {0} 
                            WHERE {0}.Namespace IS NOT NULL 
                                AND {0}.Namespace LIKE :Namespace)))",
                    (t, p) => t.Name.EqualsIgnoreCase("Assessment")),

                new FilterApplicationDetails(
                    "AssessmentMetadata", 
                    @"(AssessmentTitle IN (
                        SELECT {0}.AssessmentTitle 
                        FROM edfi.Assessment {0} 
                        WHERE (({0}.Namespace IS NOT NULL AND {0}.Namespace LIKE :Namespace) 
                            OR ({0}.Namespace IS NULL AND {0}.AssessmentFamilyTitle IN (
                                SELECT {1}.AssessmentFamilyTitle 
                                FROM edfi.AssessmentFamily {1} 
                                WHERE {1}.Namespace IS NOT NULL AND {1}.Namespace LIKE :Namespace)))))",
                    (t, p) => t.Name.EqualsIgnoreCase("AssessmentItem")
                              || t.Name.EqualsIgnoreCase("ObjectiveAssessment")
                              || t.Name.EqualsIgnoreCase("StudentAssessment")),
            }).AsReadOnly();

            return filters;
        }
    }
}