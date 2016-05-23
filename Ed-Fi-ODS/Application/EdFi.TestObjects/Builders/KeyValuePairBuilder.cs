using System.Collections.Generic;

namespace EdFi.TestObjects.Builders
{
    public class KeyValuePairBuilder : IValueBuilder
    {
        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (buildContext.TargetType.IsGenericType && buildContext.TargetType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                var typeArgs = buildContext.TargetType.GetGenericArguments();
                var kvpType = typeof(KeyValuePair<,>).MakeGenericType(typeArgs);

                var constructor = kvpType.GetConstructor(typeArgs);

                var entryKey = this.Factory.Create("xyz", typeArgs[0], buildContext.PropertyValueConstraints, buildContext.ObjectGraphLineage);
                var entryValue = this.Factory.Create("xyz", typeArgs[1], buildContext.PropertyValueConstraints, buildContext.ObjectGraphLineage);

                var kvp = constructor.Invoke(new[] { entryKey, entryValue });

                return ValueBuildResult.WithValue(kvp, buildContext.LogicalPropertyPath);
            }

            return ValueBuildResult.NotHandled;
        }

        public void Reset()
        {

        }

        public ITestObjectFactory Factory { get; set; }
    }
}
