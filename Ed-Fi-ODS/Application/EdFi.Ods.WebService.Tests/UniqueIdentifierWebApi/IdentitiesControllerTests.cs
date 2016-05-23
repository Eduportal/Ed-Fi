using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using EdFi.Identity.Models;
using EdFi.Ods.WebService.Tests._Helpers;
using EdFi.Ods.WebService.Tests._TestBases;
using EduId.Database.Tests;
using NCrunch.Framework;
using NUnit.Framework;
using Should;
using Test.Common;

namespace EdFi.Ods.WebService.Tests.UniqueIdentifierWebApi
{
    [TestFixture]
    public class IdentitiesControllerTests : EmptyDatabaseHttpTestBase
    {
        private TestConfiguration config;
        private UniqueIdCreator _creator;

        [SetUp]
        public void Setup()
        {
            _creator = new UniqueIdCreator();
            config = new TestConfiguration();
            SampleDataPopulator.CleanDatabase(new SqlConnection(config.EduIdConnectionString));
        }


        [Test]
        [ExclusivelyUses(TestSingletons.EduIdDatabase)]
        public void When_creating_a_person_with_minimal_required_data()
        {
            //When creating a unique Identity
            var person = new IdentityResource() { FamilyNames = "Shiraz", GivenNames = "Asif" };
            var responseMessage = _creator.PostToIdentitiesController(person);
            var createdPerson = _creator.ExtractResultFromHttpResponse<IdentityResource>(responseMessage);
            responseMessage.StatusCode.ShouldEqual(HttpStatusCode.OK);

            responseMessage = _creator.GetFromIdentitiesControllerById(createdPerson.UniqueId);
            var extractedPerson = _creator.ExtractResultFromHttpResponse<IdentityResource>(responseMessage);
            extractedPerson.BirthDate.ShouldBeNull();
            extractedPerson.BirthGender.ShouldBeEmpty();
        }

        [Test]
        [ExclusivelyUses(TestSingletons.EduIdDatabase)]
        public void When_creating_and_retrieving_person_with_additional_school_associations()
        {
            //When creating a unique Identity
            var person = _creator.InitializeAPersonWithFixedData();
            person.SchoolAssociation = new SchoolAssociationResource();
            person.SchoolAssociation.LocalEducationAgencyName = "Lea1";
            person.SchoolAssociation.SchoolName = "School1";
            person.SchoolAssociation.SchoolYear = "2014";
            var responseMessage = _creator.PostToIdentitiesController(person);
            var createdPerson = _creator.ExtractResultFromHttpResponse<IdentityResource>(responseMessage);
            var identity = createdPerson.UniqueId;

            responseMessage.StatusCode.ShouldEqual(HttpStatusCode.OK);
            createdPerson.SchoolAssociation.LocalEducationAgencyName.ShouldEqual("Lea1");
            createdPerson.SchoolAssociation.SchoolName.ShouldEqual("School1");
            createdPerson.SchoolAssociation.SchoolYear.ShouldEqual("2014");

            responseMessage = _creator.GetFromIdentitiesControllerById(createdPerson.UniqueId);
            var extractedPerson = _creator.ExtractResultFromHttpResponse<IdentityResource>(responseMessage);
            extractedPerson.SchoolAssociation.LocalEducationAgencyName.ShouldEqual("Lea1");
            extractedPerson.SchoolAssociation.SchoolName.ShouldEqual("School1");
            extractedPerson.SchoolAssociation.SchoolYear.ShouldEqual("2014");
        }

        [Test]
        [ExclusivelyUses(TestSingletons.EduIdDatabase)]
        public void When_creating_unique_or_similar_persons()
        {
            //When creating a unique Identity
            var person1 = _creator.InitializeAPersonWithFixedData();
            var responseMessage1 = _creator.PostToIdentitiesController(person1);
            var createdPerson1 = _creator.ExtractResultFromHttpResponse<IdentityResource>(responseMessage1);
            //and a second similar Identity with same properties
            var person2 = _creator.InitializeAPersonWithFixedData();
            var responseMessage2 = _creator.PostToIdentitiesController(person2);
            var createdPerson2 = _creator.ExtractResultFromHttpResponse<IdentityResource>(responseMessage2);

            responseMessage1.StatusCode.ShouldEqual(HttpStatusCode.OK);
            createdPerson1.BirthDate.ShouldEqual(person1.BirthDate);
            createdPerson1.BirthGender.ShouldEqual(person1.BirthGender);
            createdPerson1.FamilyNames.ShouldEqual(person1.FamilyNames);
            createdPerson1.GivenNames.ShouldEqual(person1.GivenNames);
            createdPerson1.UniqueId.ShouldNotBeNull();

            responseMessage2.StatusCode.ShouldEqual(HttpStatusCode.OK);
            createdPerson2.BirthDate.ShouldEqual(person2.BirthDate);
            createdPerson2.BirthGender.ShouldEqual(person2.BirthGender);
            createdPerson2.FamilyNames.ShouldEqual(person2.FamilyNames);
            createdPerson2.GivenNames.ShouldEqual(person2.GivenNames);
            createdPerson2.UniqueId.ShouldNotBeNull();

            createdPerson2.UniqueId.ShouldNotBeSameAs(createdPerson1.UniqueId);
        }

