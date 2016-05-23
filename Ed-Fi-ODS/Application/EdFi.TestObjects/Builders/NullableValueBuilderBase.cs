using System;

namespace EdFi.TestObjects.Builders
{
    public abstract class NullableValueBuilderBase<T> : IValueBuilder
    {
        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (buildContext.TargetType.IsGenericType
                && buildContext.TargetType.GetGenericTypeDefinition() == typeof (Nullable<>))
            {
                // Reassign the nullable type to the underlying target type
                buildContext.TargetType = Nullable.GetUnderlyingType(buildContext.TargetType);
            }

            if (buildContext.TargetType == typeof (T))
            {
                var value = this.GetNextValue();
                return ValueBuildResult.WithValue(value, buildContext.LogicalPropertyPath);
            }

            return ValueBuildResult.NotHandled;
        }

        protected abstract T GetNextValue();
        public abstract void Reset();

        public ITestObjectFactory Factory { get; set; }
    }
}