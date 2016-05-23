using System;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ValueBuilders
{
    public abstract class PropertySkipperBase : IValueBuilder
    {
        protected abstract string PropertyName { get; }

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (!IsHandled(buildContext))
                return ValueBuildResult.NotHandled;

            return ValueBuildResult.Skip(buildContext.LogicalPropertyPath);
        }

        protected virtual bool IsHandled(BuildContext buildContext)
        {
            return 
                buildContext.LogicalPropertyPath.EndsWith("." + PropertyName, StringComparison.InvariantCultureIgnoreCase)
                || buildContext.LogicalPropertyPath.Equals(PropertyName, StringComparison.InvariantCultureIgnoreCase);
        }

        public void Reset() { }
        public ITestObjectFactory Factory { get; set; }
    }
}