        [Test]
        [ExclusivelyUses(TestSingletons.EduIdDatabase)]
        public void When_accessing_a_valid_unique_person_by_id()
        {
            var person = _creator.InitializeAPersonWithUniqueData();
            var responseMessage = _creator.PostToIdentitiesController(person);
            responseMessage.StatusCode.ShouldEqual(HttpStatusCode.OK);
            var createdPerson = _creator.ExtractResultFromHttpResponse<IdentityResource>(responseMessage);
            responseMessage = _creator.GetFromIdentitiesControllerById(createdPerson.UniqueId);
            var returnedPerson = _creator.ExtractResultFromHttpResponse<IdentityResource>(responseMessage);
            returnedPerson.UniqueId.ShouldEqual(createdPerson.UniqueId);
            returnedPerson.BirthDate.ShouldEqual(person.BirthDate);
            returnedPerson.BirthGender.ShouldEqual(person.BirthGender);
            returnedPerson.GivenNames.ShouldEqual(person.GivenNames);
            returnedPerson.FamilyNames.ShouldEqual(person.FamilyNames);
        }

        [Test]
        [ExclusivelyUses(TestSingletons.EduIdDatabase)]
        public void When_accessing_an_invalid_person_by_id()
        {
            var responseMessage = _creator.GetFromIdentitiesControllerById("SomeNonExistentId");
            var returnedPerson = _creator.ExtractResultFromHttpResponse<IdentityResource>(responseMessage);
            responseMessage.IsSuccessStatusCode.ShouldBeFalse();
            responseMessage.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
            returnedPerson.ShouldBeNull();
        }

        [Test]
        [ExclusivelyUses(TestSingletons.EduIdDatabase)]
        public void When_accessing_duplicate_valid_persons_by_id()
        {
            var responseMessage = _creator.GetFromIdentitiesControllerById("SomeNonExistentId");
            var returnedPerson = _creator.ExtractResultFromHttpResponse<IdentityResource>(responseMessage);
            responseMessage.IsSuccessStatusCode.ShouldBeFalse();
            responseMessage.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
            returnedPerson.ShouldBeNull();
        }

        [Test]
        [ExclusivelyUses(TestSingletons.EduIdDatabase)]
        public void When_searching_for_a_unique_person()
        {
            var personCommon = _creator.InitializeAPersonWithFixedData();
            _creator.PostToIdentitiesController(personCommon);
            var personUnique = _creator.InitializeAPersonWithUniqueData();
            personUnique.GivenNames = "Rumpelstiltskin" + DateTime.Now.Ticks;
            personUnique.BirthDate = DateTime.Now.AddDays(new Random().Next(-100, 1));
            _creator.PostToIdentitiesController(personUnique).StatusCode.ShouldEqual(HttpStatusCode.OK);
            var request = _creator.MapIdentityResourceToIdentityRequest(personUnique);
            var responseMessage = _creator.GetFromIdentitiesControllerUsingPerson(request);
            var matchResult = _creator.ExtractResultFromHttpResponse<EduId.Models.Identity[]>(responseMessage);
            matchResult.Where(r => r.IsMatch).ToArray().Length.ShouldEqual(1);
            matchResult.First().Weight.ShouldBeGreaterThan(0);
            matchResult.Count(m => m.IsMatch).ShouldEqual(1);

        }

        [Test]
        [ExclusivelyUses(TestSingletons.EduIdDatabase)]
        public void When_searching_matches_with_limited_result()
        {
            var personCommon = _creator.InitializeAPersonWithFixedData();
            _creator.PostToIdentitiesController(personCommon);
            var personUnique = new IdentityResource
            {
                BirthDate = new DateTime(1995, 2, 3),
                BirthGender = "Male",
                FamilyNames = "Smith",
                GivenNames = "Rumpelstiltskin",
            };
            ;
            _creator.PostToIdentitiesController(personUnique).StatusCode.ShouldEqual(HttpStatusCode.OK);
            var request = _creator.MapIdentityResourceToIdentityRequest(personUnique);
            request.Limit = 1;
            var responseMessage = _creator.GetFromIdentitiesControllerUsingPerson(request);
            var matchResult = _creator.ExtractResultFromHttpResponse<EduId.Models.Identity[]>(responseMessage);

            matchResult.Count().ShouldEqual(1);
        }

        [Test]
        [ExclusivelyUses(TestSingletons.EduIdDatabase)]
        public void When_searching_for_non_unique_person()
        {
            var person = _creator.InitializeAPersonWithFixedData();
            _creator.PostToIdentitiesController(person);
            _creator.PostToIdentitiesController(person);//twice to create duplicate
            var responseMessage = _creator.GetFromIdentitiesControllerUsingPerson(_creator.MapIdentityResourceToIdentityRequest(person));
            var matchResult = _creator.ExtractResultFromHttpResponse<IdentityResource[]>(responseMessage);
            matchResult.Length.ShouldBeGreaterThan(1);
        }
    }
}