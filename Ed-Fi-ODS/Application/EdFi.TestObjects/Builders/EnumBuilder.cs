using System;
using System.Collections.Generic;
using System.Linq;

namespace EdFi.TestObjects.Builders
{
    public class EnumBuilder : IValueBuilder
    {
        private Dictionary<Type, int> valueIndicesByEnumType = new Dictionary<Type, int>();
        private Dictionary<Type, List<int>> valuesByEnumType = new Dictionary<Type, List<int>>();

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (buildContext.TargetType.IsEnum)
            {
                // Initialize enum values
                if (!this.valuesByEnumType.ContainsKey(buildContext.TargetType))
                {
                    this.valuesByEnumType[buildContext.TargetType] = Enum.GetValues(buildContext.TargetType).Cast<int>().ToList();
                    this.valueIndicesByEnumType[buildContext.TargetType] = 0;
                }

                // Get the current index and list of enum values
                int index = this.valueIndicesByEnumType[buildContext.TargetType];
                var enumValues = this.valuesByEnumType[buildContext.TargetType];

                // Get the next enum value
                var value = enumValues[index];

                // Increment/cycle the index for this enum type
                this.valueIndicesByEnumType[buildContext.TargetType] = (index + 1) % enumValues.Count;

                return ValueBuildResult.WithValue(value, buildContext.LogicalPropertyPath);
            }

            return ValueBuildResult.NotHandled;
        }

        public void Reset()
        {
            this.valueIndicesByEnumType = new Dictionary<Type, int>();
            this.valuesByEnumType = new Dictionary<Type, List<int>>();
        }

        public ITestObjectFactory Factory { get; set; }
    }
}
