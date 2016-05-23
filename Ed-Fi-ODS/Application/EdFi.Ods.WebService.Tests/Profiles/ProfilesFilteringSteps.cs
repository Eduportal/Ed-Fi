// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Castle.Core.Internal;
using Castle.Windsor;
using EdFi.Common.Extensions;
using EdFi.Common.Inflection;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Common.Utils.Profiles;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.WebService.Tests.Owin;
using EdFi.TestObjects;
using Newtonsoft.Json;
using NUnit.Framework;
using TechTalk.SpecFlow;
using SchoolResource_Full = EdFi.Ods.Api.Models.Resources.School.School;
using StudentAssessmentResource_Full = EdFi.Ods.Api.Models.Resources.StudentAssessment.StudentAssessment;

using ScoreResult_Entity = EdFi.Ods.Entities.NHibernate.StudentAssessmentAggregate.StudentAssessmentStudentObjectiveAssessmentScoreResult;
using PerformanceLevel_Entity = EdFi.Ods.Entities.NHibernate.StudentAssessmentAggregate.StudentAssessmentStudentObjectiveAssessmentPerformanceLevel;
using ScoreResult_Full = EdFi.Ods.Api.Models.Resources.StudentAssessment.StudentAssessmentStudentObjectiveAssessmentScoreResult;
using PerformanceLevel_Full = EdFi.Ods.Api.Models.Resources.StudentAssessment.StudentAssessmentStudentObjectiveAssessmentPerformanceLevel;

using SchoolEntity = EdFi.Ods.Entities.NHibernate.SchoolAggregate.School;
using StudentAssessment_Entity = EdFi.Ods.Entities.NHibernate.StudentAssessmentAggregate.StudentAssessment;
using SchoolCategory_Entity = EdFi.Ods.Entities.NHibernate.SchoolAggregate.SchoolCategory;
using SchoolGradeLevel_Entity = EdFi.Ods.Entities.NHibernate.SchoolAggregate.SchoolGradeLevel;
using EducationOrganizationAddress_Entity = EdFi.Ods.Entities.NHibernate.EducationOrganizationAggregate.EducationOrganizationAddress;
using EducationOrganizationInternationalAddress_Entity = EdFi.Ods.Entities.NHibernate.EducationOrganizationAggregate.EducationOrganizationInternationalAddress;
using SchoolCategory_Full = EdFi.Ods.Api.Models.Resources.School.SchoolCategory;
using SchoolGradeLevel_Full = EdFi.Ods.Api.Models.Resources.School.SchoolGradeLevel;
using EducationOrganizationAddress_Full = EdFi.Ods.Api.Models.Resources.EducationOrganization.EducationOrganizationAddress;
using EducationOrganizationInternationalAddress_Full = EdFi.Ods.Api.Models.Resources.EducationOrganization.EducationOrganizationInternationalAddress;

namespace EdFi.Ods.WebService.Tests.Profiles
{
    [Binding]
    public class ProfilesFilteringSteps
    {
        [When(@"a PUT request with a collection containing only (.*) (included|excluded) (Type|Descriptor) values is submitted to (.*) with a request body content type of (?:the appropriate value for the profile in use|""(.*)"")")]
        public void WhenAPUTRequestWithACollectionContainingOnlyConformingIncludedOrExcludedTypeValuesIsSubmittedToSchoolsWithARequestBodyContentTypeOfTheAppropriateValueForTheProfileInUse(
            ConformanceType conformanceType, IncludedOrExcluded includedOrExcluded, string typeOrDescriptor, string resourceCollectionName, string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                contentType = ProfilesContentTypeHelper.CreateContentType(resourceCollectionName, ScenarioContext.Current.Get<string>(ScenarioContextKeys.ProfileName), ContentTypeUsage.Writable);

            var httpClient = FeatureContext.Current.Get<HttpClient>();
            

            httpClient.DefaultRequestHeaders.Clear();
            var container = FeatureContext.Current.Get<IWindsorContainer>();

            HttpContent putRequestContent = null;
            Guid id = Guid.NewGuid();

            switch (resourceCollectionName)
            {
                case "schools":
                    putRequestContent = GetSchoolPutRequestContent(id, contentType, conformanceType, includedOrExcluded, container, httpClient);
                    break;

                case "studentAssessments":
                    putRequestContent = GetStudentAssessmentPutRequestContent(id, contentType, conformanceType, includedOrExcluded, container, httpClient);
                    break;

                default:
                    throw new NotSupportedException();
            }

            // Post resource, using the Profile's content type
            var putResponseMessage = httpClient.PutAsync(OwinUriHelper.BuildApiUri("9999", resourceCollectionName + "/" + id), putRequestContent).Sync();

            ScenarioContext.Current.Set(putResponseMessage);
        }

