// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace EdFi.Common.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Converts the provided enumerable collection into a read-only list, preserving read-only 
        /// semantics, but eliminating the need for downstream ".ToList()" calls to prevent possible 
        /// repeat enumerations of the source.
        /// </summary>
        /// <typeparam name="T">The collection's item type.</typeparam>
        /// <param name="enumerable">The source collection.</param>
        /// <returns>A read-only list.</returns>
        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.ToList().AsReadOnly();
        }

        public static IEnumerable<T> InsertBefore<T>(
            this IEnumerable<T> source,
            T insertionPoint,
            T itemToInsert)
        {
            if (!source.Contains(insertionPoint))
                throw new ArgumentException(
                    "Item could not be inserted because the insertion point item was not found in the collection.");

            return source.TakeWhile(x => !x.Equals(insertionPoint))
                         .Concat(new[] {itemToInsert})
                         .Concat(source.SkipWhile(x => !x.Equals(insertionPoint)));
        }

        public static IEnumerable<T> InsertAfter<T>(
            this IEnumerable<T> source,
            T insertionPoint,
            T itemToInsert)
        {
            if (!source.Contains(insertionPoint))
                throw new ArgumentException(
                    "Item could not be inserted because the insertion point item was not found in the collection.");

            if (source.Last().Equals(insertionPoint))
                return source.Concat(new[] {itemToInsert});

            return source.TakeWhile(x => !x.Equals(insertionPoint))
                         .Concat(new[] {insertionPoint, itemToInsert})
                         .Concat(
                             source.SkipWhile(x => !x.Equals(insertionPoint))
                                   .Skip(1));
        }

        public static IEnumerable<T> InsertAtHead<T>(
            this IEnumerable<T> source,
            T itemToInsert)
        {
            yield return itemToInsert;

            foreach (var item in source)
                yield return item;
        }

        public static IEnumerable<T> Concat<T>(
            this IEnumerable<T> source,
            T item)
        {
            return source.Concat(new[] {item});
        }
    }
}