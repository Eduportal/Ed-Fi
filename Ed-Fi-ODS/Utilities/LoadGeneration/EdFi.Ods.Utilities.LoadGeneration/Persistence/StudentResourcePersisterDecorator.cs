using System;
using System.Linq;
using System.Collections.Generic;
using EdFi.Common.InversionOfControl;
using EdFi.Ods.Utilities.LoadGeneration.ValueBuilders;
using EdFi.TestObjects;
using log4net;

namespace EdFi.Ods.Utilities.LoadGeneration.Persistence
{
    // TODO: Needs unit tests

    /// <summary>
    /// Implements a resource perister that adds additional steps necessary to ensure that the calling API key
    /// will have access to the newly created student by creating and persisting a StudentSchoolAssociation resource.
    /// </summary>
    public class StudentResourcePersisterDecorator : IResourcePersister
    {
        private ILog _logger = LogManager.GetLogger(typeof(StudentResourcePersisterDecorator));

        private readonly IResourcePersister _next;
        private readonly IEducationOrganizationIdentifiersProvider _educationOrganizationIdentifiersProvider;
        private readonly IServiceLocator _serviceLocator;
        private ITestObjectFactory _testObjectFactory;
        private readonly IApiSdkReflectionProvider _apiSdkReflectionProvider;
        private readonly IPersonEducationOrganizationCache _personEducationOrganizationCache;
        //private readonly IStudentSchoolAssociationCache _studentSchoolAssociationCache;

        public StudentResourcePersisterDecorator(IResourcePersister next, 
            IEducationOrganizationIdentifiersProvider educationOrganizationIdentifiersProvider,
            IServiceLocator serviceLocator, IApiSdkReflectionProvider apiSdkReflectionProvider,
            IPersonEducationOrganizationCache personEducationOrganizationCache)
            //IStudentSchoolAssociationCache studentSchoolAssociationCache)
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

            // Stop additional handling now, if the resource isn't a student
            if (!resource.GetType().Name.Equals("Student", StringComparison.InvariantCultureIgnoreCase))
                return;

            dynamic resourceAsDynamic = resource;
            string studentUniqueId = resourceAsDynamic.studentUniqueId;

            // Establish the school context for the Student
            int schoolId = GetSchoolId(context);

            // Create an appropriate StudentSchoolAssociation
            dynamic studentSchoolAssociationResource = CreateStudentSchoolAssociationResource(studentUniqueId, schoolId);

            //int schoolId = studentSchoolAssociationResource.schoolReference.schoolId;
            //string studentUniqueId = studentSchoolAssociationResource.studentReference.studentUniqueId;

            //// Make note of the association between the student and the school
            //_personEducationOrganizationCache.AddPersonEducationOrganizationAssociation("Student", studentUniqueId, schoolId);

            // Set the newly associated school into the current context
            context["schoolId"] = schoolId;

            // Persist the student school association
            object ignored;
            _next.PersistResource(studentSchoolAssociationResource, context, out ignored);

            _logger.DebugFormat("StudentUniqueId '{0}' is now associated with school id '{1}'.", studentUniqueId, schoolId);
        }

        private object CreateStudentSchoolAssociationResource(string studentUniqueId, int schoolId)
        {
            var constraints = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            constraints.Add("SchoolId", schoolId);
            constraints.Add("StudentUniqueId", studentUniqueId);

            Type modelType;
            
            if (!_apiSdkReflectionProvider.TryGetModelType("StudentSchoolAssociation", out modelType))
                throw new Exception("Unable to find model type for 'StudentSchoolAssociation' in order to establish necessary data for API key authorization.");

            object studentSchoolAssociationResource = null;

            ICommittable committable = null;

            try
            {
                // Make note of the association between the student and the school
                // This needs to happen before the association is created or we won't be able to build the object due to ExistingReference filtering on studentUniqueIds not associated with schools (a protection mechanism)
                committable = _personEducationOrganizationCache.AddPersonEducationOrganizationAssociation("Student", studentUniqueId, schoolId);

                studentSchoolAssociationResource = TestObjectFactory.Create(string.Empty, modelType, constraints, new LinkedList<object>());

                return studentSchoolAssociationResource;
            }
            finally
            {
                // If something bad and unexpected happened, remove the association between the student and the school
                if (studentSchoolAssociationResource == null)
                    _personEducationOrganizationCache.RemovePersonEducationOrganizationAssociation("Student", studentUniqueId, schoolId);
                
                if (committable != null)
                    committable.Commit();
            }
        }

        private int GetSchoolId(IDictionary<string, object> context)
        {
            IList<int> possibleSchools = null;
            
            if (context != null)
            {
                // TODO: GKM - remove this wrapper, even though it is only used internally
                var caseInsensitiveContext = new Dictionary<string, object>(context, StringComparer.InvariantCultureIgnoreCase);
                object value;

                // Try to find EdOrg-related context values
                if (caseInsensitiveContext.TryGetValue("authorization.SchoolId", out value))
                    return (int) value;

                if (caseInsensitiveContext.TryGetValue("authorization.LocalEducationAgencyId", out value)
                    || caseInsensitiveContext.TryGetValue("authorization.EducationOrganizationId", out value))
                {
                    var identifiers = _educationOrganizationIdentifiersProvider.GetEducationOrganizationIdentifiers();

                    _logger.DebugFormat("Looking for schools related to education organization id '{0}' (for creating StudentSchoolAssociation).", value);

                    possibleSchools = (from i in identifiers
                        where i.EducationOrganizationId == (int) value
                              && i.SchoolId != null
                        select i.SchoolId.Value)
                        .ToList();

                    _logger.DebugFormat("{0} school(s) found related to education organization id '{1}'.", possibleSchools.Count, value);
                }
            }

            if (possibleSchools == null || !possibleSchools.Any())
            {
                possibleSchools =
                    (from i in _educationOrganizationIdentifiersProvider.GetEducationOrganizationIdentifiers("School")
                    select i.SchoolId.Value)
                    .ToList();
            }

            return possibleSchools.GetRandomMember();
        }
    }
}