        private static HttpContent GetSchoolPutRequestContent(
            Guid id,
            string contentType,
            ConformanceType conformanceType,
            IncludedOrExcluded includedOrExcluded,
            IWindsorContainer container,
            HttpClient httpClient)
        {
            // Get the "GetById" repository operation
            var getSchoolEntityById = container.Resolve<IGetEntityById<SchoolEntity>>();

            // Retrieve an "existing" entity
            var schoolEntityForUpdate = getSchoolEntityById.GetById(id);

            // Map the "updated" entity to a full School resource
            var fullSchoolResourceForUpdate = new SchoolResource_Full();
            schoolEntityForUpdate.MapTo(fullSchoolResourceForUpdate, null);

            //empty the id for the Post operation
            fullSchoolResourceForUpdate.Id = Guid.Empty;

            var profileElt = ScenarioContext.Current.Get<XElement>("ProfileXElement");
            string resourceName = "School";
            bool inclusiveFilter = includedOrExcluded == IncludedOrExcluded.Included;

            // Assumption: For simplicity (until otherwise required) we're only supporting AddressType on addresses
            Func<string, bool> filterPropertyNamePredicate = x =>
                x.EqualsIgnoreCase("AddressType")
                || x.EqualsIgnoreCase("CountryDescriptor")
                || x.EqualsIgnoreCase("SchoolCategoryType")
                || x.EqualsIgnoreCase("GradeLevelDescriptor");

            var filteredCollectionElts =
                GetFilteredCollectionElts(
                    profileElt,
                    resourceName,
                    false,
                    filterPropertyNamePredicate,
                    inclusiveFilter).ToList();

            if (!filteredCollectionElts.Any())
                throw new InvalidOperationException(
                    "Assumptions have been made for simplicity of the tests.  No collection filtered on 'AddressType' or 'CountryDescriptor' was found.");

            bool conforming = conformanceType == ConformanceType.Conforming;
            bool valuesAreIncluded = includedOrExcluded == IncludedOrExcluded.Included;

            bool nonConforming = conformanceType == ConformanceType.NonConforming;
            bool valuesAreExcluded = includedOrExcluded == IncludedOrExcluded.Excluded;

            foreach (var filteredCollectionElt in filteredCollectionElts)
            {
                var filterElt = filteredCollectionElt.Element("Filter");
                string filterPropertyName = filterElt.Attribute("propertyName").Value;

                // Get the filter's values
                var filterValues = filterElt
                    .Elements("Value")
                    .Select(x => x.Value)
                    .ToList();

                var testObjectFactory = container.Resolve<ITestObjectFactory>();

                // Defensive programming
                if (!(conforming || nonConforming) || !(valuesAreIncluded || valuesAreExcluded))
                    throw new NotSupportedException(
                        "Both members of conforming/non-conforming or included/excluded have been set to false, indicating a change of mutual exclusivity expectations.");

                if ((valuesAreIncluded && conforming) || (valuesAreExcluded && nonConforming))
                {
                    if (filterPropertyName == "AddressType")
                    {
                        // Remove all items
                        fullSchoolResourceForUpdate.EducationOrganizationAddresses.Clear();
                        schoolEntityForUpdate.EducationOrganizationAddresses.Clear();

                        // Add only conforming/included (or non-conforming excluded) items to the resource
                        filterValues
                            .Select(
                                fv =>
                                {
                                    var item = testObjectFactory.Create<EducationOrganizationAddress_Full>();
                                    item.AddressType = fv;
                                    return item;
                                })
                            .ForEach(a => fullSchoolResourceForUpdate.EducationOrganizationAddresses.Add(a));

                        // Add some values to entity that are disallowed by the filter to ensure they are ignored (rather than processed and deleted) during persistence
                        if (conforming)
                        {
                            filterValues
                                .Select(
                                    fv =>
                                    {
                                        var item = testObjectFactory.Create<EducationOrganizationAddress_Entity>();
                                        item.AddressType = fv + "1111";
                                        return item;
                                    })
                                .ForEach(a => schoolEntityForUpdate.EducationOrganizationAddresses.Add(a));
                        }
                    }
                    else if (filterPropertyName == "CountryDescriptor")
                    {
                        // Remove all items
                        fullSchoolResourceForUpdate.EducationOrganizationInternationalAddresses.Clear();
                        schoolEntityForUpdate.EducationOrganizationInternationalAddresses.Clear();

                        // Add only conforming (included) items
                        filterValues
                            .Select(
                                fv =>
                                {
                                    var item =
                                        testObjectFactory.Create<EducationOrganizationInternationalAddress_Full>();
                                    item.CountryDescriptor = fv;
                                    return item;
                                })
                            .ForEach(
                                a => fullSchoolResourceForUpdate.EducationOrganizationInternationalAddresses.Add(a));

                        // Add some values to entity that are disallowed by the filter to ensure they are ignored (rather than processed and deleted) during persistence
                        if (conforming)
                        {
                            filterValues
                                .Select(
                                    fv =>
                                    {
                                        var item =
                                            testObjectFactory.Create<EducationOrganizationInternationalAddress_Entity>
                                                ();
                                        item.CountryDescriptor = fv + "1111";
                                        return item;
                                    })
                                .ForEach(
                                    a => schoolEntityForUpdate.EducationOrganizationInternationalAddresses.Add(a));
                        }
                    }
                    else if (filterPropertyName == "SchoolCategoryType")
                    {
                        // Remove all items
                        fullSchoolResourceForUpdate.SchoolCategories.Clear();
                        schoolEntityForUpdate.SchoolCategories.Clear();

                        // Add only conforming/included (or non-conforming excluded) item to the resource
                        filterValues
                            .Select(
                                fv =>
                                {
                                    var item = testObjectFactory.Create<SchoolCategory_Full>();
                                    item.SchoolCategoryType = fv;
                                    return item;
                                })
                            .ForEach(a => fullSchoolResourceForUpdate.SchoolCategories.Add(a));

                        // Add some values to entity that are disallowed by the filter to ensure they are ignored (rather than processed and deleted) during persistence
                        if (conforming)
                        {
                            filterValues
                                .Select(
                                    fv =>
                                    {
                                        var item = testObjectFactory.Create<SchoolCategory_Entity>();
                                        item.SchoolCategoryType = fv + "1111";
                                        return item;
                                    })
                                .ForEach(a => schoolEntityForUpdate.SchoolCategories.Add(a));
                        }
                    }
                    else if (filterPropertyName == "GradeLevelDescriptor")
                    {
                        // Remove all items
                        fullSchoolResourceForUpdate.SchoolGradeLevels.Clear();
                        schoolEntityForUpdate.SchoolGradeLevels.Clear();

                        // Add only conforming (included) items
                        filterValues
                            .Select(
                                fv =>
                                {
                                    var item = testObjectFactory.Create<SchoolGradeLevel_Full>();
                                    item.GradeLevelDescriptor = fv;
                                    return item;
                                })
                            .ForEach(a => fullSchoolResourceForUpdate.SchoolGradeLevels.Add(a));

                        // Add some values to entity that are disallowed by the filter to ensure they are ignored (rather than processed and deleted) during persistence
                        if (conforming)
                        {
                            filterValues
                                .Select(
                                    fv =>
                                    {
                                        var item = testObjectFactory.Create<SchoolGradeLevel_Entity>();
                                        item.GradeLevelDescriptor = fv + "1111";
                                        return item;
                                    })
                                .ForEach(a => schoolEntityForUpdate.SchoolGradeLevels.Add(a));
                        }
                    }
                }
                else if ((valuesAreExcluded && conforming) || (valuesAreIncluded && nonConforming))
                {
                    if (filterPropertyName == "AddressType")
                    {
                        // Remove all items
                        fullSchoolResourceForUpdate.EducationOrganizationAddresses.Clear();
                        schoolEntityForUpdate.EducationOrganizationAddresses.Clear();

                        // Add only non-conforming (not included) items
                        filterValues
                            .Select(
                                fv =>
                                {
                                    var item = testObjectFactory.Create<EducationOrganizationAddress_Full>();
                                    item.AddressType = fv + "9999";
                                    return item;
                                })
                            .ForEach(a => fullSchoolResourceForUpdate.EducationOrganizationAddresses.Add(a));

                        // Add some values to entity that are disallowed by the filter to ensure they are ignored (rather than processed and deleted) during persistence
                        if (conforming)
                        {
                            filterValues
                                .Select(
                                    fv =>
                                    {
                                        var item = testObjectFactory.Create<EducationOrganizationAddress_Entity>();
                                        item.AddressType = fv;
                                        return item;
                                    })
                                .ForEach(a => schoolEntityForUpdate.EducationOrganizationAddresses.Add(a));
                        }
                    }
                    else if (filterPropertyName == "CountryDescriptor")
                    {
                        // Remove all items
                        fullSchoolResourceForUpdate.EducationOrganizationInternationalAddresses.Clear();
                        schoolEntityForUpdate.EducationOrganizationInternationalAddresses.Clear();

                        // Add only conforming (included) items
                        filterValues
                            .Select(
                                v =>
                                {
                                    var item =
                                        testObjectFactory.Create<EducationOrganizationInternationalAddress_Full>();
                                    item.CountryDescriptor = v + "9999";
                                    return item;
                                })
                            .ForEach(
                                a => fullSchoolResourceForUpdate.EducationOrganizationInternationalAddresses.Add(a));

                        // Add some values to entity that are disallowed by the filter to ensure they are ignored (rather than processed and deleted) during persistence
                        if (conforming)
                        {
                            filterValues
                                .Select(
                                    fv =>
                                    {
                                        var item =
                                            testObjectFactory.Create<EducationOrganizationInternationalAddress_Entity>
                                                ();
                                        item.CountryDescriptor = fv;
                                        return item;
                                    })
                                .ForEach(
                                    a => schoolEntityForUpdate.EducationOrganizationInternationalAddresses.Add(a));
                        }
                    }
                    else if (filterPropertyName == "SchoolCategoryType")
                    {
                        // Remove all items
                        fullSchoolResourceForUpdate.SchoolCategories.Clear();
                        schoolEntityForUpdate.SchoolCategories.Clear();

                        // Add only non-conforming (not included) items
                        filterValues
                            .Select(
                                fv =>
                                {
                                    var item = testObjectFactory.Create<SchoolCategory_Full>();
                                    item.SchoolCategoryType = fv + "9999";
                                    return item;
                                })
                            .ForEach(a => fullSchoolResourceForUpdate.SchoolCategories.Add(a));

                        // Add some values to entity that are disallowed by the filter to ensure they are ignored (rather than processed and deleted) during persistence
                        if (conforming)
                        {
                            filterValues
                                .Select(
                                    fv =>
                                    {
                                        var item = testObjectFactory.Create<SchoolCategory_Entity>();
                                        item.SchoolCategoryType = fv;
                                        return item;
                                    })
                                .ForEach(a => schoolEntityForUpdate.SchoolCategories.Add(a));
                        }
                    }
                    else if (filterPropertyName == "GradeLevelDescriptor")
                    {
                        // Remove all items
                        fullSchoolResourceForUpdate.SchoolGradeLevels.Clear();
                        schoolEntityForUpdate.SchoolGradeLevels.Clear();

                        // Add only conforming (included) items
                        filterValues
                            .Select(
                                v =>
                                {
                                    var item = testObjectFactory.Create<SchoolGradeLevel_Full>();
                                    item.GradeLevelDescriptor = v + "9999";
                                    return item;
                                })
                            .ForEach(a => fullSchoolResourceForUpdate.SchoolGradeLevels.Add(a));

                        // Add some values to entity that are disallowed by the filter to ensure they are ignored (rather than processed and deleted) during persistence
                        if (conforming)
                        {
                            filterValues
                                .Select(
                                    fv =>
                                    {
                                        var item = testObjectFactory.Create<SchoolGradeLevel_Entity>();
                                        item.GradeLevelDescriptor = fv;
                                        return item;
                                    })
                                .ForEach(a => schoolEntityForUpdate.SchoolGradeLevels.Add(a));
                        }
                    }
                }
            }

            if (conforming)
            {
                // Make a copy of the entity for subsequent comparison
                var originalSchoolEntity = new SchoolEntity();
                schoolEntityForUpdate.MapTo(originalSchoolEntity, null);

                // Save into the ScenarioContext the targeted entity and the prepared school resource for 
                // subsequent inspection to ensure values are synched to entity correctly, and existing 
                // values disallowed by the filter are intact on the entity.
                ScenarioContext.Current.Set(
                    ((FakeRepository<SchoolEntity>) getSchoolEntityById).EntitiesById[id],
                    "currentSchoolEntity");
                ScenarioContext.Current.Set(originalSchoolEntity, "originalSchoolEntity");
                ScenarioContext.Current.Set(fullSchoolResourceForUpdate, "schoolResourceForUpdate");
            }

            // Modify the client to specify that we're sending a profile-specific resource
            httpClient.DefaultRequestHeaders.Add(HttpRequestHeader.ContentType.ToString(), contentType);

            //build content
            HttpContent putRequestContent = new StringContent(
                JsonConvert.SerializeObject(fullSchoolResourceForUpdate),
                Encoding.UTF8,
                contentType);
            
            return putRequestContent;
        }

