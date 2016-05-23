using System;
using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class Section_SessionReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            dynamic instance = buildContext.GetContainingInstance();

            return buildContext.Matches("Section", "SessionReference")
                   && (instance.locationReference != null
                       || instance.schoolReference != null
                       || instance.classPeriodReference != null
                       || instance.courseOfferingReference != null);
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic instance = buildContext.GetContainingInstance();

            if (instance.courseOfferingReference != null)
                return new Dictionary<string, object>
                {
                    { "schoolId", instance.courseOfferingReference.schoolId },
                    { "termType", instance.courseOfferingReference.termType },
                    { "schoolYear", instance.courseOfferingReference.schoolYear },
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