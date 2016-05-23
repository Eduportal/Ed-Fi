// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EdFi.Ods.Utilities.LoadGeneration.Persistence;
using EdFi.Ods.Utilities.LoadGeneration._Extensions;
using EdFi.TestObjects;
using log4net;

namespace EdFi.Ods.Utilities.LoadGeneration.ValueBuilders
{
    public class GeneralReferenceStudentUniqueIdValueBuilder : IValueBuilder
    {
        private ILog _logger = LogManager.GetLogger(typeof(GeneralReferenceStudentUniqueIdValueBuilder));

        private readonly IRandomStudentUniqueIdSelector _randomStudentUniqueIdSelector;
        private readonly IApiSdkReflectionProvider _apiSdkReflectionProvider;
        private readonly IResourcePersister _resourcePersister;
        private readonly IEducationOrganizationIdentifiersProvider _educationOrganizationIdentifiersProvider;

        public GeneralReferenceStudentUniqueIdValueBuilder(
            IRandomStudentUniqueIdSelector randomStudentUniqueIdSelector, 
            IApiSdkReflectionProvider apiSdkReflectionProvider,
            IResourcePersister resourcePersister,
            IEducationOrganizationIdentifiersProvider educationOrganizationIdentifiersProvider)
        {
            _randomStudentUniqueIdSelector = randomStudentUniqueIdSelector;
            _apiSdkReflectionProvider = apiSdkReflectionProvider;
            _resourcePersister = resourcePersister;
            _educationOrganizationIdentifiersProvider = educationOrganizationIdentifiersProvider;
        }

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            // Handle StudentUniqueId properties of type string NOT on Student or StudentReference instances
            if (buildContext.TargetType != typeof(string)
                || !buildContext.LogicalPropertyPath.EndsWith("StudentUniqueId", StringComparison.InvariantCultureIgnoreCase)
                || buildContext.GetContainingTypeName().Equals("Student", StringComparison.InvariantCultureIgnoreCase)
                || buildContext.GetContainingTypeName().Equals("StudentReference", StringComparison.InvariantCultureIgnoreCase))
                return ValueBuildResult.NotHandled;

            int educationOrganizationId;
            string educationOrganizationPropertyName;

            // Try to determine ed org context, if we are unable to do so, we must defer and try again
            if (!TryEstablishEducationOrganizationContext(buildContext, out educationOrganizationId, out educationOrganizationPropertyName, buildContext.ContainingType))
                return ValueBuildResult.Deferred;

            string studentUniqueId;

            // Look for an existing student, and return value if found
            if (_randomStudentUniqueIdSelector.TryGetRandomStudentUniqueId(educationOrganizationId, out studentUniqueId))
                return ValueBuildResult.WithValue(studentUniqueId, buildContext.LogicalPropertyPath);
            
            // Prepare education organization constraints for creating a new student
            var constraints = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { educationOrganizationPropertyName, educationOrganizationId }
            };

            _logger.DebugFormat("Unable to find a student associated with education organization id '{0}' (obtained from a property named '{1}').  Creating a new student using this value as context.",
                educationOrganizationId, educationOrganizationPropertyName);

            // Create a new student
            var modelType = GetStudentModelType();
            dynamic student = Factory.Create(string.Empty, modelType, constraints, null);
                
            // Persist the student
            object reference;
            _resourcePersister.PersistResource((object) student, constraints, out reference);

            return ValueBuildResult.WithValue((string)student.studentUniqueId, buildContext.LogicalPropertyPath);
        }

        private bool TryEstablishEducationOrganizationContext(BuildContext buildContext, 
            out int educationOrganizationId, out string educationOrganizationPropertyName, Type type)
        {
            // Set defaults for out parameters
            educationOrganizationPropertyName = null;
            educationOrganizationId = 0;

            // Look for the ed org in the current build context
            var contextualEducationOrganizationValue = buildContext.PropertyValueConstraints.GetOrderedEducationOrganizationIdentifierValues()
                .FirstOrDefault();

            if (!contextualEducationOrganizationValue.Equals(default(KeyValuePair<string, object>)))
            {
                educationOrganizationId = Convert.ToInt32(contextualEducationOrganizationValue.Value);
                educationOrganizationPropertyName = contextualEducationOrganizationValue.Key;

                if (educationOrganizationId != 0)
                    return true;
            }

            // Look for ed org context on the current class
            var educationOrganizationIdProperty = 
                type.GetEducationOrganizationIdentifierProperties()
                .FirstOrDefault();

            // Did we find a property to establish the EdOrg context?
            if (educationOrganizationIdProperty != null)
            {
                object idValue = educationOrganizationIdProperty.GetValue(buildContext.GetContainingInstance());

                educationOrganizationId = idValue.ToInt32();

                // No value? Defer until we have the value
                if (educationOrganizationId == 0)
                    return false;

                educationOrganizationPropertyName = educationOrganizationIdProperty.Name;
            }
            else
            {
                //// TODO: Unit test needed (BEGIN)
                //object parentInstance = buildContext.GetParentInstance();

                //if (parentInstance != null)
                //{
                //    var parentType = parentInstance.GetType();

                //    // Look at the parent's properties
                //    educationOrganizationIdProperty =
                //        (from p in parentType.GetEducationOrganizationIdentifierProperties()
                //        where !p.PropertyType.IsCustomClass()
                //        select p)
                //        .FirstOrDefault();

                //    // If still no context found, search its references
                //    if (educationOrganizationIdProperty == null)
                //    {
                //        // Iterate properties that are custom classes
                //        foreach (var referenceProperty in parentType.GetProperties()
                //            .Where(p => p.PropertyType.IsCustomClass()))
                //        {
                //            // Look for an Ed-Org property on each reference type
                //            educationOrganizationIdProperty =
                //                referenceProperty.PropertyType.GetEducationOrganizationIdentifierProperties()
                //                    .FirstOrDefault();

                //            // If we found an EdOrg property, get the value ow
                //            if (educationOrganizationIdProperty != null)
                //            {
                //                educationOrganizationId = educationOrganizationIdProperty.GetValue()
                //            }
                //        }
                //    }
                //    // Look at the parent's references

                //}
                //// TODO: Unit test needed (END)
                
                educationOrganizationPropertyName = "SchoolId";

                // Grab a random School Id
                educationOrganizationId =
                    _educationOrganizationIdentifiersProvider.GetEducationOrganizationIdentifiers("School")
                        .GetRandomMember()
                        .EducationOrganizationId;
            }

            return true;
        }

        private Type _studentModelType;

        private Type GetStudentModelType()
        {
            if (_studentModelType == null)
            {
                if (!_apiSdkReflectionProvider.TryGetModelType("Student", out _studentModelType))
                    throw new Exception("Unable to locate School resource model type.");
            }

            return _studentModelType;
        }

        // Note: May need to refactor in the future for EdOrg extensibility

        public void Reset() { }

        public ITestObjectFactory Factory { get; set; }
    }
}