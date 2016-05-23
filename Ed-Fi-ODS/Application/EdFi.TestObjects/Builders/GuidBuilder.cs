using System;

namespace EdFi.TestObjects.Builders
{
    public class GuidBuilder : IValueBuilder
    {
        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (buildContext.TargetType.IsGenericType
                && buildContext.TargetType.GetGenericTypeDefinition() == typeof(Nullable<>)
                && Nullable.GetUnderlyingType(buildContext.TargetType) == typeof(Guid))
            {
                // TODO: For REST API usage, we don't want any nullable Guids to have values, but this is not desired generic behavior.
                return ValueBuildResult.Skip(buildContext.LogicalPropertyPath);
            }

            if (buildContext.TargetType == typeof(Guid))
            {
                return ValueBuildResult.WithValue(Guid.NewGuid(), buildContext.LogicalPropertyPath);
            }

            return ValueBuildResult.NotHandled;
        }

        public void Reset()
        {
        }

        public ITestObjectFactory Factory { get; set; }
    }
}