        private static HttpContent GetStudentAssessmentPutRequestContent(
            Guid id,
            string contentType,
            ConformanceType conformanceType,
            IncludedOrExcluded includedOrExcluded,
            IWindsorContainer container,
            HttpClient httpClient)
        {
            // Get the "GetById" repository operation
            var getStudentAssessmentEntityById = container.Resolve<IGetEntityById<StudentAssessment_Entity>>();

            // Retrieve an "existing" entity
            var studentAssessmentForUpdate = getStudentAssessmentEntityById.GetById(id);

            // Map the "updated" entity to a full School resource
            var fullStudentAssessmentResourceForUpdate = new StudentAssessmentResource_Full();
            studentAssessmentForUpdate.MapTo(fullStudentAssessmentResourceForUpdate, null);

            //empty the id for the Post operation
            fullStudentAssessmentResourceForUpdate.Id = Guid.Empty;

            var profileElt = ScenarioContext.Current.Get<XElement>("ProfileXElement");
            string resourceName = "StudentAssessment";
            bool inclusiveFilter = includedOrExcluded == IncludedOrExcluded.Included;

            // Assumption: For simplicity (until otherwise required) we're only supporting specific properties
            Func<string, bool> filterPropertyNamePredicate = x =>
                x.EqualsIgnoreCase("AssessmentReportingMethodType")
                || x.EqualsIgnoreCase("PerformanceLevelDescriptor");

            var filteredCollectionElts =
                GetFilteredCollectionElts(
                    profileElt,
                    resourceName,
                    false,
                    filterPropertyNamePredicate,
                    inclusiveFilter).ToList();

            if (!filteredCollectionElts.Any())
                throw new InvalidOperationException(
                    "Assumptions have been made for simplicity of the tests.  No collection filtered on 'AssessmentReportingMethodType' or 'PerformanceLevelDescriptor' was found.");

            bool conforming = conformanceType == ConformanceType.Conforming;
            bool valuesAreIncluded = includedOrExcluded == IncludedOrExcluded.Included;

            bool nonConforming = conformanceType == ConformanceType.NonConforming;
            bool valuesAreExcluded = includedOrExcluded == IncludedOrExcluded.Excluded;

            foreach (var filteredCollectionElt in filteredCollectionElts)
            {
                var filterElt = filteredCollectionElt.Element("Filter");
                string filterPropertyName = filterElt.Attribute("propertyName").Value;

                // Get the filter's values
                var filterValues = filterElt
                    .Elements("Value")
                    .Select(x => x.Value)
                    .ToList();

                var testObjectFactory = container.Resolve<ITestObjectFactory>();

                // Defensive programming
                if (!(conforming || nonConforming) || !(valuesAreIncluded || valuesAreExcluded))
                    throw new NotSupportedException(
                        "Both members of conforming/non-conforming or included/excluded have been set to false, indicating a change of mutual exclusivity expectations.");

                if ((valuesAreIncluded && conforming) || (valuesAreExcluded && nonConforming))
                {
                    if (filterPropertyName == "AssessmentReportingMethodType")
                    {
                        // Remove all items
                        fullStudentAssessmentResourceForUpdate.StudentAssessmentStudentObjectiveAssessments.ForEach(x => x.StudentAssessmentStudentObjectiveAssessmentScoreResults.Clear());
                        studentAssessmentForUpdate.StudentAssessmentStudentObjectiveAssessments.ForEach(x => x.StudentAssessmentStudentObjectiveAssessmentScoreResults.Clear());

                        // Add only conforming/included (or non-conforming excluded) items to the resource
                        filterValues
                            .Select(
                                fv =>
                                {
                                    var item = testObjectFactory.Create<ScoreResult_Full>();
                                    item.AssessmentReportingMethodType = fv;
                                    return item;
                                })
                            .ForEach(a => fullStudentAssessmentResourceForUpdate.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentScoreResults.Add(a));

                        // Add some values to entity that are disallowed by the filter to ensure they are ignored (rather than processed and deleted) during persistence
                        if (conforming)
                        {
                            filterValues
                                .Select(
                                    fv =>
                                    {
                                        var item = testObjectFactory.Create<ScoreResult_Entity>();
                                        item.AssessmentReportingMethodType = fv + "1111";
                                        return item;
                                    })
                                .ForEach(a => studentAssessmentForUpdate.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentScoreResults.Add(a));
                        }
                    }
                    else if (filterPropertyName == "PerformanceLevelDescriptor")
                    {
                        // Remove all items
                        fullStudentAssessmentResourceForUpdate.StudentAssessmentStudentObjectiveAssessments.ForEach(x => x.StudentAssessmentStudentObjectiveAssessmentPerformanceLevels.Clear());
                        studentAssessmentForUpdate.StudentAssessmentStudentObjectiveAssessments.ForEach(x => x.StudentAssessmentStudentObjectiveAssessmentPerformanceLevels.Clear());

                        // Add only conforming (included) items
                        filterValues
                            .Select(
                                fv =>
                                {
                                    var item =
                                        testObjectFactory.Create<PerformanceLevel_Full>();
                                    item.PerformanceLevelDescriptor = fv;
                                    return item;
                                })
                            .ForEach(
                                a => fullStudentAssessmentResourceForUpdate.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentPerformanceLevels.Add(a));

                        // Add some values to entity that are disallowed by the filter to ensure they are ignored (rather than processed and deleted) during persistence
                        if (conforming)
                        {
                            filterValues
                                .Select(
                                    fv =>
                                    {
                                        var item =
                                            testObjectFactory.Create<PerformanceLevel_Entity>
                                                ();
                                        item.PerformanceLevelDescriptor = fv + "1111";
                                        return item;
                                    })
                                .ForEach(
                                    a => studentAssessmentForUpdate.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentPerformanceLevels.Add(a));
                        }
                    }
                }
                else if ((valuesAreExcluded && conforming) || (valuesAreIncluded && nonConforming))
                {
                    if (filterPropertyName == "AssessmentReportingMethodType")
                    {
                        // Remove all items
                        fullStudentAssessmentResourceForUpdate.StudentAssessmentStudentObjectiveAssessments.ForEach(x => x.StudentAssessmentStudentObjectiveAssessmentScoreResults.Clear());
                        studentAssessmentForUpdate.StudentAssessmentStudentObjectiveAssessments.ForEach(x => x.StudentAssessmentStudentObjectiveAssessmentScoreResults.Clear());

                        // Add only non-conforming (not included) items
                        filterValues
                            .Select(
                                fv =>
                                {
                                    var item = testObjectFactory.Create<ScoreResult_Full>();
                                    item.AssessmentReportingMethodType = fv + "9999";
                                    return item;
                                })
                            .ForEach(a => fullStudentAssessmentResourceForUpdate.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentScoreResults.Add(a));

                        // Add some values to entity that are disallowed by the filter to ensure they are ignored (rather than processed and deleted) during persistence
                        if (conforming)
                        {
                            filterValues
                                .Select(
                                    fv =>
                                    {
                                        var item = testObjectFactory.Create<ScoreResult_Entity>();
                                        item.AssessmentReportingMethodType = fv;
                                        return item;
                                    })
                                .ForEach(a => studentAssessmentForUpdate.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentScoreResults.Add(a));
                        }
                    }
                    else if (filterPropertyName == "PerformanceLevelDescriptor")
                    {
                        // Remove all items
                        fullStudentAssessmentResourceForUpdate.StudentAssessmentStudentObjectiveAssessments.ForEach(x => x.StudentAssessmentStudentObjectiveAssessmentPerformanceLevels.Clear());
                        studentAssessmentForUpdate.StudentAssessmentStudentObjectiveAssessments.ForEach(x => x.StudentAssessmentStudentObjectiveAssessmentPerformanceLevels.Clear());

                        // Add only conforming (included) items
                        filterValues
                            .Select(
                                v =>
                                {
                                    var item =
                                        testObjectFactory.Create<PerformanceLevel_Full>();
                                    item.PerformanceLevelDescriptor = v + "9999";
                                    return item;
                                })
                            .ForEach(
                                a => fullStudentAssessmentResourceForUpdate.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentPerformanceLevels.Add(a));

                        // Add some values to entity that are disallowed by the filter to ensure they are ignored (rather than processed and deleted) during persistence
                        if (conforming)
                        {
                            filterValues
                                .Select(
                                    fv =>
                                    {
                                        var item =
                                            testObjectFactory.Create<PerformanceLevel_Entity>
                                                ();
                                        item.PerformanceLevelDescriptor = fv;
                                        return item;
                                    })
                                .ForEach(
                                    a => studentAssessmentForUpdate.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentPerformanceLevels.Add(a));
                        }
                    }
                }
            }

            if (conforming)
            {
                // Make a copy of the entity for subsequent comparison
                var originalStudentAssessmentEntity = new StudentAssessment_Entity();
                studentAssessmentForUpdate.MapTo(originalStudentAssessmentEntity, null);

                // Save into the ScenarioContext the targeted entity and the prepared school resource for 
                // subsequent inspection to ensure values are synched to entity correctly, and existing 
                // values disallowed by the filter are intact on the entity.
                ScenarioContext.Current.Set(
                    ((FakeRepository<StudentAssessment_Entity>) getStudentAssessmentEntityById).EntitiesById[id],
                    "currentStudentAssessmentEntity");
                ScenarioContext.Current.Set(originalStudentAssessmentEntity, "originalStudentAssessmentEntity");
                ScenarioContext.Current.Set(fullStudentAssessmentResourceForUpdate, "studentAssessmentResourceForUpdate");
            }

            // Modify the client to specify that we're sending a profile-specific resource
            httpClient.DefaultRequestHeaders.Add(HttpRequestHeader.ContentType.ToString(), contentType);

            //build content
            HttpContent putRequestContent = new StringContent(
                JsonConvert.SerializeObject(fullStudentAssessmentResourceForUpdate),
                Encoding.UTF8,
                contentType);
            
            return putRequestContent;
        }

