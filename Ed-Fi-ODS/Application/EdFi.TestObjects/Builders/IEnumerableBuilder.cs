using System;
using System.Collections.Generic;
using System.Linq;

namespace EdFi.TestObjects.Builders
{
    public class IEnumerableBuilder : IValueBuilder
    {
        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (buildContext.TargetType.IsArray
                ||
                (buildContext.TargetType.IsGenericType
                 && (buildContext.TargetType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                     || buildContext.TargetType.GetGenericTypeDefinition() == typeof(IList<>)
                     || buildContext.TargetType.GetGenericTypeDefinition() == typeof(List<>)
                // IQueryable is generally not serializable, but this is left here for possible future inclusion
                // || targetType.GetGenericTypeDefinition() == typeof(IQueryable<>)
                    )))
            {
                Type itemType;

                if (buildContext.TargetType.IsArray)
                {
                    itemType = buildContext.TargetType.GetElementType();
                }
                else
                {
                    itemType = buildContext.TargetType.GetGenericArguments()[0];
                }

                Type listType = typeof(List<>).MakeGenericType(itemType);
                var list = Activator.CreateInstance(listType);
                var addMethod = listType.GetMethod("Add", new[] { itemType });

                var distinctValues = new HashSet<object>();

                for (int i = 0; i < Factory.CollectionCount; i++)
                {
                    var newItem = this.Factory.Create(buildContext.LogicalPropertyPath + "[" + i + "]", itemType, 
                        buildContext.PropertyValueConstraints, buildContext.ObjectGraphLineage);

                    if (newItem != null)
                        distinctValues.Add(newItem);
                }

                // Enfore distinctness in values added to the lists
                // TODO: Need unit tests for enforcing distinctness
                foreach (var item in distinctValues)
                {
                    addMethod.Invoke(list, new[] { item });
                }

                if (buildContext.TargetType.IsArray)
                {
                    // Convert list to an array
                    var toArrayMethod = listType.GetMethod("ToArray");
                    return ValueBuildResult.WithValue(toArrayMethod.Invoke(list, null), buildContext.LogicalPropertyPath);
                }
                #region IQueryable support (commented out)
                // IQueryable is generally not serializable, but this is left here for possible future inclusion
                //else if (targetType.GetGenericTypeDefinition() == typeof(IQueryable<>))
                //{
                //    // Convert list to an IQueryable
                //    var asQueryableMethod = typeof(Queryable).GetMethod("AsQueryable",
                //           new[] {typeof(IEnumerable<>).MakeGenericType(itemType)});

                    //    value = asQueryableMethod.Invoke(null, new[] {list});
                //}
                #endregion

                else
                {
                    return ValueBuildResult.WithValue(list, buildContext.LogicalPropertyPath);
                }
            }

            return ValueBuildResult.NotHandled;
        }

        public void Reset() {}

        public ITestObjectFactory Factory { get; set; }
    }
}