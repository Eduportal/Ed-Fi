using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using System.Xml.Linq;
using Castle.Windsor;
using EdFi.Common.Extensions;
using EdFi.Common.Inflection;
using EdFi.Ods.Api.Models;
using EdFi.Ods.Api.Services.Extensions;
using EdFi.Ods.Entities.NHibernate;
using EdFi.Ods.Entities.NHibernate.Architecture;
using EdFi.Ods.CodeGen.Models.ProfileMetadata;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Common.Utils.Extensions;
using EdFi.Ods.Common.Utils.Profiles;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.WebService.Tests.Owin;
using KellermanSoftware.CompareNetObjects;
using log4net;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using Should;
using TechTalk.SpecFlow;
using SchoolEntity = EdFi.Ods.Entities.NHibernate.SchoolAggregate.School;
using LocalEducationAgencyEntity = EdFi.Ods.Entities.NHibernate.LocalEducationAgencyAggregate.LocalEducationAgency;
using StudentEntity = EdFi.Ods.Entities.NHibernate.StudentAggregate.Student;
using StudentLearningStyleEntity = EdFi.Ods.Entities.NHibernate.StudentAggregate.StudentLearningStyle;
using SchoolResource_Full = EdFi.Ods.Api.Models.Resources.School.School;
using LocalEducationAgencyResource_Full = EdFi.Ods.Api.Models.Resources.LocalEducationAgency.LocalEducationAgency;
using StudentResource_Full = EdFi.Ods.Api.Models.Resources.Student.Student;
using AssessmentFamilyEntity = EdFi.Ods.Entities.NHibernate.AssessmentFamilyAggregate.AssessmentFamily;
using AssessmentFamilyResource_Full = EdFi.Ods.Api.Models.Resources.AssessmentFamily.AssessmentFamily;
using MetadataProfiles = EdFi.Ods.CodeGen.Models.ProfileMetadata.Profiles;
using Property = EdFi.Ods.CodeGen.Models.ProfileMetadata.PropertyDefinition;
using AcademicWeekEntity = EdFi.Ods.Entities.NHibernate.AcademicWeekAggregate.AcademicWeek;
using AcademicWeekResource_Full = EdFi.Ods.Api.Models.Resources.AcademicWeek.AcademicWeek;

namespace EdFi.Ods.WebService.Tests.Profiles
{
    public static class ScenarioContextKeys
    {
        public static string Data                               = "data";
        public static string ProfileName                        = "profileName";
        public static string AssignedProfiles                   = "assignedProfiles";
        public static string ResourceModelName                  = "resourceModelName";
        public static string ResourceCollectionName             = "resourceCollectionName";
        public static string PersistedEntity                    = "persistedSchoolEntity";
        public static string OriginalEntity                     = "originalSchoolEntity";
        public static string ResourceForUpdate                  = "schoolResourceForUpdate";
        public static string ContextualPrimaryKeyPropertyCount  = "contextualPrimaryKeyPropertyCount";
        public static string ExcludedPropertyCount              = "excludedPropertyCount";
        public static string IncludedPropertyCount              = "includedPropertyCount";
    }

