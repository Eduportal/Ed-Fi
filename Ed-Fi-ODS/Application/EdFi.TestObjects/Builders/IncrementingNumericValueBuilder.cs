using System;
using System.Collections.Generic;

namespace EdFi.TestObjects.Builders
{
    public class IncrementingNumericValueBuilder : IValueBuilder
    {
        private Dictionary<Type, bool> nextNullableResultByType = new Dictionary<Type, bool>();
        public static double Precision = .001;

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (!buildContext.TargetType.IsValueType)
                return ValueBuildResult.NotHandled;

            if (buildContext.TargetType.IsGenericType
                && buildContext.TargetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                bool nextResultIsNull;

                // Get or initialize the flag for this nullable type
                if (!this.nextNullableResultByType.TryGetValue(buildContext.TargetType, out nextResultIsNull))
                    this.nextNullableResultByType[buildContext.TargetType] = false;

                // Flip the result for the next time we request an instance of this nullable type
                this.nextNullableResultByType[buildContext.TargetType] = !nextResultIsNull;

                // If this result should be null, do so now
                if (nextResultIsNull)
                    return ValueBuildResult.WithValue(null, buildContext.LogicalPropertyPath);

                // Reassign the nullable type to the underlying target type
                buildContext.TargetType = Nullable.GetUnderlyingType(buildContext.TargetType);
            }

            if (buildContext.TargetType == typeof(UInt16)
                || buildContext.TargetType == typeof(UInt32)
                || buildContext.TargetType == typeof(UInt64)
                || buildContext.TargetType == typeof(Byte)
                || buildContext.TargetType == typeof(SByte)
                || buildContext.TargetType == typeof(Int16)
                || buildContext.TargetType == typeof(Int32)
                || buildContext.TargetType == typeof(Int64)
                || buildContext.TargetType == typeof(Decimal)
                || buildContext.TargetType == typeof(Double)
                || buildContext.TargetType == typeof(Single)
                )
            {
                var value = this.GetNextValue(buildContext.TargetType);
                return ValueBuildResult.WithValue(value, buildContext.LogicalPropertyPath);
            }

            return ValueBuildResult.NotHandled;
        }

        public void Reset()
        {
            this.nextValueByType.Clear();
        }

        public ITestObjectFactory Factory { get; set; }

        private readonly Dictionary<Type, dynamic> nextValueByType = new Dictionary<Type, dynamic>();

        private dynamic GetNextValue(Type type)
        {
            if (!this.nextValueByType.ContainsKey(type))
                this.nextValueByType[type] = (dynamic) type.GetDefault() + (dynamic)Convert.ChangeType(Precision, type);

            dynamic nextValue = Convert.ChangeType(this.nextValueByType[type] + 1, type);
            this.nextValueByType[type] = nextValue;
            return nextValue;
        }
    }

    public static class ReflectionUtility
    {
        public static object GetDefault(this Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }
    }
}
