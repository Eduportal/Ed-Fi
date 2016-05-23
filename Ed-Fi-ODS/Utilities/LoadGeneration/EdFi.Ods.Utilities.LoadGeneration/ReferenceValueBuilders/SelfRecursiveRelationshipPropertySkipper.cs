// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    /// <summary>
    /// Implements an <see cref="IValueBuilder"/> that skips properties that match the Ed-Fi convention for self-recursive properties.
    /// </summary>
    /// <remarks>
    /// Self-recursive properties are properties whose names are prefixed with "Parent" on model types that are themselves
    /// unrelated to Ed-Fi parents (i.e. the model type name doesn't contain "parent").
    /// </remarks>
    public class SelfRecursiveRelationshipPropertySkipper : IValueBuilder
    {
        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            // Only handle request for properties that start with Parent, have a containing instance available, and are not on a containing type with "Parent" in its name
            if (!buildContext.GetPropertyName().StartsWith("Parent", StringComparison.InvariantCultureIgnoreCase)
                || buildContext.GetContainingInstance() == null  
                || buildContext.GetContainingTypeName().ToLower().Contains("parent"))
                return ValueBuildResult.NotHandled;

            // Don't try to generate a value for this property, as it will cause the need to satisfy a recursive referential constraint
            return ValueBuildResult.Skip(buildContext.LogicalPropertyPath);
        }

        public void Reset()
        {
            
        }

        public ITestObjectFactory Factory { get; set; }
    }
}