        [Then(@"the submitted (Type|Descriptor) values should be persisted to the (.*)")]
        public void ThenTheSubmittedValuesShouldBePersisted(string typeOrDescriptor, string resourceModelNameText)
        {
            string resourceModelName = resourceModelNameText.ToMixedCase();

            switch (resourceModelName)
            {
                case "School":
                    var currentSchool = ScenarioContext.Current.Get<SchoolEntity>("currentSchoolEntity");
                    var originalSchool = ScenarioContext.Current.Get<SchoolEntity>("originalSchoolEntity");
                    var schoolResource = ScenarioContext.Current.Get<SchoolResource_Full>("schoolResourceForUpdate");

                    if (typeOrDescriptor == "Type")
                    {
                        // Inherited collection filtered on Type
                        // --------------------------------------
                        var submittedResourceAddressTypes = schoolResource.EducationOrganizationAddresses.Select(x => x.AddressType);
                        var currentEntityAddressTypes = currentSchool.EducationOrganizationAddresses.Select(x => x.AddressType);
                        var originalEntityAddressTypes = originalSchool.EducationOrganizationAddresses.Select(x => x.AddressType);

                        // Applied addressTypes is the difference between the current (saved) types and the original state
                        var appliedEntityAddressTypes = currentEntityAddressTypes.Except(originalEntityAddressTypes);

                        // Make sure all the addresses in the resource were synched to the entity.
                        Assert.That(submittedResourceAddressTypes, Is.EquivalentTo(appliedEntityAddressTypes));

                        // Concrete collection filtered on Type
                        // --------------------------------------
                        var submittedResourceSchoolCategoryTypes = schoolResource.SchoolCategories.Select(x => x.SchoolCategoryType);
                        var currentEntitySchoolCategoryTypes = currentSchool.SchoolCategories.Select(x => x.SchoolCategoryType);
                        var originalEntitySchoolCategoryTypes = originalSchool.SchoolCategories.Select(x => x.SchoolCategoryType);

                        // Applied SchoolCategoryTypes is the difference between the current (saved) types and the original state
                        var appliedEntitySchoolCategoryTypes = currentEntitySchoolCategoryTypes.Except(originalEntitySchoolCategoryTypes);

                        // Make sure all the addresses in the resource were synched to the entity.
                        Assert.That(submittedResourceSchoolCategoryTypes, Is.EquivalentTo(appliedEntitySchoolCategoryTypes));
                    }
                    else if (typeOrDescriptor == "Descriptor")
                    {
                        // Inherited collection filtered on Descriptor
                        // --------------------------------------------
                        var submittedResourceCountryDescriptors = schoolResource.EducationOrganizationInternationalAddresses.Select(x => x.CountryDescriptor);
                        var currentEntityCountryDescriptors = currentSchool.EducationOrganizationInternationalAddresses.Select(x => x.CountryDescriptor);
                        var originalEntityCountryDescriptors = originalSchool.EducationOrganizationInternationalAddresses.Select(x => x.CountryDescriptor);

                        // Applied CountryDescriptors is the difference between the current (saved) descriptors and the original state
                        var appliedEntityCountryDescriptors = currentEntityCountryDescriptors.Except(originalEntityCountryDescriptors);

                        // Make sure all the InternationalAddresses in the resource were synched to the entity.
                        Assert.That(submittedResourceCountryDescriptors, Is.EquivalentTo(appliedEntityCountryDescriptors));

                        // Concrete collection filtered on Descriptor
                        // --------------------------------------------
                        var submittedResourceGradeLevelDescriptors = schoolResource.SchoolGradeLevels.Select(x => x.GradeLevelDescriptor);
                        var currentEntityGradeLevelDescriptors = currentSchool.SchoolGradeLevels.Select(x => x.GradeLevelDescriptor);
                        var originalEntityGradeLevelDescriptors = originalSchool.SchoolGradeLevels.Select(x => x.GradeLevelDescriptor);

                        // Applied GradeLevelDescriptors is the difference between the current (saved) descriptors and the original state
                        var appliedEntityGradeLevelDescriptors = currentEntityGradeLevelDescriptors.Except(originalEntityGradeLevelDescriptors);

                        // Make sure all the InternationalAddresses in the resource were synched to the entity.
                        Assert.That(submittedResourceGradeLevelDescriptors, Is.EquivalentTo(appliedEntityGradeLevelDescriptors));
                    }
                    break;

                case "StudentAssessment":
                    var currentStudentAssessment = ScenarioContext.Current.Get<StudentAssessment_Entity>("currentStudentAssessmentEntity");
                    var originalStudentAssessment = ScenarioContext.Current.Get<StudentAssessment_Entity>("originalStudentAssessmentEntity");
                    var studentAssessmentResource = ScenarioContext.Current.Get<StudentAssessmentResource_Full>("studentAssessmentResourceForUpdate");

                    if (typeOrDescriptor == "Type")
                    {
                        // Concrete collection filtered on Type
                        // --------------------------------------
                        var submittedResourceReportingMethodTypes = studentAssessmentResource.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentScoreResults.Select(x => x.AssessmentReportingMethodType);
                        var currentEntityReportingMethodTypes = currentStudentAssessment.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentScoreResults.Select(x => x.AssessmentReportingMethodType);
                        var originalEntityReportingMethodTypes = originalStudentAssessment.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentScoreResults.Select(x => x.AssessmentReportingMethodType);

                        // Applied SchoolCategoryTypes is the difference between the current (saved) types and the original state
                        var appliedEntityReportingMethodTypes = currentEntityReportingMethodTypes.Except(originalEntityReportingMethodTypes);

                        // Make sure all the addresses in the resource were synched to the entity.
                        Assert.That(submittedResourceReportingMethodTypes, Is.EquivalentTo(appliedEntityReportingMethodTypes));
                    }
                    else if (typeOrDescriptor == "Descriptor")
                    {
                        // Concrete collection filtered on Descriptor
                        // --------------------------------------------
                        var submittedResourcePerformanceLevelDescriptors = studentAssessmentResource.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentPerformanceLevels.Select(x => x.PerformanceLevelDescriptor);
                        var currentEntityPerformanceLevelDescriptors = currentStudentAssessment.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentPerformanceLevels.Select(x => x.PerformanceLevelDescriptor);
                        var originalEntityPerformanceLevelDescriptors = originalStudentAssessment.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentPerformanceLevels.Select(x => x.PerformanceLevelDescriptor);

                        // Applied GradeLevelDescriptors is the difference between the current (saved) descriptors and the original state
                        var appliedEntityPerformanceLevelDescriptors = currentEntityPerformanceLevelDescriptors.Except(originalEntityPerformanceLevelDescriptors);

                        // Make sure all the InternationalAddresses in the resource were synched to the entity.
                        Assert.That(submittedResourcePerformanceLevelDescriptors, Is.EquivalentTo(appliedEntityPerformanceLevelDescriptors));
                    }
                    break;

                default:
                    throw new Exception(
                        string.Format(
                            "Unsupported resource model '{0}' encountered in Type/Descriptor verification step for persisted values on filtered child collections.",
                            resourceModelName));
            }

        }

