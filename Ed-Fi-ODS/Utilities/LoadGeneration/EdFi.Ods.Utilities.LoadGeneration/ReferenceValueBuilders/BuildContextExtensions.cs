// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public static class BuildContextExtensions
    {
        /// <summary>
        /// Indicates whether the build context matches a given containing and target type names.
        /// </summary>
        /// <param name="buildContext">The BuildContext instance to be evaluated.</param>
        /// <param name="containingTypeName">The <see cref="Type.Name"/> of the desired containing type.</param>
        /// <param name="targetTypeName">The <see cref="Type.Name"/> of the desired target type.</param>
        /// <returns></returns>
        public static bool Matches(this BuildContext buildContext, string containingTypeName, string targetTypeName)
        {
            return buildContext.GetContainingTypeName().Equals(containingTypeName, StringComparison.InvariantCultureIgnoreCase)
                && buildContext.TargetType.Name.Equals(targetTypeName, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}