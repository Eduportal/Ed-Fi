using System;
using System.Collections.Generic;

namespace EdFi.TestObjects.Builders
{
    public class IDictionaryBuilder : IValueBuilder
    {
        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (buildContext.TargetType.IsGenericType && buildContext.TargetType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                var typeArgs = buildContext.TargetType.GetGenericArguments();
                Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeArgs);
                var dictionary = Activator.CreateInstance(dictionaryType);
                var addMethod = dictionaryType.GetMethod("Add", typeArgs);

                for (int i = 0; i < this.Factory.CollectionCount; i++)
                {
                    var entryKey = this.GetEntryKey(buildContext.LogicalPropertyPath, typeArgs[0], buildContext.PropertyValueConstraints, buildContext.ObjectGraphLineage);

                    var entryValue = this.Factory.Create(buildContext.LogicalPropertyPath + "+Value", typeArgs[1], buildContext.PropertyValueConstraints, buildContext.ObjectGraphLineage);

                    addMethod.Invoke(dictionary, new[] { entryKey, entryValue });
                }

                return ValueBuildResult.WithValue(dictionary, buildContext.LogicalPropertyPath);
            }

            return ValueBuildResult.NotHandled;
        }

        private object GetEntryKey(string logicalPropertyPath, Type keyType, IDictionary<string, object> propertyValueConstraints, LinkedList<object> objectGraphLineage)
        {
            object entryKey = null;
            int attempts = 0;

            // Make sure we get an actual key value (can't use null)
            while (entryKey == null
                    || (keyType == typeof(string) && string.IsNullOrEmpty((string)entryKey))
                && attempts < 3)
            {
                entryKey = this.Factory.Create(logicalPropertyPath + "+Key", keyType, propertyValueConstraints, objectGraphLineage);
                attempts++;
            }

            return entryKey;
        }

        public void Reset()
        {

        }

        public ITestObjectFactory Factory { get; set; }
    }
}
