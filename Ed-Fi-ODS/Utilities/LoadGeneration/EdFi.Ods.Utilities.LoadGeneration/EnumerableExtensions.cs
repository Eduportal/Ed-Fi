// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EdFi.Ods.Utilities.LoadGeneration.Infrastructure;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public static class EnumerableExtensions
    {
        private static readonly IRandom _random = new RandomWrapper();

        /// <summary>
        /// Provides a replacable function that returns a random number generator to be used 
        /// by the <see cref="EnumerableExtensions.GetRandomMember{T}"/> extension method.
        /// </summary>
        public static Func<IRandom> Random = () => _random;

        /// <summary>
        /// Gets a random member from a collection.
        /// </summary>
        /// <param name="source">The source of items from which to randomly select.</param>
        public static T GetRandomMember<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (!source.Any())
                throw new ArgumentException("Unable to select a random item from an empty collection.");

            var sourceAsList = source as IList<T>;

            // Handle lists more efficiently
            if (sourceAsList != null)
            {
                int index = Random().Next(0, sourceAsList.Count);
                return sourceAsList[index];
            }

            // Perform random selection using the IEnumerable<T> abstraction
            var randomIndex = Random().Next(0, source.Count());
            return source.ElementAt(randomIndex);
        }
    }
}