        [Then(@"the pre-existing (Type|Descriptor) values should be intact on the (.*)")]
        public void ThenTheExistingValuesShouldBeIntact(string typeOrDescriptor, string resourceModelNameText)
        {
            string resourceModelName = resourceModelNameText.ToMixedCase();

            switch (resourceModelName)
            {
                case "School":
                    var currentSchool = ScenarioContext.Current.Get<SchoolEntity>("currentSchoolEntity");
                    var originalSchool = ScenarioContext.Current.Get<SchoolEntity>("originalSchoolEntity");
                    var schoolResource = ScenarioContext.Current.Get<SchoolResource_Full>("schoolResourceForUpdate");

                    if (typeOrDescriptor == "Type")
                    {
                        // Inherited collection filtered on Type
                        // --------------------------------------
                        var submittedResourceAddressTypes = schoolResource.EducationOrganizationAddresses.Select(x => x.AddressType);
                        var currentEntityAddressTypes = currentSchool.EducationOrganizationAddresses.Select(x => x.AddressType);
                        var originalEntityAddressTypes = originalSchool.EducationOrganizationAddresses.Select(x => x.AddressType);

                        // Intact addressTypes is the difference between the current (saved) types and the submitted resource
                        var intactEntityAddressTypes = currentEntityAddressTypes.Except(submittedResourceAddressTypes);

                        // Make sure all the addresses in the original entity are still intact
                        Assert.That(intactEntityAddressTypes, Is.EquivalentTo(originalEntityAddressTypes));

                        // Concrete collection filtered on Type
                        // --------------------------------------
                        var submittedResourceCategoryTypes = schoolResource.SchoolCategories.Select(x => x.SchoolCategoryType);
                        var currentEntityCategoryTypes = currentSchool.SchoolCategories.Select(x => x.SchoolCategoryType);
                        var originalEntityCategoryTypes = originalSchool.SchoolCategories.Select(x => x.SchoolCategoryType);

                        // Intact addressTypes is the difference between the current (saved) types and the submitted resource
                        var intactEntityCategoryTypes = currentEntityCategoryTypes.Except(submittedResourceCategoryTypes);

                        // Make sure all the addresses in the original entity are still intact
                        Assert.That(intactEntityCategoryTypes, Is.EquivalentTo(originalEntityCategoryTypes));
                    }
                    else if (typeOrDescriptor == "Descriptor")
                    {
                        // Inherited collection filtered on Descriptor
                        // --------------------------------------------
                        var submittedResourceCountryDescriptors = schoolResource.EducationOrganizationInternationalAddresses.Select(x => x.CountryDescriptor);
                        var currentEntityCountryDescriptors = currentSchool.EducationOrganizationInternationalAddresses.Select(x => x.CountryDescriptor);
                        var originalEntityCountryDescriptors = originalSchool.EducationOrganizationInternationalAddresses.Select(x => x.CountryDescriptor);

                        // Intact CountryDescriptors is the difference between the current (saved) descriptors and the submitted resource
                        var intactEntityCountryDescriptors = currentEntityCountryDescriptors.Except(submittedResourceCountryDescriptors);

                        // Make sure all the InternationalAddresses in the original entity are still intact
                        Assert.That(intactEntityCountryDescriptors, Is.EquivalentTo(originalEntityCountryDescriptors));

                        // Concrete collection filtered on Descriptor
                        // --------------------------------------------
                        var submittedResourceGradeLevelDescriptors = schoolResource.SchoolGradeLevels.Select(x => x.GradeLevelDescriptor);
                        var currentEntityGradeLevelDescriptors = currentSchool.SchoolGradeLevels.Select(x => x.GradeLevelDescriptor);
                        var originalEntityGradeLevelDescriptors = originalSchool.SchoolGradeLevels.Select(x => x.GradeLevelDescriptor);

                        // Intact GradeLevelDescriptors is the difference between the current (saved) descriptors and the submitted resource
                        var intactEntityGradeLevelDescriptors = currentEntityGradeLevelDescriptors.Except(submittedResourceGradeLevelDescriptors);

                        // Make sure all the InternationalAddresses in the original entity are still intact
                        Assert.That(intactEntityGradeLevelDescriptors, Is.EquivalentTo(originalEntityGradeLevelDescriptors));
                    }
                    break;

                case "StudentAssessment":
                    var currentStudentAssessment = ScenarioContext.Current.Get<StudentAssessment_Entity>("currentStudentAssessmentEntity");
                    var originalStudentAssessment = ScenarioContext.Current.Get<StudentAssessment_Entity>("originalStudentAssessmentEntity");
                    var studentAssessmentResource = ScenarioContext.Current.Get<StudentAssessmentResource_Full>("studentAssessmentResourceForUpdate");

                    if (typeOrDescriptor == "Type")
                    {
                        // Concrete collection filtered on Type
                        // --------------------------------------
                        var submittedReportingMethodTypes = studentAssessmentResource.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentScoreResults.Select(x => x.AssessmentReportingMethodType);
                        var currentEntityReportingMethodTypes = currentStudentAssessment.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentScoreResults.Select(x => x.AssessmentReportingMethodType);
                        var originalEntityReportingMethodTypes = originalStudentAssessment.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentScoreResults.Select(x => x.AssessmentReportingMethodType);

                        // Intact addressTypes is the difference between the current (saved) types and the submitted resource
                        var intactEntityReportingMethodTypes = currentEntityReportingMethodTypes.Except(submittedReportingMethodTypes);

                        // Make sure all the addresses in the original entity are still intact
                        Assert.That(intactEntityReportingMethodTypes, Is.EquivalentTo(originalEntityReportingMethodTypes));
                    }
                    else if (typeOrDescriptor == "Descriptor")
                    {
                        // Concrete collection filtered on Descriptor
                        // --------------------------------------------
                        var submittedResourcePerformanceLevelDescriptors = studentAssessmentResource.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentPerformanceLevels.Select(x => x.PerformanceLevelDescriptor);
                        var currentEntityPerformanceLevelDescriptors = currentStudentAssessment.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentPerformanceLevels.Select(x => x.PerformanceLevelDescriptor);
                        var originalEntityPerformanceLevelDescriptors = originalStudentAssessment.StudentAssessmentStudentObjectiveAssessments.First().StudentAssessmentStudentObjectiveAssessmentPerformanceLevels.Select(x => x.PerformanceLevelDescriptor);

                        // Intact GradeLevelDescriptors is the difference between the current (saved) descriptors and the submitted resource
                        var intactEntityPerformanceLevelDescriptors = currentEntityPerformanceLevelDescriptors.Except(submittedResourcePerformanceLevelDescriptors);

                        // Make sure all the InternationalAddresses in the original entity are still intact
                        Assert.That(intactEntityPerformanceLevelDescriptors, Is.EquivalentTo(originalEntityPerformanceLevelDescriptors));
                    }

                    break;

                default:
                    throw new Exception(
                        string.Format(
                            "Unsupported resource model '{0}' encountered in Type/Descriptor verification step for intact values on filtered child collections.",
                            resourceModelName));
            }
        }

