// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Type;

namespace EdFi.Ods.Security.AuthorizationStrategies.NHibernateConfiguration
{
    /// <summary>
    /// Provides an NHibernate FilterDefinition and a predicate function for determining how it should be applied to the entity mappings.
    /// </summary>
    public class FilterApplicationDetails
    {
        /// <summary>
        /// Gets the NHibernate FilterDefinition to be added to the configuration.
        /// </summary>
        public FilterDefinition FilterDefinition { get; private set; }

        /// <summary>
        /// Gets the predicate functional for determining whether the filter should be applied to a particular entity.
        /// </summary>
        public Func<Type, PropertyInfo[], bool> ShouldApply { get; private set; }

        /// <summary>
        /// Creates a new filter definition, automatically parsing embedded parameters from the condition treating 
        /// parameters ending with "UniqueId" as strings, with "Id" as 32-bit integers, with "Date" as dates, and 
        /// everything else typed as strings.
        /// </summary>
        /// <param name="filterName">The name of the filter.</param>
        /// <param name="defaultConditionFormat">The default condition for the filter with numbered format specifiers marking where distinct aliases are required (e.g. "(Column1 LIKE :Parm1 OR Column2 IN (SELECT {0}.Column2 FROM Table1 {0})").</param>
        /// <param name="shouldApply">A predicate function using a mapped entity's <see cref="Type"/> and properties used to determine whether the filter should be applied to a particular entity.</param>
        public FilterApplicationDetails(
            string filterName,
            string defaultConditionFormat,
            Func<Type, PropertyInfo[], bool> shouldApply)
        {
            ShouldApply = shouldApply;

            string defaultCondition = ProcessFormatStringForAliases(defaultConditionFormat);

            var parameterNames = ParseDistinctParameterNames(defaultConditionFormat);
            var parameters = parameterNames.ToDictionary(
                n => n,
                n =>
                {
                    // Handle UniqueIds as strings
                    if (n.EndsWith("UniqueId"))
                        return (IType) NHibernateUtil.String;

                    // Handle Ids as integers
                    if (n.EndsWith("Id"))
                        return (IType) NHibernateUtil.Int32;

                    // Handle dates
                    if (n.EndsWith("Date"))
                        return (IType) NHibernateUtil.DateTime;

                    // Treat everything else a string
                    return (IType) NHibernateUtil.String;
                },
                StringComparer.InvariantCultureIgnoreCase);

            FilterDefinition = new FilterDefinition(filterName, defaultCondition, parameters, false);
        }

        /// <summary>
        /// Creates a new filter definition, using the supplied parameter definitions.
        /// </summary>
        /// <param name="filterName">The name of the filter.</param>
        /// <param name="defaultConditionFormat">The default condition for the filter with numbered format specifiers marking where distinct aliases are required (e.g. "(Column1 LIKE :Parm1 OR Column2 IN (SELECT {0}.Column2 FROM Table1 {0})").</param>
        /// <param name="parameters">The named parameters and their data types (e.g. NHibernateUtil.String).</param>
        /// <param name="shouldApply">A predicate function using a mapped entity's <see cref="Type"/> and properties used to determine whether the filter should be applied to a particular entity.</param>
        public FilterApplicationDetails(
            string filterName,
            string defaultConditionFormat,
            IDictionary<string, IType> parameters,
            Func<Type, PropertyInfo[], bool> shouldApply)
        {
            ShouldApply = shouldApply;

            string defaultCondition = ProcessFormatStringForAliases(defaultConditionFormat);

            FilterDefinition = new FilterDefinition(filterName, defaultCondition, parameters, false);
        }

        private static readonly Regex ParameterRegex = new Regex(@"\:(?<Parameter>\w+)", RegexOptions.Compiled);

        private static string ProcessFormatStringForAliases(string format)
        {
            int parameterCount = 0;

            while (format.Contains("{" + parameterCount + "}"))
                parameterCount++;

            var parameterValues = new List<object>();
            var aliasGenerator = new AliasGenerator();

            for (int i = 0; i < parameterCount; i++)
            {
                parameterValues.Add(aliasGenerator.GetNextAlias());
            }

            string defaultCondition = string.Format(format, parameterValues.ToArray());

            return defaultCondition;
        }

        private static IEnumerable<string> ParseDistinctParameterNames(string defaultCondition)
        {
            var matches = ParameterRegex.Matches(defaultCondition);

            var parameterNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (Match match in matches)
                parameterNames.Add(match.Groups["Parameter"].Value);

            return parameterNames;
        }

        public override string ToString()
        {
            return string.Format("Filter: {0} (Parameters: {1})", 
                FilterDefinition.FilterName,
                string.Join(", ", FilterDefinition.ParameterNames));
        }
    }
}