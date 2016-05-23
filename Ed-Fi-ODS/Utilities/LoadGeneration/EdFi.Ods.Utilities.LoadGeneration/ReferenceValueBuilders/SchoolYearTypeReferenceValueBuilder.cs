// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Reflection;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class SchoolYearTypeReferenceValueBuilder : IValueBuilder
    {
        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (!buildContext.TargetType.Name.Equals("SchoolYearTypeReference", StringComparison.InvariantCultureIgnoreCase))
                return ValueBuildResult.NotHandled;

            // Use the factory to obtain a school year value
            object schoolYearValue = Factory.Create("SchoolYear", typeof(int), null, null);
            int schoolYear = Convert.ToInt32(schoolYearValue);

            var reference = Activator.CreateInstance(buildContext.TargetType);
            
            // Find the school year property, case insensitively
            var schoolYearProperty = buildContext.TargetType.GetProperty("SchoolYear",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            // Set the value to the known single property (we don't have the luxury of letting the factory build the instnace)
            schoolYearProperty.SetValue(reference, schoolYear);

            return ValueBuildResult.WithValue(reference, buildContext.LogicalPropertyPath);
        }

        public void Reset() {}
        public ITestObjectFactory Factory { get; set; }
    }
}