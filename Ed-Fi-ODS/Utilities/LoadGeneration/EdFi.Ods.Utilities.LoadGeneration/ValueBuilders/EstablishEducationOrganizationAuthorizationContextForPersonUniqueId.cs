// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel.Registration;
using EdFi.Ods.Utilities.LoadGeneration._Extensions;
using EdFi.TestObjects;
using log4net;

namespace EdFi.Ods.Utilities.LoadGeneration.ValueBuilders
{
    // TODO: Needs unit tests
    public class EstablishEducationOrganizationAuthorizationContextForPersonUniqueId : IValueBuilder
    {
        private ILog _logger = LogManager.GetLogger(typeof(EstablishEducationOrganizationAuthorizationContextForPersonUniqueId));

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            // TODO: Needs specification class for "Primary Relationships"
            if (buildContext.LogicalPropertyPath.StartsWith("StudentSchoolAssociation", StringComparison.InvariantCultureIgnoreCase)
                || buildContext.LogicalPropertyPath.StartsWith("StaffEducationOrganizationEmploymentAssociation", StringComparison.InvariantCultureIgnoreCase))
            {
                // We're building a primary relationship class, don't try to set the authorization context from itself
                return ValueBuildResult.NotHandled;
            }

            if (!buildContext.TargetType.IsCustomClass())
            {
                if (buildContext.TargetType != typeof(string)
                    && !buildContext.LogicalPropertyPath.EndsWith("UniqueId", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ValueBuildResult.NotHandled;
                }

                // Handle deferring PersonUniqueId property if there's an ed org context present, but not available
                // Determine if there are any Ed-Org properties on the target class
                var containingTypeEdOrgProperties1 = buildContext.ContainingType.GetEducationOrganizationIdentifierProperties().ToList();

                if (containingTypeEdOrgProperties1.Any())
                {
                    // Do any have values?
                    var edOrgContextProperties =
                        (from p in containingTypeEdOrgProperties1
                        select new {Property = p, Value = p.GetValue(buildContext.GetContainingInstance())})
                        .ToList();

                    // TODO: Bug? This could conceiveably put us into a Defer-loop if some EdOrg wasn't supplied for some reason.
                    if (edOrgContextProperties.Any(x => x.Value == 0 || x.Value == null))
                        return ValueBuildResult.Deferred;

                    // BUG: GKM - This is just flat out wrong, how to decide what to use here?
                    var edOrgContext = edOrgContextProperties.FirstOrDefault();

                    if (edOrgContext != null)
                    {
                        ClearAuthorizationValuesFromContext(buildContext.PropertyValueConstraints);
                        string authorizationValueName = "authorization." + edOrgContext.Property.Name;

                        _logger.DebugFormat("Establishing education organization authorization context of [{0}={1}] from {2} for building '{3}'.",
                            authorizationValueName, edOrgContext.Value, edOrgContext.Property.DeclaringType.Name, buildContext.LogicalPropertyPath);

                        // TODO: Exploration #1
                        IDictionaryExtensions.TrySetValue(buildContext.PropertyValueConstraints, authorizationValueName, edOrgContext.Value);
                        //buildContext.PropertyValueConstraints[edOrgContext.Property.Name] = edOrgContext.Value;
                        return ValueBuildResult.NotHandled;
                    }

                    _logger.DebugFormat("Deferring the building of UniqueId property at '{0}' while education organization context is established on the containing type.", buildContext.LogicalPropertyPath);
                    
                    return ValueBuildResult.Deferred;
                }

                return ValueBuildResult.NotHandled;
            }

            //// TODO: Needs specification class for "Primary Relationships"
            //if (buildContext.LogicalPropertyPath.StartsWith("StudentSchoolAssociation", StringComparison.InvariantCultureIgnoreCase)
            //    || buildContext.LogicalPropertyPath.StartsWith("StaffEducationOrganizationEmploymentAssociation", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    // We're building a primary relationship class, don't try to set the authorization context from itself
            //    return ValueBuildResult.NotHandled;
            //}

            // Get the person-related properties
            var personUniqueIdProperties = buildContext.TargetType.GetPersonUniqueIdProperties();

            // No people properties on this class?  We're not interested.
            if (!personUniqueIdProperties.Any())
                return ValueBuildResult.NotHandled;
            
            // We need to allow EdOrg context to be established  

            // Determine if there are any Ed-Org properties on the target class
            var targetTypeEdOrgProperties = buildContext.TargetType.GetEducationOrganizationIdentifierProperties();

            // If there are any EdOrgs properties on the target type, let building continue
            if (targetTypeEdOrgProperties.Any())
                return ValueBuildResult.NotHandled; 
            
            // Our target type has no EdOrg properties on it.  Look at the containing type
            var containingTypeEdOrgProperties = buildContext.ContainingType.GetEducationOrganizationIdentifierProperties().ToList();

            // Does the containing type have any value properties to provide the context?
            if (containingTypeEdOrgProperties.Any())
            {
                var propertyInfo = containingTypeEdOrgProperties.First();
                var propertyValue = propertyInfo.GetValue(buildContext.GetContainingInstance());

                ClearAuthorizationValuesFromContext(buildContext.PropertyValueConstraints);
                string authorizationValueName = "authorization." + propertyInfo.Name;

                _logger.DebugFormat("Establishing education organization authorization context of [{0}={1}] from {2} for building '{3}'.",
                     authorizationValueName, propertyValue, propertyInfo.DeclaringType.Name, buildContext.LogicalPropertyPath);

                // Set the value into context, and allow building to continue
                // TODO: Exploration #1
                IDictionaryExtensions.TrySetValue(buildContext.PropertyValueConstraints, authorizationValueName, propertyValue);
                //buildContext.PropertyValueConstraints[propertyInfo.Name] = propertyValue;
                return ValueBuildResult.NotHandled;
            }

            var containingInstance = buildContext.GetContainingInstance();

            // Does the containing type have any other reference properties that have Edorg context on them?
            var availableEdOrgProperties =
                (from rp in buildContext.ContainingType.GetCustomClassProperties()
                from p in rp.PropertyType.GetEducationOrganizationIdentifierProperties()
                select new
                {
                    ReferenceProperty = rp,
                    EdOrgProperty = p,
                }).ToList();

            if (!availableEdOrgProperties.Any())
                return ValueBuildResult.NotHandled;

            // There are ed org properties available.
            // Find the first one with a value and set the context
            foreach (var info in availableEdOrgProperties)
            {
                var reference = info.ReferenceProperty.GetValue(containingInstance);

                if (reference == null)
                    continue;

                var propertyInfo = info.EdOrgProperty;
                var propertyValue = propertyInfo.GetValue(reference);

                if (propertyValue == 0 || propertyValue == null)
                    continue;

                ClearAuthorizationValuesFromContext(buildContext.PropertyValueConstraints);
                string authorizationValueName = "authorization."+ propertyInfo.Name;

                _logger.DebugFormat("Establishing education organization authorization context of [{0}={1}] from {2} for building '{3}'.",
                     authorizationValueName, propertyValue, propertyInfo.DeclaringType.Name, buildContext.LogicalPropertyPath);

                // We have our context
                // TODO: Exploration #1
                IDictionaryExtensions.TrySetValue(buildContext.PropertyValueConstraints, authorizationValueName, propertyValue);
                //buildContext.PropertyValueConstraints[propertyInfo.Name] = propertyValue;
                return ValueBuildResult.NotHandled;
            }

            // None of the ed org properties have values, so defer building this reference
            return ValueBuildResult.Deferred;
        }

        private void ClearAuthorizationValuesFromContext(IDictionary<string, object> propertyValueConstraints)
        {
            var keysToRemove =
                propertyValueConstraints.Where(
                    x => x.Key.StartsWith("authorization.", StringComparison.InvariantCultureIgnoreCase))
                    .Select(x => x.Key)
                    .ToList();

            if (keysToRemove.Any())
                _logger.DebugFormat("Clearing the following existing authorization context keys: {0}", string.Join(", ", keysToRemove));

            foreach (string key in keysToRemove)
                propertyValueConstraints.Remove(key);
        }

        public void Reset() { }
        public ITestObjectFactory Factory { get; set; }
    }
}