using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdFi.Ods.Entities.Common.Validation;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Should;

namespace EdFi.Ods.Api.Data.Tests
{
    public class EntityAttributesTest
    {
        public class TestTypeForCheckingAttributeDecorations
        {
            [RequiredWithNonDefault]
            [Display(Name=@"SomeOtherName")]
            public int PropertyWithDisplayAttribute { get; set; }
            [RequiredWithNonDefault]
            public int PropertyWithoutDisplayAttribute { get; set; }
        }

        [TestFixture]
        public class When_decorating_properties_with_attribute_RequiredWithNonDefault_having_display_attribute
        {
            private TestTypeForCheckingAttributeDecorations testType;

            [TestFixtureSetUp]
            public void Setup()
            {
                testType = new TestTypeForCheckingAttributeDecorations {PropertyWithDisplayAttribute = 0, PropertyWithoutDisplayAttribute = 1};
            }
            
            [Test]
            public void Should_use_display_name_attribute_in_validation_message()
            {
                var validationResults = new List<ValidationResult>();
                Validator.TryValidateObject(testType, new ValidationContext(testType, null, null), validationResults, true);
                validationResults.Count.ShouldEqual(1);
                validationResults.First().ErrorMessage.ShouldEqual("SomeOtherName is required.");
            }
        }

        [TestFixture]
        public class When_decorating_properties_with_attribute_RequiredWithNonDefault_without_display_attribute
        {
            private TestTypeForCheckingAttributeDecorations testType;

            [TestFixtureSetUp]
            public void Setup()
            {
                testType = new TestTypeForCheckingAttributeDecorations { PropertyWithDisplayAttribute = 1, PropertyWithoutDisplayAttribute = 0};
            }

            [Test]
            public void Should_use_member_name_in_validation_message()
            {
                var validationResults = new List<ValidationResult>();
                Validator.TryValidateObject(testType, new ValidationContext(testType, null, null), validationResults, true);
                validationResults.Count.ShouldEqual(1);
                validationResults.First().ErrorMessage.ShouldEqual("PropertyWithoutDisplayAttribute is required.");
            }
         }

        [TestFixture]
        public class When_decorating_properties_with_attribute_RequiredWithNonDefault
        {
            private RequiredWithNonDefaultAttribute attributeLogic;
            
            [TestFixtureSetUp]
            public void Setup()
            {
                attributeLogic = new RequiredWithNonDefaultAttribute();
            }

            [Test]
            public void Should_accept_zero_values_for_decimal_types()
            {
                attributeLogic.IsValid(0.0M).ShouldBeTrue();
            }

            [Test]
            public void Should_not_accept_zero_values_for_int_types()
            {
                try
                {
                    attributeLogic.IsValid(0);
                    Assert.Fail("Exception was expected but not thrown");
                }
                catch (Exception)
                {
                    Assert.Pass("Exception should be thrown because of failed validation");
                }
                
            }

        }
    }
}
