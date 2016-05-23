// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ValueBuilders
{
    public class PersonInstanceUniqueIdValueBuilder : IValueBuilder
    {
        private readonly IUniqueIdFactory uniqueIdFactory;

        private static Regex uniqueIdRegex = new Regex(@".*(staff|student|parent)UniqueId$", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public PersonInstanceUniqueIdValueBuilder(IUniqueIdFactory uniqueIdFactory)
        {
            this.uniqueIdFactory = uniqueIdFactory;
        }

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            //if (buildContext.TargetType != typeof(string)
            //    || !buildContext.LogicalPropertyPath.EndsWith("StudentUniqueId", StringComparison.InvariantCultureIgnoreCase)
            //    || !buildContext.GetContainingTypeName().Equals("Student", StringComparison.InvariantCultureIgnoreCase))
            //    return ValueBuildResult.NotHandled;

            // Not a string?  Quit now.
            if (buildContext.TargetType != typeof(string))
                return ValueBuildResult.NotHandled;

            // Try to match on the particular flavor of UniqueId
            var uniqueIdMatch = uniqueIdRegex.Match(buildContext.LogicalPropertyPath);

            // If didn't find a UniqueId suffix, or the containing type name doesn't match the type we found... Quit now.
            if (!uniqueIdMatch.Success
                || !buildContext.GetContainingTypeName().Equals(uniqueIdMatch.Groups[1].Value, StringComparison.InvariantCultureIgnoreCase))
                return ValueBuildResult.NotHandled;

            // We found a person (staff, student or parent)
            dynamic person = buildContext.GetContainingInstance();

            object personAsObject = person;
            bool personTypeHasBirthDateProperty = 
                null != personAsObject.GetType().GetProperty("BirthDate", 
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            // Wait for pertinent identity information to be generated
            if (string.IsNullOrEmpty(person.firstName)
                || string.IsNullOrEmpty(person.lastSurname)
                || string.IsNullOrEmpty(person.sexType)
                || (personTypeHasBirthDateProperty && person.birthDate == null))
            {
                return ValueBuildResult.Deferred;
            }

            // Create a new UniqueId
            string uniqueId = uniqueIdFactory.CreateUniqueId(
                person.firstName, 
                person.lastSurname, 
                personTypeHasBirthDateProperty ? person.birthDate : null, 
                person.sexType);

            return ValueBuildResult.WithValue(uniqueId, buildContext.LogicalPropertyPath);
        }

        public void Reset() { }
        public ITestObjectFactory Factory { get; set; }
    }
}