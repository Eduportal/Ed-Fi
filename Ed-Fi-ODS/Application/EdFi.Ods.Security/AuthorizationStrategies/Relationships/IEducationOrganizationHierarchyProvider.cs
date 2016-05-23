// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using QuickGraph;

namespace EdFi.Ods.Security.AuthorizationStrategies.Relationships
{
    /// <summary>
    /// Defines a method for obtaining a graph representing the education organization type hierarchy.
    /// </summary>
    public interface IEducationOrganizationHierarchyProvider
    {
        /// <summary>
        /// Gets the Education Organization type hierarchy represented as a graph.
        /// </summary>
        /// <returns>The hierarchy represented as a graph.</returns>
        AdjacencyGraph<string, Edge<string>> GetEducationOrganizationHierarchy();
    }
}