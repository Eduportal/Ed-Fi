// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System.Collections.Generic;
using EdFi.Ods.Security.AuthorizationStrategies.NHibernateConfiguration;

namespace EdFi.Ods.Security.AuthorizationStrategies.Relationships
{
    public class RelationshipsAuthorizationStrategyFilterConfigurator
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
                    "LocalEducationAgencyIdToStudentUniqueId", 
                    @"StudentUniqueId IN (
                        SELECT {0}.StudentUniqueId 
                        FROM [auth].[LocalEducationAgencyIdToStudentUniqueId] {0} 
                        WHERE {0}.LocalEducationAgencyId IN (:LocalEducationAgencyId))",
                    (t, p) => p.HasPropertyNamed("StudentUniqueId")),

                new FilterApplicationDetails(
                    "SchoolIdToStudentUniqueId", 
                    @"StudentUniqueId IN (
                        SELECT {0}.StudentUniqueId 
                        FROM [auth].[SchoolIdToStudentUniqueId] {0} 
                        WHERE {0}.SchoolId IN (:SchoolId))",
                    (t, p) => p.HasPropertyNamed("StudentUniqueId")),

                new FilterApplicationDetails(
                    "LocalEducationAgencyIdToStudentUSI", 
                    @"StudentUSI IN (
                        SELECT {0}.StudentUSI 
                        FROM [auth].[LocalEducationAgencyIdToStudentUSI] {0} 
                        WHERE {0}.LocalEducationAgencyId IN (:LocalEducationAgencyId))",
                    (t, p) => p.HasPropertyNamed("StudentUSI")),

                new FilterApplicationDetails(
                    "SchoolIdToStudentUSI", 
                    @"StudentUSI IN (
                        SELECT {0}.StudentUSI 
                        FROM [auth].[SchoolIdToStudentUSI] {0} 
                        WHERE {0}.SchoolId IN (:SchoolId))",
                    (t, p) => p.HasPropertyNamed("StudentUSI")),

                new FilterApplicationDetails(
                    "LocalEducationAgencyIdToStaffUniqueId", 
                    @"StaffUniqueId IN (
                        SELECT {0}.StaffUniqueId 
                        FROM [auth].[LocalEducationAgencyIdToStaffUniqueId] {0} 
                        WHERE {0}.LocalEducationAgencyId IN (:LocalEducationAgencyId))",
                    (t, p) => p.HasPropertyNamed("StaffUniqueId")),

                new FilterApplicationDetails(
                    "SchoolIdToStaffUniqueId", 
                    @"StaffUniqueId IN (
                        SELECT {0}.StaffUniqueId 
                        FROM [auth].[SchoolIdToStaffUniqueId] {0} 
                        WHERE {0}.SchoolId IN (:SchoolId))",
                    (t, p) => p.HasPropertyNamed("StaffUniqueId")),

                new FilterApplicationDetails(
                    "LocalEducationAgencyIdToStaffUSI", 
                    @"StaffUSI IN (
                        SELECT {0}.StaffUSI 
                        FROM [auth].[LocalEducationAgencyIdToStaffUSI] {0} 
                        WHERE {0}.LocalEducationAgencyId IN (:LocalEducationAgencyId))",
                    (t, p) => p.HasPropertyNamed("StaffUSI")),

                new FilterApplicationDetails(
                    "SchoolIdToStaffUSI", 
                    @"StaffUSI IN (
                        SELECT {0}.StaffUSI 
                        FROM [auth].[SchoolIdToStaffUSI] {0} 
                        WHERE {0}.SchoolId IN (:SchoolId))",
                    (t, p) => p.HasPropertyNamed("StaffUSI")),

                new FilterApplicationDetails(
                    "LocalEducationAgencyIdToParentUniqueId", 
                    @"ParentUniqueId IN (
                        SELECT {0}.ParentUniqueId 
                        FROM [auth].[LocalEducationAgencyIdToParentUniqueId] {0} 
                        WHERE {0}.LocalEducationAgencyId IN (:LocalEducationAgencyId))",
                    (t, p) => p.HasPropertyNamed("ParentUniqueId")),

                new FilterApplicationDetails(
                    "SchoolIdToParentUniqueId", 
                    @"ParentUniqueId IN (
                        SELECT {0}.ParentUniqueId 
                        FROM [auth].[SchoolIdToParentUniqueId] {0} 
                        WHERE {0}.SchoolId IN (:SchoolId))",
                    (t, p) => p.HasPropertyNamed("ParentUniqueId")),

                new FilterApplicationDetails(
                    "LocalEducationAgencyIdToParentUSI", 
                    @"ParentUSI IN (
                        SELECT {0}.ParentUSI 
                        FROM [auth].[LocalEducationAgencyIdToParentUSI] {0} 
                        WHERE {0}.LocalEducationAgencyId IN (:LocalEducationAgencyId))",
                    (t, p) => p.HasPropertyNamed("ParentUSI")),

                new FilterApplicationDetails(
                    "SchoolIdToParentUSI", 
                    @"ParentUSI IN (
                        SELECT {0}.ParentUSI 
                        FROM [auth].[SchoolIdToParentUSI] {0} 
                        WHERE {0}.SchoolId IN (:SchoolId))",
                    (t, p) => p.HasPropertyNamed("ParentUSI")),

                // Legacy filter (alphabetic approach, rather than "claim value to target" approach)
                // While the view names should follow the alphabetic convention, the filter names shouldn't need to.
                // -------------------------------------------------------------------------------------------
                new FilterApplicationDetails(
                    "EducationOrganizationIdToLocalEducationAgencyId", 
                    @"EducationOrganizationId IN (
                        SELECT {0}.EducationOrganizationId 
                        FROM [auth].[EducationOrganizationIdToLocalEducationAgencyId] {0} 
                        WHERE {0}.LocalEducationAgencyId IN (:LocalEducationAgencyId))",
                    (t, p) => p.HasPropertyNamed("EducationOrganizationId")),

                new FilterApplicationDetails(
                    "EducationOrganizationIdToSchoolId", 
                    @"EducationOrganizationId IN (
                        SELECT {0}.EducationOrganizationId 
                        FROM [auth].[EducationOrganizationIdToSchoolId] {0} 
                        WHERE {0}.SchoolId IN (:SchoolId))",
                    (t, p) => p.HasPropertyNamed("EducationOrganizationId")),

                // -------------------------------------------------------------------------------------------

                new FilterApplicationDetails(
                    "LocalEducationAgencyIdToEducationOrganizationId", 
                    @"EducationOrganizationId IN (
                        SELECT {0}.EducationOrganizationId 
                        FROM [auth].[EducationOrganizationIdToLocalEducationAgencyId] {0} 
                        WHERE {0}.LocalEducationAgencyId IN (:LocalEducationAgencyId))",
                    (t, p) => p.HasPropertyNamed("EducationOrganizationId")),

                new FilterApplicationDetails(
                    "SchoolIdToEducationOrganizationId", 
                    @"EducationOrganizationId IN (
                        SELECT {0}.EducationOrganizationId 
                        FROM [auth].[EducationOrganizationIdToSchoolId] {0} 
                        WHERE {0}.SchoolId IN (:SchoolId))",
                    (t, p) => p.HasPropertyNamed("EducationOrganizationId")),

                new FilterApplicationDetails(
                    "LocalEducationAgencyIdToSchoolId", 
                    @"SchoolId IN (
                        SELECT {0}.SchoolId 
                        FROM [auth].[LocalEducationAgencyIdToSchoolId] {0} 
                        WHERE {0}.LocalEducationAgencyId IN (:LocalEducationAgencyId))",
                    (t, p) => p.HasPropertyNamed("SchoolId")),
            })
            .AsReadOnly();

            return filters;
        }
    }
}