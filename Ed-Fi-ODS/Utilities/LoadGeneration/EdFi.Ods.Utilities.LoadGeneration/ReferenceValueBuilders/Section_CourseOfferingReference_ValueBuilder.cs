using System;
using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class Section_CourseOfferingReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            dynamic instance = buildContext.GetContainingInstance();

            return buildContext.Matches("Section", "CourseOfferingReference")
                   && (instance.locationReference != null
                       || instance.schoolReference != null
                       || instance.classPeriodReference != null
                       || instance.sessionReference != null);
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic instance = buildContext.GetContainingInstance();

            if (instance.sessionReference != null)
                return new Dictionary<string, object>
                {
                    { "schoolId", instance.sessionReference.schoolId },
                    { "termType", instance.sessionReference.termType },
                    { "schoolYear", instance.sessionReference.schoolYear },
                };

            if (instance.locationReference != null) 
                return new Dictionary<string, object>
                {
                    {"schoolId", instance.locationReference.schoolId}
                };

            if (instance.schoolReference != null) 
                return new Dictionary<string, object>
                {
                    {"schoolId", instance.schoolReference.schoolId}
                };

            if (instance.classPeriodReference != null)
                return new Dictionary<string, object>
                {
                    {"schoolId", instance.classPeriodReference.schoolId}
                };

            throw new InvalidOperationException("Property constraints cannot be built for a condition that isn't supposed to be handled by this value builder.");
        }
    }
}