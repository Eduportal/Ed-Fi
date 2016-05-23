// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************
namespace EdFi.Ods.Security.Claims
{
    /// <summary>
    /// Defines the claim types (as URI values) used by the Ed-Fi ODS API.
    /// </summary>
    public static class EdFiOdsApiClaimTypes
    {
        /// <summary>
        /// The prefix of the namespace assigned to callers of the Ed-Fi ODS API.  This is used to establish and authorize "ownership" of certain data within the underlying ODS.
        /// </summary>
        public const string NamespacePrefix = @"http://ed-fi.org/claims/namespacePrefix";
    }
}