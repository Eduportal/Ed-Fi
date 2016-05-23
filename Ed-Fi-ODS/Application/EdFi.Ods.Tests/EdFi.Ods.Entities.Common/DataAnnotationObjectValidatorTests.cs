using EdFi.Ods.Entities.Common.Validation;
using NUnit.Framework;
using Should;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EdFi.Ods.Tests.EdFi.Ods.Entities.Common
{
    [TestFixture]
    public class DataAnnotationsObjectValidatorTests
    {
        private readonly DataAnnotationsObjectValidator validator = new DataAnnotationsObjectValidator();
        private ICollection<ValidationResult> validationResults;

        private class DataAnnotatedProperty
        {
            [Required]
            public string RequiredProperty { get; set; }
        }

        private class DataAnnotatedClass
        {
            [ValidateObject]
            public DataAnnotatedProperty Property { get; set; }
        }

        private class DataAnnotatedCollectionClass
        {
            [ValidateEnumerable]
            public IEnumerable<DataAnnotatedClass> PropertyList { get; set; }
        }

        [Test]
        public void When_validating_object_with_data_annotation_should_validate_and_raise_error()
        {
            var objectToValidate = new DataAnnotatedProperty();

            validationResults = validator.ValidateObject(objectToValidate);

            validationResults.Count.ShouldEqual(1);
        }

        [Test]
        public void When_validating_object_with_validationObjectAttribute_should_validate_recursively_and_raise_error()
        {
            var objectToValidate = new DataAnnotatedClass { Property = new DataAnnotatedProperty() };

            validationResults = validator.ValidateObject(objectToValidate);

            validationResults.Count.ShouldEqual(1);
        }

        [Test]
        public void When_validating_object_with_validateEnumerableAttribute_should_validate_items_in_enumeration_and_raise_error()
        {
            var list = new List<DataAnnotatedClass> { new DataAnnotatedClass { Property = new DataAnnotatedProperty() } };
            var objectToValidate = new DataAnnotatedCollectionClass { PropertyList = list };

            validationResults = validator.ValidateObject(objectToValidate);

            validationResults.Count.ShouldEqual(1);
        }
    }
}
