using System;
using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class StandardReferenceValueBuilder : ReferenceValueBuilderBase, IValueBuilder
    {
        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            // No constraints on a standard reference
            return new Dictionary<string, object>();
        }

        protected override bool IsHandled(BuildContext buildContext)
        {
            return buildContext.TargetType.Name.EndsWith("Reference");
        }
    }
}
