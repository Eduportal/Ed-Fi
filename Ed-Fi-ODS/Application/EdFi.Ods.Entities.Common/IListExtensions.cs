using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.Common;

namespace EdFi.Ods.Entities.Common
{
    public static class IListExtensions
    {
        public static bool SynchronizeListTo<T>(this IList<T> sourceList, IList<T> targetList, Action<T> onChildAdded, Func<T, bool> includeItem = null)
            where T : ISynchronizable //<T>
        {
            bool isModified = false;

            // Find items to delete
            var itemsToDelete =
                targetList.Where(i => includeItem == null || includeItem(i))
                    .Except(sourceList.Where(i => includeItem == null || includeItem(i)))
                    .ToList();

            foreach (var item in itemsToDelete)
            {
                // This statement causes failure in NHibernate when no version column is present. 
                //  --> (item as IChildEntity).SetParent(null);
                targetList.Remove(item);
                isModified = true;
            }

            // Copy properties on existing items
            var itemsToUpdate =
                (from p in targetList.Where(i => includeItem == null || includeItem(i))
                 from s in sourceList.Where(i => includeItem == null || includeItem(i))
                 where p.Equals(s)
                 select new {Submitted = s, Persisted = p})
                    .ToList();

            foreach (var pair in itemsToUpdate)
                isModified |= pair.Submitted.Synchronize(pair.Persisted);

            // Find items to add
            var itemsToAdd =
                sourceList.Where(i => includeItem == null || includeItem(i))
                .Except(targetList.Where(i => includeItem == null || includeItem(i)))
                .ToList();

            foreach (var item in itemsToAdd)
            {
                targetList.Add(item);

                if (onChildAdded != null)
                    onChildAdded(item);

                isModified = true;
            }

            return isModified;
        }

        public static void MapListTo<TSource, TTarget>(this IList<TSource> sourceList, IList<TTarget> targetList)
            where TSource : IMappable
        {
            if (sourceList == null)
                return;
                
            if (targetList == null)
                return;

            var targetListType = targetList.GetType();
            var itemType = GetItemType(targetListType);

            foreach (var sourceItem in sourceList.Distinct())
            {
                var targetItem = (TTarget) Activator.CreateInstance(itemType);
                sourceItem.Map(targetItem);
                targetList.Add(targetItem);
            }
        }

        private static readonly ConcurrentDictionary<Type, Type> itemTypeByUnderlyingListType = new ConcurrentDictionary<Type, Type>();

        private static Type GetItemType(Type targetListType)
        {
            Type itemType;

            if (!itemTypeByUnderlyingListType.TryGetValue(targetListType, out itemType))
            {
                var listTypes = targetListType.GetGenericArguments();

                if (listTypes.Length == 0)
                    throw new ArgumentException(string.Format("Target list type of '{0}' does not have any generic arguments.", targetListType.FullName));

                // Assumption: ItemType is last generic argument (most of the time this will be a List<T>, 
                // but it could be a CovariantIListAdapter<TBase, TDerived>.  We want the last generic argument type.
                itemType = listTypes[listTypes.Length - 1];
                itemTypeByUnderlyingListType[targetListType] = itemType;
            }
            return itemType;
        }
    }
}