        [Then(@"the response model's collection items should only contain items matching the included (Type|Descriptor) values")]
        public void ThenTheResponseModelSCollectionItemsShouldOnlyContainItemsMatchingTheIncludedTypeValues(string typeOrDescriptor)
        {
            dynamic data = ProfilesSteps.GetDataFromResponse();
            var profileElt = ScenarioContext.Current.Get<XElement>("ProfileXElement");

            Func<string, bool> predicate = s => s.EndsWithIgnoreCase(typeOrDescriptor);

            string resourceModelName = ScenarioContext.Current.Get<string>(ScenarioContextKeys.ResourceModelName);
            ValidateFilteringOnResponseChildCollections(resourceModelName, data, profileElt, predicate, inclusiveFilters: true);
        }

        [Then(@"the response model's collection items should not contain items matching the excluded (Type|Descriptor) values")]
        public void ThenTheResponseModelSCollectionItemsShouldNotContainItemsMatchingTheExcludedTypeValues(string typeOrDescriptor)
        {
            dynamic data = ProfilesSteps.GetDataFromResponse();
            var profileElt = ScenarioContext.Current.Get<XElement>("ProfileXElement");

            Func<string, bool> predicate = s => s.EndsWithIgnoreCase(typeOrDescriptor);

            string resourceModelName = ScenarioContext.Current.Get<string>(ScenarioContextKeys.ResourceModelName);
            ValidateFilteringOnResponseChildCollections(resourceModelName, data, profileElt, predicate, inclusiveFilters: false);
        }

