using System;
using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class Section_SchoolReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            dynamic instance = buildContext.GetContainingInstance();

            return buildContext.Matches("Section", "SchoolReference")
                   && (instance.locationReference != null
                       || instance.classPeriodReference != null
                       || instance.sessionReference != null
                       || instance.courseOfferingReference != null
                       );
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic instance = buildContext.GetContainingInstance();

            if (instance.locationReference != null) 
                return new Dictionary<string, object> {{"schoolId", instance.locationReference.schoolId}};

            if (instance.classPeriodReference != null)
                return new Dictionary<string, object> { { "schoolId", instance.classPeriodReference.schoolId } };

            if (instance.sessionReference != null)
                return new Dictionary<string, object> { { "schoolId", instance.sessionReference.schoolId } };

            if (instance.courseOfferingReference != null)
                return new Dictionary<string, object> { { "schoolId", instance.courseOfferingReference.schoolId } };

            throw new InvalidOperationException("Property constraints cannot be built for a condition that isn't supposed to be handled by this value builder.");
        }
    }
}