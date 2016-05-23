using EdFi.TestObjects;
using EdFi.TestObjects._Extensions;

namespace EdFi.Ods.Tests.TestObjects.CustomBuilders
{
    using System;
    using System.Linq;
    using System.Reflection;

    using global::EdFi.Ods.Entities.Common;
    using global::EdFi.Ods.Tests._Extensions;

    public class AutoIncrementAttributedIdSkipper : IValueBuilder
    {
        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            string logicalPropertyPath = buildContext.LogicalPropertyPath;

            if (buildContext.ContainingType == null)
                return ValueBuildResult.NotHandled;

            var propertyName = logicalPropertyPath.Split('.').Last();
            var property = buildContext.ContainingType.GetPublicProperties().SingleOrDefault(p => p.Name == propertyName);

            if (property == null)
                throw new Exception(
                    string.Format("Unable to find property '{0}' on type '{1}'.", 
                        propertyName, buildContext.ContainingType.FullName));

            string parentName = GetParentTypeName(property);

            // Make sure column matches typical database conventions (performance optimization)
            if (propertyName.Equals(parentName + "Id"))
            {
                // Make sure that property "Id" type is an integer before skipping it (and assuming it's an IDENTITY column, by convention)
                if (property.PropertyType == typeof(int))
                {
                    var attributes = this.Factory.CustomAttributeProvider.GetCustomAttributes(property, false);

                    // If the property is an auto-incrementing value, skip it
                    if (attributes.Any(x => x is AutoIncrementAttribute))
                        return ValueBuildResult.Skip(logicalPropertyPath);
                }
            }

            return ValueBuildResult.NotHandled;
        }

        private static string GetParentTypeName(PropertyInfo property)
        {
            if (property.DeclaringType.IsInterface)
            {
                string declaringTypeName = property.DeclaringType.Name;

                if (declaringTypeName.StartsWith("I") && declaringTypeName.Length > 1)
                    return declaringTypeName.Substring(1);
            }

            return property.DeclaringType.Name;
        }

        public void Reset()
        {
        }

        public ITestObjectFactory Factory { get; set; }
    }
}