// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Common.InversionOfControl;
using EdFi.Ods.Utilities.LoadGeneration.ValueBuilders;
using EdFi.TestObjects;
using log4net;

namespace EdFi.Ods.Utilities.LoadGeneration.Persistence
{
    public class StaffResourcePersisterDecorator : IResourcePersister
    {
        private ILog _logger = LogManager.GetLogger(typeof(StaffResourcePersisterDecorator));

        private readonly IResourcePersister _next;
        private readonly IEducationOrganizationIdentifiersProvider _educationOrganizationIdentifiersProvider;
        private readonly IServiceLocator _serviceLocator;
        private ITestObjectFactory _testObjectFactory;
        private readonly IApiSdkReflectionProvider _apiSdkReflectionProvider;
        private readonly IPersonEducationOrganizationCache _personEducationOrganizationCache;
        //private readonly IStaffEducationOrganizationAssociationCache _staffEducationOrganizationAssociationCache;

        public StaffResourcePersisterDecorator(IResourcePersister next, 
            IEducationOrganizationIdentifiersProvider educationOrganizationIdentifiersProvider,
            IServiceLocator serviceLocator, IApiSdkReflectionProvider apiSdkReflectionProvider,
            IPersonEducationOrganizationCache personEducationOrganizationCache)
        {
            _next = next;
            _educationOrganizationIdentifiersProvider = educationOrganizationIdentifiersProvider;
            _serviceLocator = serviceLocator;
            _apiSdkReflectionProvider = apiSdkReflectionProvider;
            _personEducationOrganizationCache = personEducationOrganizationCache;
        }

        private ITestObjectFactory TestObjectFactory
        {
            get
            {
                if (_testObjectFactory == null)
                    _testObjectFactory = _serviceLocator.Resolve<ITestObjectFactory>();

                return _testObjectFactory;
            }
        }

        public void PersistResource(object resource, IDictionary<string, object> context, out object resourceReference)
        {
            // TODO: Needs a unit test for explicit not null check.
            if (context == null)
                throw new ArgumentNullException("context");

            // Pass call through to main persistence
            _next.PersistResource(resource, context, out resourceReference);

            if (resource == null)
                return;

            // Stop additional handling now, if the resource isn't a staff
            if (!resource.GetType().Name.Equals("Staff", StringComparison.InvariantCultureIgnoreCase))
                return;

            dynamic resourceAsDynamic = resource;
            string staffUniqueId = resourceAsDynamic.staffUniqueId;

            // Establish the school context for the Staff
            int educationOrganizationId = GetEducationOrganizationId(context);

            // Create an appropriate StaffEducationOrganizationEmploymentAssociation
            dynamic employmentAssociationResource = CreateEmploymentAssociationResource(staffUniqueId, educationOrganizationId);

            //int educationOrganizationId = employmentAssociationResource.educationOrganizationReference.educationOrganizationId;
            //string staffUniqueId = employmentAssociationResource.staffReference.staffUniqueId;

            //// Make note of the association between the staff and the school
            //_personEducationOrganizationCache.AddPersonEducationOrganizationAssociation("Staff", staffUniqueId, educationOrganizationId);

            // Set the newly associated ed org into the current context
            context["educationOrganizationId"] = educationOrganizationId;

            // Persist the staff ed org association
            object ignored;
            _next.PersistResource(employmentAssociationResource, context, out ignored);

            _logger.DebugFormat("StaffUniqueId '{0}' is now associated with school id '{1}'.", staffUniqueId, educationOrganizationId);
        }

        private object CreateEmploymentAssociationResource(string staffUniqueId, int educationOrganizationId)
        {
            var constraints = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            constraints.Add("EducationOrganizationId", educationOrganizationId);
            constraints.Add("StaffUniqueId", staffUniqueId);

            Type modelType;
            
            if (!_apiSdkReflectionProvider.TryGetModelType("StaffEducationOrganizationEmploymentAssociation", out modelType))
                throw new Exception("Unable to find model type for 'StaffEducationOrganizationEmploymentAssociation' in order to establish necessary data for API key authorization.");

            object employmentAssociationResource = null;

            ICommittable committable = null;

            try
            {
                // Make note of the association between the staff and the school
                committable = _personEducationOrganizationCache.AddPersonEducationOrganizationAssociation("Staff", staffUniqueId, educationOrganizationId);

                employmentAssociationResource = TestObjectFactory.Create(string.Empty, modelType, constraints, new LinkedList<object>());

                return employmentAssociationResource;
            }
            finally
            {
                // If something bad and unexpected happened, remove the association between the staff and the ed org
                if (employmentAssociationResource == null)
                    _personEducationOrganizationCache.RemovePersonEducationOrganizationAssociation("Staff", staffUniqueId, educationOrganizationId);

                if (committable != null)
                    committable.Commit();
            }
        }

        private int GetEducationOrganizationId(IDictionary<string, object> context)
        {
            IList<int> possibleEducationOrganizations = null;
            
            if (context != null)
            {
                var caseInsensitiveContext = new Dictionary<string, object>(context, StringComparer.InvariantCultureIgnoreCase);
                object value;

                // Try to find LEA or School identifiers in context
                if (caseInsensitiveContext.TryGetValue("authorization.LocalEducationAgencyId", out value)
                    || caseInsensitiveContext.TryGetValue("authorization.SchoolId", out value)) 
                    return (int) value;

                if (caseInsensitiveContext.TryGetValue("authorization.EducationOrganizationId", out value))
                {
                    var identifiers = _educationOrganizationIdentifiersProvider.GetEducationOrganizationIdentifiers();

                    possibleEducationOrganizations = (from i in identifiers
                        where i.EducationOrganizationId == (int) value
                              && i.LocalEducationAgencyId != null
                        select i.LocalEducationAgencyId.Value)
                        .ToList();
                }
            }

            if (possibleEducationOrganizations == null)
            {
                possibleEducationOrganizations =
                    (from i in _educationOrganizationIdentifiersProvider.GetEducationOrganizationIdentifiers("LocalEducationAgency")
                    select i.LocalEducationAgencyId.Value)
                    .ToList();
            }

            return possibleEducationOrganizations.GetRandomMember();
        }
    }
}