        private static void ValidateFilteringOnResponseChildCollections(string resourceModelName, dynamic data,
            XElement profileElt, Func<string, bool> filterPropertyNamePredicate, bool inclusiveFilters)
        {
            var filteredCollectionElts = GetFilteredCollectionElts(profileElt, resourceModelName, true, filterPropertyNamePredicate, inclusiveFilters);

            if (!filteredCollectionElts.Any())
                Assert.Fail("No filtered collections matching the criteria for validation were found.");

            // Iterate through the filtered collections
            foreach (var filteredCollectionElt in filteredCollectionElts)
            {
                var filterElt = filteredCollectionElt.Element("Filter");

                // Get the filter's values
                var filterValues = filterElt
                    .Elements("Value")
                    .Select(x => x.Value)
                    .ToList();

                string collectionPropertyName = filteredCollectionElt.Attribute("name").Value;

                // Get the filter's property name
                string filterPropertyName = filterElt.Attribute("propertyName").Value;
                string jsonFilterPropertyName = ApplyJavascriptNamingConventions(
                    CompositeTermInflector.MakeSingular(collectionPropertyName),
                    filterPropertyName);

                var actualValues = new List<string>();

                var jsonPath = new Stack<string>();

                // Crawl up the hierarchy of the definition, building a JSON access path
                XElement currentElement = filteredCollectionElt;

                while (currentElement != null && !currentElement.Name.LocalName.EndsWith("ContentType"))
                {
                    if (currentElement.Name == "Collection"
                        || currentElement.Name == "Object")
                    {
                        string parentName = null;
                            
                        if (currentElement.Parent.Name == "Collection")
                        {
                            string grandparentClassName = CompositeTermInflector.MakeSingular(currentElement.Parent.Attribute("name").Value);

                            parentName = ApplyJavascriptNamingConventions(
                                grandparentClassName,
                                currentElement.Attribute("name").Value);
                        }
                        else if (currentElement.Parent.Name == "Object")
                        {
                            string grandparentClassName = currentElement.Parent.Attribute("name").Value;

                            parentName = ApplyJavascriptNamingConventions(
                                grandparentClassName,
                                currentElement.Attribute("name").Value);
                        }
                        else if (currentElement.Parent.Name.LocalName.EndsWith("ContentType"))
                        {
                            parentName = ApplyJavascriptNamingConventions(
                                resourceModelName,
                                currentElement.Attribute("name").Value);
                        }
                        else
                        {
                            throw new Exception(string.Format(
                                "Unexpected element '{0}' encountered in Profile definition '{1}' while trying to build path for extracting collection from JSON response.",
                                currentElement.Name, profileElt.Attribute("name").Value));
                        }

                        if (currentElement.Name == "Collection" && currentElement != filteredCollectionElt)
                            jsonPath.Push(parentName + "[0]");
                        else
                            jsonPath.Push(parentName);
                    }
                    else
                    {
                        throw new Exception(string.Format(
                            "Unexpected element '{0}' encountered in Profile definition '{1}' while trying to build path for extracting collection from JSON response.",
                            currentElement.Name, profileElt.Attribute("name").Value));
                    }

                    currentElement = currentElement.Parent;
                }

                // Navigate down the JSON data to get to the targeted collection's data
                var currentContext = data;
                string segment;

                while (jsonPath.Any() && (segment = jsonPath.Pop()) != null)
                {
                    string segmentCollectionPropertyName;

                    currentContext = segment.TryTrimSuffix("[0]", out segmentCollectionPropertyName) 
                        ? currentContext[segmentCollectionPropertyName][0] 
                        : currentContext[segment];
                }

                // Make sure we found the data for comparison.
                if (currentContext == null)
                {
                    throw new Exception(
                        string.Format(
                            "Unable to get items for collection property '{0}'.",
                            collectionPropertyName));
                }

                var collectionItems = currentContext;

                // Extract the actual values for the property in question
                foreach (dynamic item in collectionItems)
                    actualValues.Add((string)item[jsonFilterPropertyName]);

                // For IncludeOnly filters, get non-conforming values
                var nonConformingValues = inclusiveFilters
                    ? actualValues.Except(filterValues).ToList()
                    : actualValues.Intersect(filterValues).ToList();

                if (nonConformingValues.Any())
                {
                    // Provide a clear failure message
                    Assert.Fail(
                        "Collection '{0}' contains items that should have been filtered out. Filter defines the set ['{1}'] to be {3}, but actual results contained non-conforming values ['{2}'].",
                        collectionPropertyName,
                        string.Join("', '", filterValues),
                        string.Join("', '", nonConformingValues),
                        inclusiveFilters ? "the only values included" : "excluded");
                }

                Console.WriteLine("Verified property '{3}' on child collection '{0}' conforms to {1} set ['{2}'].",
                    collectionPropertyName,
                    inclusiveFilters ? "inclusive" : "exclusive",
                    string.Join("', '", filterValues),
                    filterPropertyName);
            }
        }

