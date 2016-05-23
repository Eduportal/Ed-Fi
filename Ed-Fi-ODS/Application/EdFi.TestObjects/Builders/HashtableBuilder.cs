using System;
using System.Collections;
using System.Collections.Generic;

namespace EdFi.TestObjects.Builders
{
    public class HashtableBuilder : IValueBuilder
    {
        private Type[] types = new[]
            {
                typeof(int),
                typeof(int?),
                typeof(string),
                typeof(double),
                typeof(double?),
                typeof(float),
                typeof(float?),
            };

        private int index = 0;

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (buildContext.TargetType == typeof(Hashtable))
            {
                var hashtable = new Hashtable();

                for (int i = 0; i < this.Factory.CollectionCount; i++)
                {
                    var entryKey = this.GetEntryKey(buildContext.LogicalPropertyPath, typeof(string), buildContext.PropertyValueConstraints, buildContext.ObjectGraphLineage);
                    //var entryKey = Factory.Create(logicalPropertyPath + "+Key", typeof(string));

                    this.IncrementIndex();
                    var entryValue = this.Factory.Create(buildContext.LogicalPropertyPath + "+Value", this.types[this.index], buildContext.PropertyValueConstraints, buildContext.ObjectGraphLineage);
                    this.IncrementIndex();

                    hashtable.Add(entryKey, entryValue);
                }

                return ValueBuildResult.WithValue(hashtable, buildContext.LogicalPropertyPath);
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

        private void IncrementIndex()
        {
            this.index = (this.index + 1) % this.types.Length;
        }

        public void Reset()
        {

        }

        public ITestObjectFactory Factory { get; set; }
    }
}