    [Binding]
    public sealed class ProfilesSteps
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof (ProfilesSteps));

        [Given(@"the caller is using the ""(.*)"" profile")]
        public void GivenTheCallerIsUsingTheProfile(string profileName)
        {
            // Find the profile, by name
            var profiles = FeatureContext.Current.Get<MetadataProfiles>();
            var profile = profiles.Profile.SingleOrDefault(x => x.name == profileName);
            // Save the profile name and deserialized model for use in the scenario
            ScenarioContext.Current.Set(profileName, ScenarioContextKeys.ProfileName);
            ScenarioContext.Current.Set(profile);

            var xdoc = FeatureContext.Current.Get<XDocument>("ProfilesXDocument");
            var profileElt = xdoc.Descendants("Profile").SingleOrDefault(e => e.Attribute("name").Value == profileName);
            ScenarioContext.Current.Set(profileElt, "ProfileXElement");
        }

        [Given(@"the caller is assigned the ""(.*)"" profile")]
        public void GivenTheCallerIsAssignedTheProfile(string profileName)
        {
            List<string> assignedProfiles;

            // Create (or add to) a list of assigned profiles
            if (ScenarioContext.Current.TryGetValue(ScenarioContextKeys.AssignedProfiles, out assignedProfiles))
                assignedProfiles.Add(profileName);
            else
                assignedProfiles = new List<string> { profileName };

            // Make the list available to the scenario
            ScenarioContext.Current.Set(assignedProfiles, ScenarioContextKeys.AssignedProfiles);
        }

        [When(@"a GET \(by id\) request is submitted to (.*) with an accept header content type of (?:the appropriate value for the profile in use|""(.*)"")")]
        public void WhenARequestIsSubmitted(string resourceCollectionName, string contentType)
        {
            // If no explicit content type was provided, build it automatically based on the profile and resource
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = ProfilesContentTypeHelper.CreateContentType(
                    resourceCollectionName,
                    ScenarioContext.Current.Get<string>(ScenarioContextKeys.ProfileName), 
                    ContentTypeUsage.Readable);
            }

            // Execute the GET request against the API
            var httpClient = FeatureContext.Current.Get<HttpClient>();

            httpClient.DefaultRequestHeaders.Clear();

            httpClient.DefaultRequestHeaders.Add(
                HttpRequestHeader.Accept.ToString(),
                contentType);

            var uri = OwinUriHelper.BuildApiUri("9999", resourceCollectionName + "/" + Guid.NewGuid().ToString("n"));
            var getResponseMessage = httpClient.GetAsync(uri).Sync();

            string resourceModelName = CompositeTermInflector.MakeSingular(resourceCollectionName).ToMixedCase();

            // Save the response, and the resource collection name for the scenario
            ScenarioContext.Current.Set(getResponseMessage);
            ScenarioContext.Current.Set(resourceCollectionName, ScenarioContextKeys.ResourceCollectionName);
            ScenarioContext.Current.Set(resourceModelName, ScenarioContextKeys.ResourceModelName);
        }

        [When(@"a POST request with a resource is submitted to (.*) with a request body content type of (?:the appropriate value for the profile in use|""(.*)"")")]
        public void WhenAPutRequestWithAResourceIsSubmittedToSchoolWithARequestBodyContentTypeOf(string resourceCollectionName, string contentType)
        {
            // If no explicit content type was provided, build it automatically based on the profile and resource
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = ProfilesContentTypeHelper.CreateContentType(
                    resourceCollectionName, 
                    ScenarioContext.Current.Get<string>("profileName"), 
                    ContentTypeUsage.Writable);
            }

            // Execute the POST request against the API
            var httpClient = FeatureContext.Current.Get<HttpClient>();
            HttpContent postRequestContent = null;

            httpClient.DefaultRequestHeaders.Clear();
            var container = FeatureContext.Current.Get<IWindsorContainer>();

            // TODO: GKM - Refactor to generic method

            switch (resourceCollectionName)
            {
                case "schools":
                    // Get the "GetById" repository operation
                    var getSchoolEntityById = container.Resolve<IGetEntityById<SchoolEntity>>();

                    // Retrieve an "existing" entity
                    var schoolEntityForUpdate = getSchoolEntityById.GetById(Guid.NewGuid());

                    // Map the "updated" entity to a full School resource
                    var fullSchoolResourceForUpdate = new SchoolResource_Full();
                    schoolEntityForUpdate.MapTo(fullSchoolResourceForUpdate, null);

                    //empty the id for the Post operation
                    fullSchoolResourceForUpdate.Id = Guid.Empty;

                    // Modify the client to specify that we're sending a profile-specific resource
                    httpClient.DefaultRequestHeaders.Add(HttpRequestHeader.ContentType.ToString(), contentType);

                    //build post content
                    postRequestContent = new StringContent(JsonConvert.SerializeObject(fullSchoolResourceForUpdate), Encoding.UTF8, contentType);

                    break;

                case "students":
                    // Get the "GetById" repository operation
                    var getStudentEntityById = container.Resolve<IGetEntityById<StudentEntity>>();

                    // Retrieve an "existing" entity
                    var studentEntityForUpdate = getStudentEntityById.GetById(Guid.NewGuid());

                    // Map the "updated" entity to a full School resource
                    var fullStudentResourceForUpdate = new StudentResource_Full();
                    studentEntityForUpdate.MapTo(fullStudentResourceForUpdate, null);

                    //empty the id for the Post operation
                    fullStudentResourceForUpdate.Id = Guid.Empty;

                    // Modify the client to specify that we're sending a profile-specific resource
                    httpClient.DefaultRequestHeaders.Add(HttpRequestHeader.ContentType.ToString(), contentType);

                    //build post content
                    postRequestContent = new StringContent(JsonConvert.SerializeObject(fullStudentResourceForUpdate), Encoding.UTF8, contentType);

                    break;

                default:
                    Assert.Fail("No PUT support has been added for resource '{0}'.", resourceCollectionName);
                    break;
            }

            // Post resource, using the Profile's content type
            var postResponseMessage = httpClient.PostAsync(OwinUriHelper.BuildApiUri("9999", resourceCollectionName), postRequestContent).Sync();

            ScenarioContext.Current.Set(postResponseMessage);
        }

        [When(@"a GET \(by id\) request is submitted using (.*) to (.*) with an accept header content type of (?:the appropriate value for the profile in use|""(.*)"")")]
        public void WhenAGetByIdRequestIsSubmittedUsingTheSDKWithAnAcceptHeaderContentTypeOfTheAppropriateValueForTheProfileInUse(
            string sdkUsage, string resourceCollectionName, string contentType)
        {
            switch (sdkUsage)
            {
                case "the SDK":
                    WhenARequestIsSubmittedWithSDK(resourceCollectionName, contentType);
                    break;
                case "raw JSON":
                    WhenARequestIsSubmittedWithRawJson(resourceCollectionName, contentType);
                    break;
                default:
                    throw new NotSupportedException("Unsupported API usage type");
            }

            string resourceModelName = CompositeTermInflector.MakeSingular(resourceCollectionName).ToMixedCase();
            ScenarioContext.Current.Set(resourceCollectionName, ScenarioContextKeys.ResourceCollectionName);
            ScenarioContext.Current.Set(resourceModelName, ScenarioContextKeys.ResourceModelName);
        }

        private static void WhenARequestIsSubmittedWithRawJson(string resourceCollectionName, string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                contentType = ProfilesContentTypeHelper.CreateContentType(resourceCollectionName,
                    ScenarioContext.Current.Get<string>(ScenarioContextKeys.ProfileName), ContentTypeUsage.Readable);

            var httpClient = FeatureContext.Current.Get<HttpClient>();

            httpClient.DefaultRequestHeaders.Clear();

            httpClient.DefaultRequestHeaders.Add(
                HttpRequestHeader.Accept.ToString(),
                contentType);

            var uri = OwinUriHelper.BuildApiUri("9999", resourceCollectionName + "/" + Guid.NewGuid().ToString("n"));
            var getResponseMessage = httpClient.GetAsync(uri).Sync();

            ScenarioContext.Current.Set(getResponseMessage);
        }

        private void WhenARequestIsSubmittedWithSDK(string resourceCollectionName, string contentType)
        {
            if (!string.IsNullOrEmpty(contentType))
                throw new Exception("Explicit content types cannot be specified for step definitions using the SDK client as the appropriate content types are supplied automatically by the client.");

            var resourceUppercased = resourceCollectionName.First().ToString().ToUpper() + resourceCollectionName.Substring(1);
            var resourceProfile = (ScenarioContext.Current.Get<string>(ScenarioContextKeys.ProfileName) + "." + resourceUppercased + "Api").Replace('-', '_');

            string tempSdkAssemblyPath = FeatureContext.Current.Get<string>("SdkAssemblyPath");
            var resourceApiType = GetTypeFromAssembly(tempSdkAssemblyPath, resourceProfile);

            var restClient = FeatureContext.Current.Get<IRestClient>("restClient");
            var newResourceApiInstance = Activator.CreateInstance(resourceApiType, restClient);


            var resourceGetByIdMethod = resourceApiType.GetMethod(string.Format("Get{0}ById", resourceUppercased));
            var arguments = new object[]
            {
                Guid.NewGuid().ToString("n"),
                string.Empty
            };

            dynamic fullPayloadResponse = resourceGetByIdMethod.Invoke(newResourceApiInstance, arguments);
            var responseAsJsonObject = Json.Decode(JsonConvert.SerializeObject(fullPayloadResponse.Data, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            }));

            ScenarioContext.Current.Set(responseAsJsonObject, ScenarioContextKeys.Data);
        }

        private static Type GetTypeFromAssembly(string sdkAssemblyPath, string typeToRetrieve)
        {
            var reflectedAssemblyTypes = Assembly.LoadFrom(sdkAssemblyPath).GetTypes();
            var resourceApiType = reflectedAssemblyTypes.First(type => type.FullName.Contains(typeToRetrieve));

            return resourceApiType;
        }

        private void CopyContextualSignatureProperties(object sourceEntity, object targetEntity)
        {
            var contextualDomainSignatureProperties = sourceEntity.GetType()
                .GetProperties()
                .Where(p => 
                    p.GetCustomAttribute<DomainSignatureAttribute>() != null
                        && (p.PropertyType.IsValueType || p.PropertyType == typeof(string)))
                .ToList();

            // Copy signature properties from source to target
            foreach (var property in contextualDomainSignatureProperties)
                property.SetValue(targetEntity, property.GetValue(sourceEntity));

            // Get all the child collections to process
            var childCollectionProperties = sourceEntity.GetType()
                .GetProperties()
                .Where(p => p.PropertyType.Name.StartsWith("IList"))
                .ToList();

            foreach (var property in childCollectionProperties)
            {
                var sourceList = (IList) property.GetValue(sourceEntity);
                var targetList = (IList) property.GetValue(targetEntity);

                if (sourceList.Count != targetList.Count)
                    Assert.Fail("While copying contextual keys to child collections, the number of items in the lists did not match.");

                for (int i = 0; i < sourceList.Count; i++)
                    CopyContextualSignatureProperties(sourceList[i], targetList[i]);
            }
        }

        [When(@"a PUT request with a completely updated resource( with preserved child collections)? is submitted using (.*) to (.*) with a request body content type of (?:the appropriate value for the profile in use|""(.*)"")")]
        public void WhenAPUTRequestWithACompletelyUpdatedResourceIsSubmittedUsingTheSDKToSchoolsWithARequestBodyContentTypeOfTheAppropriateValueForTheProfileInUse(
            string withPreservedChildCollections, string sdkUsage, string resourceCollectionName, string contentType)
        {
            ScenarioContext.Current.Set(resourceCollectionName, ScenarioContextKeys.ResourceCollectionName);

            bool preserveChildCollections = !string.IsNullOrWhiteSpace(withPreservedChildCollections);

            if (string.IsNullOrEmpty(contentType))
                contentType = ProfilesContentTypeHelper.CreateContentType(resourceCollectionName,
                    ScenarioContext.Current.Get<string>(ScenarioContextKeys.ProfileName), ContentTypeUsage.Writable);

            switch (resourceCollectionName)
            {
                case "assessmentFamilies":
                    PutResourceItem<AssessmentFamilyEntity, AssessmentFamilyResource_Full>(
                        resourceCollectionName, contentType, preserveChildCollections, sdkUsage);
                    break;

                case "schools":
                    PutResourceItem<SchoolEntity, SchoolResource_Full>(
                        resourceCollectionName, contentType, preserveChildCollections, sdkUsage);
                    break;

                case "localEducationAgencies":
                    PutResourceItem<LocalEducationAgencyEntity, LocalEducationAgencyResource_Full>(
                        resourceCollectionName, contentType, preserveChildCollections, sdkUsage);
                    break;

                case "students":
                    PutResourceItem<StudentEntity, StudentResource_Full>(
                        resourceCollectionName, contentType, preserveChildCollections, sdkUsage);
                    break;

                case "academicWeeks":
                    PutResourceItem<AcademicWeekEntity, AcademicWeekResource_Full>(
                        resourceCollectionName, contentType, preserveChildCollections, sdkUsage);
                    break;

                default:
                    Assert.Fail("No PUT support has been added for resource '{0}'.", resourceCollectionName);
                    break;
            }
        }

        private void PutResourceItem<TEntity, TResource>(
            string resourceCollectionName,
            string contentType,
            bool preserveChildCollections,
            string sdkUsage)
            where TEntity : IHasIdentifier, IDateVersionedEntity, new()
            where TResource : new()
        {
            // Get the "GetById" repository operation
            var container = FeatureContext.Current.Get<IWindsorContainer>();
            var getEntityById = container.Resolve<IGetEntityById<TEntity>>();

            // Retrieve an "existing" entity
            Guid id = Guid.NewGuid();
            var entity = getEntityById.GetById(id);

            // Find the mapper method
            string mapperTypeName = string.Format(
                "{0}.{1}Mapper",
                typeof(Marker_EdFi_Ods_Entities_Common).Namespace,
                typeof(TEntity).Name);

            Type mapperType = Type.GetType(mapperTypeName + ", " + typeof(Marker_EdFi_Ods_Entities_Common).Assembly.GetName().Name);
            var mapperMethod = mapperType.GetMethod("MapTo", BindingFlags.Public | BindingFlags.Static);

            // Make a copy of the entity for subsequent comparison
            var originalEntity = new TEntity();
            mapperMethod.Invoke(null, new object[] { entity, originalEntity, null });

            var comparer = new CompareObjects();

            if (!comparer.Compare(entity, originalEntity))
                Assert.Fail("These entities should be identical at this point. " + comparer.DifferencesString);

            // Create a second entity with all different values
            var entityForUpdate = getEntityById.GetById(Guid.NewGuid());

            // Assign the "updated" entity the original entity's identifier and primary key
            entityForUpdate.Id = id;

            // Copy the child collection contextual primary keys to the "updated" version so that 
            // they are seen as the same entities (rather than deletions and additions)
            if (preserveChildCollections)
                CopyContextualSignatureProperties(entity, entityForUpdate);

            // Map the "updated" entity to a full School resource
            var fullResourceForUpdate = new TResource();
            mapperMethod.Invoke(null, new object[] { entityForUpdate, fullResourceForUpdate, null });

            var putResponseMessage = new HttpResponseMessage();
            switch (sdkUsage)
            {
                case "raw JSON":
                    var httpClient = FeatureContext.Current.Get<HttpClient>();
                    httpClient.DefaultRequestHeaders.Clear();

                    // Modify the client to specify that we're sending a profile-specific resource
                    httpClient.DefaultRequestHeaders.Add(
                        HttpRequestHeader.ContentType.ToString(),
                        contentType);

                    // Put the "updated" full school resource, using the Profile's content type
                    putResponseMessage = httpClient.PutAsync(
                       OwinUriHelper.BuildApiUri("9999", resourceCollectionName + "/" + id.ToString("n")),
                       new StringContent(
                           JsonConvert.SerializeObject(fullResourceForUpdate),
                           Encoding.UTF8,
                           contentType))
                        .Sync();
                    break;

                case "the SDK":
                    string tempSdkAssemblyPath = FeatureContext.Current.Get<string>("SdkAssemblyPath");

                    var resourceUppercased = resourceCollectionName.First().ToString().ToUpper() + resourceCollectionName.Substring(1);
                    var resourceProfileApi = (ScenarioContext.Current.Get<string>(ScenarioContextKeys.ProfileName) + "." + resourceUppercased + "Api").Replace('-', '_');
                    var resourceApiType = GetTypeFromAssembly(tempSdkAssemblyPath, resourceProfileApi);

                    var restClient = FeatureContext.Current.Get<IRestClient>("restClient");
                    var newResourceApiInstance = Activator.CreateInstance(resourceApiType, restClient);
                    
                    var singularEntity = CompositeTermInflector.MakeSingular(resourceCollectionName);
                    var entityName = singularEntity.First().ToString().ToUpper() + singularEntity.Substring(1);
                    var resourcePutMethod = resourceApiType.GetMethod(string.Format("Put{0}", entityName));

                    var resourceProfileModel = (ScenarioContext.Current.Get<string>(ScenarioContextKeys.ProfileName) + ".").Replace('-', '_');
                    var resourcePutType = GetTypeFromAssembly(tempSdkAssemblyPath, resourceProfileModel + entityName + "_Writable");
                    dynamic resourcePutObject = Json.Decode(JsonConvert.SerializeObject(fullResourceForUpdate), resourcePutType);

                    var arguments = new object[]
                    {
                        id.ToString("n"),
                        string.Empty,
                        resourcePutObject
                    };
                    
                    var restPutMessage = (RestResponse)resourcePutMethod.Invoke(newResourceApiInstance, arguments);
                    putResponseMessage.Content = new StringContent(restPutMessage.Content);
                    putResponseMessage.StatusCode = restPutMessage.StatusCode;
                    break;

                default:
                    Assert.Fail("No PUT support has been added for resource '{0}'.", resourceCollectionName);
                    break;
            }

            ScenarioContext.Current.Set(putResponseMessage);

            // Obtain the entity that was "upserted" directly from the fake repository
            var updateEntity = container.Resolve<IUpsertEntity<TEntity>>();
            var repository = (FakeRepository<TEntity>)updateEntity;

            // Save values into the scenario context
            ScenarioContext.Current.Set(repository.EntitiesById[id], ScenarioContextKeys.PersistedEntity);
            ScenarioContext.Current.Set(originalEntity, ScenarioContextKeys.OriginalEntity);
            ScenarioContext.Current.Set(fullResourceForUpdate, ScenarioContextKeys.ResourceForUpdate);
        }

        [Then(@"the number of properties on the response model should reflect the number of included properties plus the Id and primary key properties")]
        public void ThenTheResponseModelShouldOnlyContainTheIncludedElements()
        {
            dynamic data = GetDataFromResponse();
            List<string> propertyNames = GetPropertyNames(data);

            var readContentType = GetReadContentType("School");

            propertyNames.Count.ShouldEqual(2 + (readContentType.Collection ?? new ClassDefinition[0]).Length +
                                            (readContentType.Property ?? new Property[0]).Length);
        }

        [Then(@"the response model should contain the id and the primary key properties of \[(.*)\]")]
        public void ThenTheResponseModelShouldContainTheIdAndPrimaryKey(string primaryKeyProperties)
        {
            dynamic data = GetDataFromResponse();

            var actualPropertyNames = (List<string>) GetPropertyNames(data);

            // Id
            Assert.That(actualPropertyNames, Has.Member("id"));

            // Primary key fields
            var primaryKeyPropertyNames = ParseNamesFromCsvText(primaryKeyProperties);

            foreach (string propertyName in primaryKeyPropertyNames)
                Assert.That(actualPropertyNames, Has.Member(propertyName));

            ScenarioContext.Current.Set(primaryKeyPropertyNames.Count, ScenarioContextKeys.ContextualPrimaryKeyPropertyCount);
        }

        [Then(@"the response model should contain the explicitly included properties of \[(.*)\]")]
        public void ThenTheResponseModelShouldContainTheIncludedProperties(string includedProperties)
        {
            dynamic data = GetDataFromResponse();
            
            List<string> actualPropertyNames = GetPropertyNames(data);

            var includedPropertyNames = ParseNamesFromCsvText(includedProperties);

            // Included concrete elements
            foreach (string propertyName in includedPropertyNames)
                Assert.That(actualPropertyNames, Has.Member(propertyName));

            ScenarioContext.Current.Set(includedPropertyNames.Count, ScenarioContextKeys.IncludedPropertyCount);
        }

        [Then(@"the response model should contain the explicitly included regular reference properties")]
        public void ThenTheResponseModelShouldContainTheExplicitlyIncludedRegularReferenceProperties()
        {
            //<Property name="LocalEducationAgencyReference" />                   <!-- Regular reference -->
            //<Property name="CharterApprovalSchoolYearTypeReference" />          <!-- Role-named reference -->

            dynamic data = GetDataFromResponse();
            List<string> propertyNames = GetPropertyNames(data);

            // Included reference elements
            Assert.That(propertyNames, Contains.Item("LocalEducationAgencyReference".ToCamelCase()));
        }

        [Then(@"the response model should contain the explicitly included role-named reference properties")]
        public void ThenTheResponseModelShouldContainTheExplicitlyIncludedRoleNamedReferenceProperties()
        {
            //<Property name="LocalEducationAgencyReference" />                   <!-- Regular reference -->
            //<Property name="CharterApprovalSchoolYearTypeReference" />          <!-- Role-named reference -->

            dynamic data = GetDataFromResponse();
            List<string> propertyNames = GetPropertyNames(data);

            // Included role-named reference elements
            Assert.That(propertyNames, Contains.Item("CharterApprovalSchoolYearTypeReference".ToCamelCase()));
        }

        [Then(@"the response model should not contain the explicitly excluded reference properties")]
        public void ThenTheResponseModelShouldNotContainTheExplicitlyExcludedReferenceProperties()
        {
            dynamic data = GetDataFromResponse();
            List<string> actualPropertyNames = GetPropertyNames(data);

            var profile = ScenarioContext.Current.Get<Profile>();
            var resource = profile.Resource.Single(x => x.name == "School");

            if (resource.ReadContentType.memberSelection != MemberSelectionMode.ExcludeOnly)
                Assert.Fail(
                    "The step definition can only be used ReadContentTypes with a memberSelectionMode of 'ExcludeOnly'.");

            var excludedMembers =
                (resource.ReadContentType.Collection ?? new ClassDefinition[0]).Select(x => x.name)
                    .Concat((resource.ReadContentType.Property ?? new Property[0]).Select(x => x.name))
                    .Select(TrimIdFromDescriptorsAndTypes);

            foreach (var excludedMember in excludedMembers)
                Assert.That(actualPropertyNames, Has.No.Member(excludedMember));

        }

        [Then(@"the persisted entity model should have unmodified primary key values")]
        public void ThenThePersistedEntityModelShouldHaveAnUnmodifiedPrimaryKeyValue()
        {
            var persistedSchoolEntity = ScenarioContext.Current.Get<SchoolEntity>(ScenarioContextKeys.PersistedEntity);
            var originalSchoolEntity = ScenarioContext.Current.Get<SchoolEntity>(ScenarioContextKeys.OriginalEntity);

            persistedSchoolEntity.SchoolId.ShouldEqual(originalSchoolEntity.SchoolId);
        }

        [Then(@"the persisted entity model should not have new values assigned to the explicitly excluded references' properties")]
        public void ThenThePersistedEntityModelShouldNotHaveTheNewValuesAssignedToTheExplicitlyExcludedReferencesProperties()
        {
            var persistedSchoolEntity = ScenarioContext.Current.Get<SchoolEntity>(ScenarioContextKeys.PersistedEntity);
            var originalSchoolEntity = ScenarioContext.Current.Get<SchoolEntity>(ScenarioContextKeys.OriginalEntity);

            var profile = ScenarioContext.Current.Get<Profile>();
            var resource = profile.Resource.Single(x => x.name == "School");

            if (resource.ReadContentType.memberSelection != MemberSelectionMode.ExcludeOnly)
                Assert.Fail(
                    "The step definition can only be used for ReadContentTypes with a memberSelectionMode of 'ExcludeOnly'.");

            var excludedReferenceProperties =
                (resource.ReadContentType.Collection ?? new ClassDefinition[0]).Select(x => x.name)
                    .Concat((resource.ReadContentType.Property ?? new Property[0]).Select(x => x.name))
                    .Where(x => x.EndsWith("Reference"))
                    .Select(ConvertResourcePropertyNameToEntityPropertyName)
                    .ToList();

            var comparer = new CompareObjects {MaxDifferences = 100};
            comparer.Compare(originalSchoolEntity, persistedSchoolEntity);

            // Verify that none of the excluded references are in the change list
            var changedValuePropertyNames = comparer.Differences
                .Select(x => x.PropertyName.TrimStart('.').Split('[')[0])
                .Distinct()
                .ToList();

            foreach (var excludedReferenceProperty in excludedReferenceProperties)
                Assert.That(changedValuePropertyNames, Has.No.Member(excludedReferenceProperty));
        }

        [Then(@"the only values changed should be the explicitly included values")]
        public void ThenTheOnlyValuesChangedShouldBeTheExplicitlyIncludedValues()
        {
            var persistedSchoolEntity = ScenarioContext.Current.Get<SchoolEntity>(ScenarioContextKeys.PersistedEntity);
            var originalSchoolEntity = ScenarioContext.Current.Get<SchoolEntity>(ScenarioContextKeys.OriginalEntity);

            var profile = ScenarioContext.Current.Get<Profile>();
            var resource = profile.Resource.Single(x => x.name == "School");

            if (resource.WriteContentType.memberSelection != MemberSelectionMode.IncludeOnly)
                Assert.Fail(
                    "The step definition can only be used for WriteContentTypes with a memberSelectionMode of 'IncludeOnly'.");

            var includedProperties =
                resource.WriteContentType.Collection.Select(x => x.name)
                    .Concat(resource.WriteContentType.Property.Select(x => x.name))
                    .Select(TrimIdFromDescriptorsAndTypes);

            var comparer = new CompareObjects {MaxDifferences = 100};
            comparer.Compare(originalSchoolEntity, persistedSchoolEntity);

            // Verify that all the differences are covered by the expected list
            var changedValuesPropertyNames = comparer.Differences.Select(x => x.PropertyName).ToList();
            changedValuesPropertyNames.All(dp => includedProperties.Any(ip => dp.TrimStart('.').StartsWith(ip)))
                .ShouldBeTrue();
        }

        [Then(@"the only values changed should be the explicitly included regular and role-named references' properties")]
        public void ThenTheOnlyValuesChangedShouldBeTheExplicitlyIncludedRegularAndRoleNamedReferencesProperties()
        {
            var persistedSchoolEntity = ScenarioContext.Current.Get<SchoolEntity>(ScenarioContextKeys.PersistedEntity);
            var originalSchoolEntity = ScenarioContext.Current.Get<SchoolEntity>(ScenarioContextKeys.OriginalEntity);

            var profile = ScenarioContext.Current.Get<Profile>();
            var resource = profile.Resource.Single(x => x.name == "School");

            var includedProperties =
                (resource.WriteContentType.Collection ?? new ClassDefinition[0]).Select(x => x.name)
                    .Concat((resource.WriteContentType.Property ?? new Property[0]).Select(x => x.name))
                    .Select(TrimIdFromDescriptorsAndTypes);

            var comparer = new CompareObjects {MaxDifferences = 100};
            comparer.Compare(originalSchoolEntity, persistedSchoolEntity);

            // This "map" is defined based on hard-coded knowledge of the profiles being used for testing
            // The effort to obtaining the physical database information here outweighs the benefit
            var explicitlyMappedIncludes =
                includedProperties.Select(
                    p =>
                    {
                        // Map the known LEA reference to the corresponding property name
                        if (p.EqualsIgnoreCase("LocalEducationAgencyReference"))
                            return "LocalEducationAgencyId";

                        // Map the known role-named reference to the corresponding property name
                        if (p.EqualsIgnoreCase("CharterApprovalSchoolYearTypeReference"))
                            return "CharterApprovalSchoolYear";

                        // This really shouldn't be used by this step and the profile defined, but map others literally
                        return p;
                    });

            // Verify that all the differences are covered by the expected list
            var differingEntityPropertyNames = comparer.Differences.Select(x => x.PropertyName).ToList();
            differingEntityPropertyNames.All(dp => explicitlyMappedIncludes.Any(ip => dp.TrimStart('.').StartsWith(ip)))
                .ShouldBeTrue();
        }

        [Then(@"the response should indicate success")]
        public void ThenTheResponseShouldIndicateSuccess()
        {
            var response = ScenarioContext.Current.Get<HttpResponseMessage>();
            Assert.That(response.StatusCode,
                Is.EqualTo(HttpStatusCode.OK)
                    .Or.EqualTo(HttpStatusCode.NoContent)
                    .Or.EqualTo(HttpStatusCode.Created),
                    response.Content.ReadAsStringAsync().Result);
        }

        [Then(@"the response should contain a ([0-9]+) (.*) failure indicating that ""(.*)""")]
        public void ThenTheResponseShouldContiainAFailureIndicatingThat(HttpStatusCode statusCode, string reasonPhrase, // TODO: GKM - Spelling
            string coreErrorMessageTemplate)
        {
            var response = ScenarioContext.Current.Get<HttpResponseMessage>();
            response.StatusCode.ShouldEqual(statusCode);
            response.ReasonPhrase.ShouldEqual(reasonPhrase);

            var data = GetDataFromResponse();

            // Use regular expression to remove embedded parameters using the format of "This is a 'parameter' in a string"
            string messageTrimmedAtColon = data.Message.Split(':')[0];

            string redactedActualMessage = Regex.Replace(messageTrimmedAtColon, "( '[^']+'|'[^']+'[/. ]?)", string.Empty, RegexOptions.IgnoreCase);
            string redactedMessageTemplate = Regex.Replace(coreErrorMessageTemplate, "( '?{[^}]+}'?|'?{[^}]+}'?[/. ]?)", string.Empty, RegexOptions.IgnoreCase);

            if (!redactedActualMessage.ContainsIgnoreCase(redactedMessageTemplate))
                Assert.Fail("The actual HTTP error message returned did not match the supplied template.\r\n    Message:  {0}\r\n    Template: {1}", 
                    data.Message, coreErrorMessageTemplate);
        }

        [Then(@"the response model should not contain the explicitly excluded properties of \[(.*)\]")]
        public void ThenTheResponseModelShouldNotContainTheExplicitlyExcludedProperties(string excludedProperties)
        {
            dynamic data = GetDataFromResponse();
            
            List<string> actualPropertyNames = GetPropertyNames(data);

            var excludedPropertyNames = ParseNamesFromCsvText(excludedProperties);

            excludedPropertyNames.ForEach(p => Assert.That(actualPropertyNames, Has.No.Member(p)));

            ScenarioContext.Current.Set(excludedPropertyNames.Count, ScenarioContextKeys.ExcludedPropertyCount);
        }

        [Then(@"the number of properties on the response model should reflect the number of properties \(including the Id and primary key properties\) less the excluded ones")]
        public void ThenTheNumberOfPropertiesOnTheResponseModelShouldReflectTheNumberOfPropertiesIncludingTheIdAndPrimaryKeyPropertiesLessTheExcludedOnes()
        {
            dynamic data = GetDataFromResponse();

            var resourcesInProfile = ScenarioContext.Current.Get<Profile>().Resource;

            int profileExcludedMemberCount =
                resourcesInProfile.Sum(r =>
                    GetResourceMemberData(r, ContentTypeUsage.Readable).MemberCount);

            int fullResourcePropertyCount =
                typeof (SchoolResource_Full)
                    .GetProperties()
                    .Count(p => p.Name != "ETag");

            int actualIncludedPropertyCount = GetPropertyNames(data).Count;

            int actualExcludedMemberCount = fullResourcePropertyCount - actualIncludedPropertyCount;

            actualExcludedMemberCount.ShouldEqual(profileExcludedMemberCount);
        }

        [Then(@"the persisted entity model should not have the new values assigned to the explicitly excluded properties")]
        public void ThenThePersistedEntityModelShouldNotHaveTheNewValuesAssignedToTheExplicitlyExcludedProperties()
        {
            var persistedSchoolEntity = ScenarioContext.Current.Get<SchoolEntity>(ScenarioContextKeys.PersistedEntity);
            var originalSchoolEntity = ScenarioContext.Current.Get<SchoolEntity>(ScenarioContextKeys.OriginalEntity);

            var profile = ScenarioContext.Current.Get<Profile>();
            var resource = profile.Resource.Single(x => x.name == "School");

            var excludedEntityPropertyNames =
                resource.WriteContentType.Collection.Select(x => x.name)
                    .Concat(resource.WriteContentType.Property.Select(x => x.name))
                    .Select(ConvertResourcePropertyNameToEntityPropertyName);

            var comparer = new CompareObjects {MaxDifferences = 100};
            comparer.Compare(originalSchoolEntity, persistedSchoolEntity);

            var changedPropertyNames =
                comparer.Differences
                    .Select(diff => diff.PropertyName.TrimStart('.'))
                    .ToList();

            Assert.That(excludedEntityPropertyNames.Intersect(changedPropertyNames).Any(), Is.False);
        }

        [Then(@"the only values not changed should be the explicitly excluded values, the id, and the primary key values")]
        public void ThenTheOnlyValuesNotChangedShouldBeTheExplicitlyExcludedValuesTheIdAndThePrimaryKeyValues()
        {
            var persistedSchoolEntity = ScenarioContext.Current.Get<SchoolEntity>(ScenarioContextKeys.PersistedEntity);
            var originalSchoolEntity = ScenarioContext.Current.Get<SchoolEntity>(ScenarioContextKeys.OriginalEntity);

            var profile = ScenarioContext.Current.Get<Profile>();
            var resource = profile.Resource.Single(x => x.name == "School");

            var excludedPropertyNames =
                // Get resource's child member names
                (resource.WriteContentType.Collection ?? new ClassDefinition[0]).Select(x => x.name)
                    .Concat((resource.WriteContentType.Property ?? new Property[0]).Select(x => x.name))
                    // Convert the member names from resource to entity property names
                    .Select(ConvertResourcePropertyNameToEntityPropertyName)
                    .ToList();

            var comparer = new CompareObjects {MaxDifferences = 100};
            comparer.Compare(originalSchoolEntity, persistedSchoolEntity);
            var differingPropertyNames = comparer
                .Differences
                // Normalize Types/Descriptor properties to the form using the Id suffix
                .Select(
                    diff =>
                        ConvertResourcePropertyNameToEntityPropertyName(diff.PropertyName.TrimStart('.').Split('[')[0]))
                // Eliminate duplicates caused by Id suffix normalization on entities
                .Distinct();

            var allEntityProperties = typeof (SchoolEntity)
                .GetProperties()
                // Normalize Types/Descriptor properties to the form using the Id suffix
                .Select(property => ConvertResourcePropertyNameToEntityPropertyName(property.Name))
                // Eliminate duplicates caused by Id suffix normalization on entities
                .Distinct();

            // the only values not changed ... 
            var actualUnchangedPropertyNames =
                allEntityProperties
                    .Except(differingPropertyNames)
                    .Where(ShouldIncludeEntityPropertyForComparison)
                    .OrderBy(x => x)
                    .ToList();

            // ... should be the explicitly excluded values, the id, and the primary key
            var expectedUnchangedPropertyNames =
                excludedPropertyNames
                    .Concat(new[] {"Id", "SchoolId"})
                    .OrderBy(x => x)
                    .ToList();

            Assert.That(actualUnchangedPropertyNames, Is.EqualTo(expectedUnchangedPropertyNames));
        }

        [Then(@"the response model should contain all of the resource model's properties")]
        public void ThenTheResponseModelShouldContainAllOfTheResourceModelsProperties()
        {
            dynamic data = GetDataFromResponse();

            var actualResponsePropertyNames = (List<string>) GetPropertyNames(data);

            var fullResourcePropertyNames =
                typeof (SchoolResource_Full)
                    .GetProperties()
                    .Where(p => p.Name != "ETag")
                    .Select(p => p.GetCustomAttribute<DataMemberAttribute>().Name);

            // Compare the sorted lists for any differences
            actualResponsePropertyNames.OrderBy(p => p)
                .ShouldEqual(fullResourcePropertyNames.OrderBy(p => p));
        }

        [Then(@"every non-primary key value on the entity should be changed")]
        public void ThenEveryNon_PrimaryKeyValueOnTheEntityShouldBeChanged()
        {
            var persistedSchoolEntity = ScenarioContext.Current.Get<SchoolEntity>(ScenarioContextKeys.PersistedEntity);
            var originalSchoolEntity = ScenarioContext.Current.Get<SchoolEntity>(ScenarioContextKeys.OriginalEntity);

            var changedPropertyPaths = new List<string>();

            var comparer = new CompareObjects
            {
                ShowBreadcrumb = true,
                MaxDifferences = int.MaxValue,
                DifferenceCallback = d =>
                {
                    string normalizedPropertyPath = RemoveArrayIndexers(d.PropertyName.TrimStart('.'));
                    changedPropertyPaths.Add(normalizedPropertyPath);
                }
            };

            comparer.Compare(originalSchoolEntity, persistedSchoolEntity);

            var entityPropertyPaths = GetEntityPropertyPaths<SchoolEntity>();

            var actualUnchangedProperties =
                entityPropertyPaths
                    .Except(changedPropertyPaths)
                    .ToList();

            var identifierProperties = new[] {"EducationOrganizationId", "Id", "SchoolId"};

            Assert.That(actualUnchangedProperties, Is.EquivalentTo(identifierProperties));
        }

        [Then(@"the response model's (?:base class )?(.*) collection items should contain the contextual primary key properties of \[(.*)\]")]
        public void ThenTheResponseModelSBaseClassCollectionAItemsShouldContainTheContextualPrimaryKeyProperties(
            string collectionPropertyName, string contextualPrimaryKeyProperties)
        {
            dynamic data = GetDataFromResponse();

            var collection = data[collectionPropertyName];

            if (collection == null)
                Assert.Fail("Collection property '{0}' not found.\r\nModel Data: \r\n{1}", 
                    collectionPropertyName, GetContentFromResponse());

            dynamic firstChildItem = collection[0];

            List<string> actualPropertyNames = GetPropertyNames(firstChildItem);

            var contextualPrimaryKeyPropertyNames = contextualPrimaryKeyProperties
                .Split(',')
                .Select(x => x.Trim())
                .ToList();

            foreach (string propertyName in contextualPrimaryKeyPropertyNames)
                Assert.That(actualPropertyNames, Has.Member(propertyName));

            ScenarioContext.Current.Set(contextualPrimaryKeyPropertyNames.Count, ScenarioContextKeys.ContextualPrimaryKeyPropertyCount);
        }

        [Then(@"the response model's (?:base class )?(.*) collection items should not contain the explicitly excluded properties of \[(.*)\]")]
        public void ThenTheResponseModelsBaseClassCollectionAItemsShouldNotContainTheExplicitlyExcludedProperties(
            string collectionPropertyName, string excludedProperties)
        {
            dynamic data = GetDataFromResponse();

            var baseClassChildCollection = data[collectionPropertyName];

            if (baseClassChildCollection == null)
                Assert.Fail("Base class child collection property '{0}' not found.\r\nModel Data: \r\n{1}",
                    collectionPropertyName, GetContentFromResponse());

            var firstItem = baseClassChildCollection[0];

            List<string> actualPropertyNames = GetPropertyNames(firstItem);

            List<string> excludedPropertyNames = ParseNamesFromCsvText(excludedProperties);

            foreach (string propertyName in excludedPropertyNames)
                Assert.That(actualPropertyNames, Has.No.Member(propertyName));

            ScenarioContext.Current.Set(excludedPropertyNames.Count, ScenarioContextKeys.ExcludedPropertyCount);
        }

        [Then(@"the response model's (?:base class )?(.*) collection items should contain the explicitly included properties of \[(.*)\]")]
        public void ThenTheResponseModelsBaseClassCollectionAItemsShouldContainTheExplicitlyIncludedProperties(
            string collectionPropertyName, string includedProperties)
        {
            dynamic data = GetDataFromResponse();

            var collection = data[collectionPropertyName];

            if (collection == null)
                Assert.Fail("Collection property '{0}' not found.\r\nModel Data: \r\n{1}",
                    collectionPropertyName, GetContentFromResponse());

            dynamic firstItem = collection[0];
            
            List<string> itemPropertyNames = GetPropertyNames(firstItem);

            var includedPropertyNames = ParseNamesFromCsvText(includedProperties);

            foreach (string propertyName in includedPropertyNames)
                Assert.That(itemPropertyNames, Has.Member(propertyName));

            ScenarioContext.Current.Set(includedPropertyNames.Count, ScenarioContextKeys.IncludedPropertyCount);
        }

        [Then(@"the number of properties on the response model's base class (.*) collection items should reflect the number of included properties plus the contextual primary key properties")]
        public void ThenTheNumberOfPropertiesOnTheResponseModelsBaseCollectionAShouldReflectTheNumberOfIncludedPropertiesPlusTheContextualPrimaryKeyProperties(
            string collectionPropertyName)
        {
            dynamic data = GetDataFromResponse();

            var baseClassChildCollection = data[collectionPropertyName];

            if (baseClassChildCollection == null)
                Assert.Fail("Base class child collection property '{0}' not found.\r\nModel Data: \r\n{1}",
                    collectionPropertyName, GetContentFromResponse());

            var firstItem = baseClassChildCollection[0];
            List<string> actualPropertyNames = GetPropertyNames(firstItem);

            AssertPropertyCountReflectIncludedPropertiesPlusContextualPrimaryKeys(actualPropertyNames, false);
        }

        [Then(@"the number of properties on the response model should reflect the number of included properties plus the primary key properties")]
        public void The_number_of_properties_on_the_response_model_should_reflect_the_number_of_included_properties_plus_the_primary_key_properties()
        {
            dynamic data = GetDataFromResponse();

            List<string> actualPropertyNames = GetPropertyNames(data);

            AssertPropertyCountReflectIncludedPropertiesPlusContextualPrimaryKeys(actualPropertyNames, true);
        }

        public void AssertPropertyCountReflectIncludedPropertiesPlusContextualPrimaryKeys(List<string> actualPropertyNames, bool isResourceLevel)
        {
            int contextualPrimaryKeyCount = ScenarioContext.Current.Get<int>(ScenarioContextKeys.ContextualPrimaryKeyPropertyCount);
            int includedPropertyCount = ScenarioContext.Current.Get<int>(ScenarioContextKeys.IncludedPropertyCount);

            int expectedCount = 
                (isResourceLevel ? 1 : 0) 
                + includedPropertyCount 
                + contextualPrimaryKeyCount;

            Assert.That(actualPropertyNames, Has.Count.EqualTo(expectedCount));
        }

        [Then(@"the only values changed on the entity model's (?:base class )?(.*) collection items should be the explicitly included properties of (.*)")]
        public void ThenTheOnlyValuesChangedOnTheEntityModelsBaseClassCollectionItemsShouldBeTheExplicitlyIncludedValues(
            string collectionPropertyName, string includedProperties)
        {
            var persistedEntity = ScenarioContext.Current.Get<object>(ScenarioContextKeys.PersistedEntity);
            var originalEntity = ScenarioContext.Current.Get<object>(ScenarioContextKeys.OriginalEntity);

            var comparer = new CompareObjects { MaxDifferences = 1000 };

            var collectionProperty = persistedEntity.GetType().GetProperty(collectionPropertyName);
            var currentCollection = collectionProperty.GetValue(persistedEntity);
            var originalCollection = collectionProperty.GetValue(originalEntity);

            comparer.Compare(currentCollection, originalCollection);

            var modifiedPropertyNames = comparer.Differences
                .Where(d => IsCollectionDifference(collectionPropertyName, d))
                .Select(x => x.PropertyName.Split('.').Last())
                .Distinct()
                .ToList();

            var includedPropertyNames = ParseNamesFromCsvText(includedProperties);

            Assert.That(modifiedPropertyNames, Is.EquivalentTo(includedPropertyNames));
        }

        [Then(@"the number of properties on the response model's (?:base class )?(.*) collection items should reflect the number of properties on the full (.*) resource model( less the explicitly excluded ones)?")]
        public void ThenTheNumberOfPropertiesOnTheResponseModelsBaseClassCollectionAItemsShouldReflectTheNumberOfPropertiesOnTheModelLessTheExplicitlyExcludedOnes(
            string collectionPropertyName, string resourceModelName, string lessTheExplicitlyExcludedOnes)
        {
            dynamic data = GetDataFromResponse();

            var baseClassChildCollection = data[collectionPropertyName];

            if (baseClassChildCollection == null)
                Assert.Fail("Base class child collection property '{0}' not found.\r\nModel Data: \r\n{1}",
                    collectionPropertyName, GetContentFromResponse());

            var firstItem = baseClassChildCollection[0];
            List<string> actualPropertyNames = GetPropertyNames(firstItem);

            // Use the verification of the method below
            AssertPropertyCountReflectFullModelPropertiesLessExcluded(
                resourceModelName, 
                lessTheExplicitlyExcludedOnes, 
                actualPropertyNames);
        }

        [Then(@"the number of properties on the response model should reflect the number of properties on the full (.*) resource model( less the explicitly excluded ones)?")]
        public void ThenTheNumberOfPropertiesOnTheResponseModelShouldReflectTheNumberOfPropertiesOnTheModelLessTheExplicitlyExcludedOnes(
            string resourceModelName, string lessTheExplicitlyExcludedOnes)
        {
            dynamic data = GetDataFromResponse();

            List<string> actualPropertyNames = GetPropertyNames(data);

            AssertPropertyCountReflectFullModelPropertiesLessExcluded(
                resourceModelName,
                lessTheExplicitlyExcludedOnes,
                actualPropertyNames);
        }

        public void AssertPropertyCountReflectFullModelPropertiesLessExcluded(
            string resourceModelName,
            string lessTheExplicitlyExcludedOnes,
            List<string> actualPropertyNames)
        {
            var fullResourceModelType =
                (from t in typeof(Marker_EdFi_Ods_Api_Models).Assembly.GetTypes()
                 // TODO: Embedded convention - base namespace for resources
                 where t.FullName.StartsWith(typeof(Marker_EdFi_Ods_Api_Models).Namespace + ".Resources.")
                    && t.Name == resourceModelName
                 select t)
                 .Single();

            // Count the serializable properties on the full resource model
            int fullResourceModelPropertyCount = fullResourceModelType
                .GetProperties()
                .Count(p => !IsETag(p.Name) && p.GetCustomAttribute<DataMemberAttribute>() != null);

            bool subtractExcludedProperties = !string.IsNullOrWhiteSpace(lessTheExplicitlyExcludedOnes);

            int excludedPropertyCount = subtractExcludedProperties
                ? ScenarioContext.Current.Get<int>(ScenarioContextKeys.ExcludedPropertyCount)
                : 0;

            Assert.That(actualPropertyNames, Has.Count.EqualTo(fullResourceModelPropertyCount - excludedPropertyCount));
        }

        [Then(@"the only values not changed on the entity model's (?:base class )?(.*) collection items should be the contextual primary key values of \[(.*?)\](?:, and the explicitly excluded properties of \[(.*)\])?")]
        public void ThenTheOnlyValuesNotChangedOnTheEntityModelSBaseClassCollectionItemsShouldBeTheExplicitlyExcludedValuesAndTheContextualPrimaryKeyValues(
            string collectionPropertyName, string contextualPrimaryKeyProperties, string excludedProperties)
        {
            var persistedEntity = ScenarioContext.Current.Get<object>(ScenarioContextKeys.PersistedEntity);
            var originalEntity = ScenarioContext.Current.Get<object>(ScenarioContextKeys.OriginalEntity);

            var comparer = new CompareObjects { MaxDifferences = 1000 };

            var collectionProperty = persistedEntity.GetType().GetProperty(collectionPropertyName);
            var currentCollection = collectionProperty.GetValue(persistedEntity);
            var originalCollection = collectionProperty.GetValue(originalEntity);

            comparer.Compare(currentCollection, originalCollection);

            var modifiedPropertyNames = comparer.Differences
                .Where(d => IsCollectionDifference(collectionPropertyName, d)) //!IsParentReferenceDifference(d, parentReferenceProperty.Name))
                .Select(x => x.PropertyName.Split('.').Last())
                .Distinct()
                .ToList();

            string entityModelName = CompositeTermInflector.MakeSingular(collectionPropertyName);

            var entityModelType = 
                (from t in typeof(Marker_EdFi_Ods_Entities_NHibernate).Assembly.GetTypes()
                 // TODO: Embedded convention - base namespace for resources
                 where t.Name == entityModelName
                 select t)
                 .Single();

            var allEntityProperties = entityModelType
                .GetProperties()
                .Where(p => 
                    p.Name != "CreateDate" // Boilerplate property for identifying transient entities
                    && (p.PropertyType.IsValueType || p.PropertyType == typeof(string)))
                .Select(p => p.Name);
            
            var unmodifiedPropertyNames = 
                allEntityProperties
                .Except(modifiedPropertyNames)
                .ToList();

            string[] expectedUnmodified =
                // Contextal primary key value
                ParseNamesFromCsvText(contextualPrimaryKeyProperties)
                // Explicitly excluded
                .Concat(ParseNamesFromCsvText(excludedProperties))
                .ToArray();

            Assert.That(unmodifiedPropertyNames, Is.EquivalentTo(expectedUnmodified));
        }

        [Then(@"the response should contain the embedded object (.*)")]
        public void ThenTheResponseShouldContainTheEmbeddedObject(string embeddedObjectPropertyName)
        {
            dynamic data = GetDataFromResponse();

            var oneToOneObject = data[embeddedObjectPropertyName];

            Assert.That(oneToOneObject, Is.Not.Null);
        }

        [Then(@"the response should not contain the embedded object (.*)")]
        public void ThenTheResponseShouldNotContainTheEmbeddedObject(string embeddedObjectPropertyName)
        {
            dynamic data = GetDataFromResponse();
            
            var oneToOneObject = data[embeddedObjectPropertyName];

            Assert.That(oneToOneObject, Is.Null);
        }

        [Then(@"the persisted entity model embedded object (.*) should be changed")]
        public void ThenThePersistedEntityModelEmbeddedObjectShouldBeChanged(string embeddedObjectPropertyName)
        {
            StudentLearningStyleEntity persistedLearningStyle, originalLearningStyle;

            GetPersistedAndOriginalLearningStyles(embeddedObjectPropertyName, 
                out persistedLearningStyle, 
                out originalLearningStyle);

            var learningStyleDifferences = CompareLearningStyles(persistedLearningStyle, originalLearningStyle);

            Assert.That(learningStyleDifferences, Is.Not.Empty);
        }

        [Then(@"the persisted entity model embedded object (.*) should not be changed")]
        public void ThenThePersistedEntityModelEmbeddedObjectShouldNotBeChanged(string embeddedObjectPropertyName)
        {
            StudentLearningStyleEntity persistedLearningStyle, originalLearningStyle;

            GetPersistedAndOriginalLearningStyles(embeddedObjectPropertyName,
                out persistedLearningStyle,
                out originalLearningStyle);

            var learningStyleDifferences = CompareLearningStyles(persistedLearningStyle, originalLearningStyle);

            Assert.That(learningStyleDifferences, Is.Empty);
        }

        [Then(@"the response should only contain the included references")]
        public void TheResponseShouldOnlyContainTheIncludedReferences()
        {
            dynamic data = GetDataFromResponse();

            Assert.That(data.BeginCalendarDateReference, Is.Null);
            Assert.That(data.EndCalendarDateReference, Is.Not.Null);
        }

        [Then(@"the persisted entity model should have unmodified values for the excluded reference")]
        public void ThePersistedEntityModelShouldHaveUnmodifiedValuesForTheExcludedReference()
        {
            dynamic persistedEntity = ScenarioContext.Current.Get<object>(ScenarioContextKeys.PersistedEntity);
            dynamic originalEntity = ScenarioContext.Current.Get<object>(ScenarioContextKeys.OriginalEntity);

            Assert.That(persistedEntity.BeginDate, Is.EqualTo(originalEntity.BeginDate));
        }

        private static void GetPersistedAndOriginalLearningStyles(
            string embeddedObjectPropertyName,
            out StudentLearningStyleEntity persistedLearningStyle,
            out StudentLearningStyleEntity originalLearningStyle)
        {
            var persistedEntity = ScenarioContext.Current.Get<object>(ScenarioContextKeys.PersistedEntity);
            var originalEntity = ScenarioContext.Current.Get<object>(ScenarioContextKeys.OriginalEntity);

            var embeddedObjectProperty = persistedEntity.GetType().GetProperty(embeddedObjectPropertyName);

            persistedLearningStyle = (StudentLearningStyleEntity) embeddedObjectProperty.GetValue(persistedEntity);
            originalLearningStyle = (StudentLearningStyleEntity) embeddedObjectProperty.GetValue(originalEntity);
        }

        private static IEnumerable<Difference> CompareLearningStyles(
            StudentLearningStyleEntity persistedLearningStyle,
            StudentLearningStyleEntity originalLearningStyle)
        {
            var comparer = new CompareObjects { MaxDifferences = 1000 };
            comparer.Compare(persistedLearningStyle, originalLearningStyle);

            // Get the differences on the learning style only (don't include the parent reference's differences)
            var learningStyleDifferences = comparer.Differences
                .Where(d => !d.PropertyName.StartsWith(".Student."));

            return learningStyleDifferences;
        }

        private static bool IsCollectionDifference(string collectionPropertyName, Difference difference)
        {
            return difference.PropertyName.Contains(string.Format(".{0}[", collectionPropertyName));
        }

        /// <summary>
        /// Gets a list of all the paths of public properties in the entire object graph for the specified entity type.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to be inspected.</typeparam>
        /// <returns></returns>
        private List<string> GetEntityPropertyPaths<TEntity>()
            where TEntity : IHasIdentifier, IDateVersionedEntity
        {
            var container = FeatureContext.Current.Get<IWindsorContainer>();
            var getEntityById = container.Resolve<IGetEntityById<TEntity>>();

            // Create two completely different entities (to enable us to use the 
            // DataComparer to create a list of all the properties for us)
            var entity1 = getEntityById.GetById(Guid.NewGuid());
            var entity2 = getEntityById.GetById(Guid.NewGuid());

            var entityPropertyPaths = new List<string>();

            var comparer = new CompareObjects
            {
                ShowBreadcrumb = true,
                MaxDifferences = int.MaxValue,
                DifferenceCallback = d =>
                    entityPropertyPaths.Add(RemoveArrayIndexers(d.PropertyName))
            };

            comparer.Compare(entity1, entity2);

            return entityPropertyPaths.Select(p => p.TrimStart('.')).OrderBy(x => x).ToList();
        }

        private string RemoveArrayIndexers(string propertyPath)
        {
            // Strip child collection indexers (e.g. "[5]") from the breadcrumb
            return Regex.Replace(propertyPath, @"(\[[0-9]+\])", string.Empty);
        }

        private bool ShouldIncludeEntityPropertyForComparison(string propertyName)
        {
            switch (propertyName)
            {
                // Internal properties on all entities
                case "CreateDate":
                case "LastModifiedDate":
                // Abstract base class table's PK
                case "EducationOrganizationId":
                    return false;

                default:
                    return true;
            }
        }

        // Note: The School doesn't have any references that are composite keys. If this method is repurposed
        // for such a scenario, an array of strings would be more appropriate as the conversion from a resource's
        // reference to an entity's corresponding properties would not be 1->1.
        private static string ConvertResourcePropertyNameToEntityPropertyName(string resourcePropertyName)
        {
            // Convert resource types and descriptors names to entity names
            if (resourcePropertyName.EndsWith("Type") || resourcePropertyName.EndsWith("Descriptor"))
                return resourcePropertyName + "Id";

            // Handle School references explicitly (pragmatic decision) rather than do the work of reflecting to gather up the properties
            switch (resourcePropertyName)
            {
                case "CharterApprovalSchoolYearTypeReference":
                    return "CharterApprovalSchoolYear";

                case "LocalEducationAgencyReference":
                    return "LocalEducationAgencyId";
            }

            return resourcePropertyName;
        }

        private class ContentMetaData
        {
            public int MemberCount;
            public List<string> Members = new List<string>();
        }

        private static ContentMetaData GetResourceMemberData(Resource resource, ContentTypeUsage contentTypeUsage)
        {
            var contentMetaData = new ContentMetaData();
            switch (contentTypeUsage)
            {
                case ContentTypeUsage.Readable:
                    if (resource.ReadContentType.Property != null)
                    {
                        contentMetaData.MemberCount += resource.ReadContentType.Property.Count();
                        contentMetaData.Members.AddRange(
                            resource.ReadContentType.Property.Select(property => property.name));
                    }
                    if (resource.ReadContentType.Object != null)
                    {
                        UpdateContentMetaData(resource.ReadContentType.Object, contentMetaData);
                    }
                    if (resource.ReadContentType.Collection != null)
                    {
                        UpdateContentMetaData(resource.ReadContentType.Collection, contentMetaData);
                    }
                    break;
                case ContentTypeUsage.Writable:
                    if (resource.WriteContentType.Property != null)
                    {
                        contentMetaData.MemberCount += resource.WriteContentType.Property.Count();
                        contentMetaData.Members.AddRange(
                            resource.WriteContentType.Property.Select(property => property.name));
                    }
                    if (resource.WriteContentType.Object != null)
                    {
                        UpdateContentMetaData(resource.WriteContentType.Object, contentMetaData);
                    }
                    if (resource.WriteContentType.Collection != null)
                    {
                        UpdateContentMetaData(resource.WriteContentType.Collection, contentMetaData);
                    }
                    break;
            }

            return contentMetaData;
        }

        private static void UpdateContentMetaData(ClassDefinition[] collections, ContentMetaData contentMetaData)
        {
            if (collections == null)
            {
                return;
            }

            foreach (ClassDefinition collection in collections)
            {
                contentMetaData.MemberCount++;
                contentMetaData.Members.Add(collection.name);
                if (collection.Property != null)
                {
                    contentMetaData.MemberCount += collection.Property.Count();
                    contentMetaData.Members.AddRange(collection.Property.Select(property => property.name));
                }
                if (collection.Collection != null && collection.Collection.Any())
                {
                    UpdateContentMetaData(collection.Collection, contentMetaData);
                }
            }
        }

        private static ContentType GetReadContentType(string resourceTypeName)
        {
            var profile = ScenarioContext.Current.Get<Profile>();
            var contentType = profile.Resource.Single(x => x.name == resourceTypeName).ReadContentType;
            return contentType;
        }

        private static List<string> GetPropertyNames(dynamic data)
        {
            if (data == null)
                return new List<string>();

            var propertyNames = ((Dictionary<string, object>.KeyCollection) data.GetDynamicMemberNames())
                .Where(p => !IsETag(p)) // Don't ever include boilerplate properties in the count
                .ToList();

            return propertyNames;
        }

        public static dynamic GetDataFromResponse()
        {
            // Check for, and use the data already in the scenario context
            if (ScenarioContext.Current.ContainsKey(ScenarioContextKeys.Data))
                return ScenarioContext.Current.Get<dynamic>(ScenarioContextKeys.Data);

            string content = GetContentFromResponse();
            dynamic data = Json.Decode(content);

            // Save it to the context
            ScenarioContext.Current.Set<dynamic>(data, ScenarioContextKeys.Data);

            // Log debug details containing the properties found on the data object
            if (_logger.IsDebugEnabled)
            {
                var propertyNames = GetPropertyNames(data);
                _logger.DebugFormat("Properties found on resource: {0}", string.Join(", ", propertyNames));
            }

            return data;
        }

        private static string GetContentFromResponse()
        {
            // Deserialize the dynamic data object from the JSON response content
            var httpContent = ScenarioContext.Current.Get<HttpResponseMessage>().Content;
            string content = httpContent.ReadAsStringAsync().Sync();
            return content;
        }

        private static string TrimIdFromDescriptorsAndTypes(string propertyName)
        {
            return (propertyName.EndsWith("TypeId") || propertyName.EndsWith("DescriptorId"))
                ? propertyName.TrimSuffix("Id")
                : propertyName;
        }

        private static List<string> ParseNamesFromCsvText(string csvText)
        {
            return
                csvText.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                       .Select(x => x.Trim())
                       .ToList();
        }

        private static bool IsETag(string propertyName)
        {
            return propertyName.Equals("ETag")
                   || propertyName.Equals("_etag");
        }
    }
}