        private static IList<XElement> GetFilteredCollectionElts(XElement profileElt, string resourceName, bool isReadContentType, Func<string, bool> filterPropertyNamePredicate, bool inclusiveFilters)
        {
            // Get the filtered collections
            var filteredCollectionElts = profileElt.XPathSelectElements(string.Format(
                "Resource[@name='{0}']/{1}ContentType//Collection[Filter[@filterMode='{2}Only']]",
                resourceName, isReadContentType ? "Read" : "Write", inclusiveFilters ? "Include" : "Exclude"))
                // Don't process filters against properties we're not interested in right now.
                .Where(e => filterPropertyNamePredicate(e.Element("Filter").Attribute("propertyName").Value))
                .ToList();

            return filteredCollectionElts;
        }

        private static string ApplyJavascriptNamingConventions(string resourceName, string propertyName)
        {
            // Special handling for School resource, which is derived from EducationOrganization
            var prefixesToProcess = new List<string> { resourceName };
            if (resourceName == "School")
            {
                // Trimming this would conflict with EdOrgCategories... leave prefix alone.
                if (propertyName.EqualsIgnoreCase("SchoolCategories"))
                    return propertyName.ToCamelCase();

                prefixesToProcess.Add("EducationOrganization");
            }

            string trimmedPropertyName = null;

            if (!prefixesToProcess.Any(x => propertyName.TryTrimPrefix(x, out trimmedPropertyName)))
                trimmedPropertyName = propertyName;

            // Convert to the idiomatic JSON name
            string jsonCollectionPropertyName = trimmedPropertyName.ToCamelCase();
            return jsonCollectionPropertyName;
        }

        [StepArgumentTransformation(@"(conforming|non-conforming)")]
        public ConformanceType ConformanceTypeTransform(string conformanceTypeText)
        {
            string valueText = conformanceTypeText.Replace("-", string.Empty);

            return (ConformanceType)Enum.Parse(typeof(ConformanceType), valueText, ignoreCase: true);
        }
    }

    public enum IncludedOrExcluded
    {
        Included = 1,
        Excluded,
    }

    public enum ConformanceType
    {
        Conforming = 1,
        NonConforming,
    }


}