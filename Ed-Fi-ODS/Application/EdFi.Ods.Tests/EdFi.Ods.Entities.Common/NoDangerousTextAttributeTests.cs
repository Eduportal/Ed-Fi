using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EdFi.Ods.Entities.Common.Validation;
using EdFi.Ods.Tests._Bases;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.Entities.Common
{
    public class DangerousTextTestObject
    {
        public DangerousTextTestObject(string name)
        {
            Name = name;
        }

        [NoDangerousText]
        public string Name { get; set; }
    }

    public class When_validating_empty_string_for_NoDangerousText : TestFixtureBase
    {
        private ICollection<ValidationResult> _actualResults;

        /// <summary>
        /// Executes the code to be tested.
        /// </summary>
        protected override void Act()
        {
            var testObject = new DangerousTextTestObject(string.Empty);

            var validator = new DataAnnotationsObjectValidator();
            _actualResults = validator.ValidateObject(testObject);
        }

        [Assert]
        public void Should_not_have_any_validation_errors()
        {
            Assert.That(_actualResults, Is.Empty);
        }
    }

    public class When_validating_string_with_safe_content_for_NoDangerousText : TestFixtureBase
    {
        private ICollection<ValidationResult> _actualResults;

        /// <summary>
        /// Executes the code to be tested.
        /// </summary>
        protected override void Act()
        {
            var testObject = new DangerousTextTestObject("Hello World");

            var validator = new DataAnnotationsObjectValidator();
            _actualResults = validator.ValidateObject(testObject);
        }

        [Assert]
        public void Should_not_have_any_validation_errors()
        {
            Assert.That(_actualResults, Is.Empty);
        }
    }

    public class When_validating_string_with_dangerous_content_for_NoDangerousText : TestFixtureBase
    {
        private ICollection<ValidationResult> _actualResults;

        /// <summary>
        /// Executes the code to be tested.
        /// </summary>
        protected override void Act()
        {
            var testObject = new DangerousTextTestObject("<Hello World>");

            var validator = new DataAnnotationsObjectValidator();
            _actualResults = validator.ValidateObject(testObject);
        }

        [Assert]
        public void Should_have_a_validation_error_regarding_a_potentially_dangerous_value()
        {
            Assert.That(_actualResults, Has.Count.EqualTo(1));
            Assert.That(_actualResults.Single().ErrorMessage, Is.StringContaining("potentially dangerous value"));
        }
    }
}
