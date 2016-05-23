using System;
using System.Linq;
using EdFi.Identity.WebApi.Models;
using EduId.WebApi.Models;
using FluentValidation.Results;
using NUnit.Framework;
using Should;

namespace UnitTests.EdFi.EduId
{
   
    [TestFixture]
    public class When_validating_an_identity
    {
        private IdentityResourceCreateValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new IdentityResourceCreateValidator();
        }

        [Test]
        public void Should_return_no_errors_given_a_valid_object()
        {
            var objectToValidate = new IdentityResource
                {
                    GivenNames = "John",
                    FamilyNames = "Smith",
                    BirthGender = "mAlE"
                };

            _validator.Validate(objectToValidate).IsValid.ShouldBeTrue();
        }

        [Test]
        public void Should_return_error_when_GivenNames_is_empty()
        {
            var objectToValidate = new IdentityResource
            {
                FamilyNames = "Smith"
            };


            var validationResults = _validator.Validate(objectToValidate);
            Dump(validationResults);
            validationResults.Errors.Any(x => x.PropertyName == "GivenNames").ShouldBeTrue();
        }

        private static void Dump(ValidationResult validationResults)
        {
            foreach (var error in validationResults.Errors)
            {
                Console.WriteLine(error);
            }
        }

        [Test]
        public void Should_return_error_when_FamilyNames_is_empty()
        {
            var objectToValidate = new IdentityResource
            {
                GivenNames = "John",
                FamilyNames = ""
            };


            var validationResults = _validator.Validate(objectToValidate);
            Dump(validationResults);
            validationResults.Errors.Any(x => x.PropertyName == "FamilyNames").ShouldBeTrue();
        }


        [Test]
        public void Should_return_error_when_gender_is_empty()
        {
            var objectToValidate = new IdentityResource
            {
                FamilyNames = "Smith"
            };


            var validationResults = _validator.Validate(objectToValidate);
            Dump(validationResults);
            validationResults.Errors.Any(x => x.PropertyName == "BirthGender").ShouldBeTrue();
        }

        [Test]
        public void Should_return_error_when_gender_is_invalid()
        {
            var objectToValidate = new IdentityResource
            {
                FamilyNames = "Smith",
                BirthGender = "Inconnu"
            };


            var validationResults = _validator.Validate(objectToValidate);
            Dump(validationResults);
            validationResults.Errors.Any(x => x.PropertyName == "BirthGender").ShouldBeTrue();
        }

        [Test]
        public void Should_return_error_when_BirthDate_is_a_future_date()
        {
            var objectToValidate = new IdentityResource
            {
                BirthDate = DateTime.Now.AddDays(1)
            };


            var validationResults = _validator.Validate(objectToValidate);
            Dump(validationResults);
            validationResults.Errors.Any(x => x.PropertyName == "BirthDate").ShouldBeTrue();
        }
    }

}
