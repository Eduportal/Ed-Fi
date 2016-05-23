namespace EdFi.Ods.Tests.EdFi.Ods.Common.Validation
{
    using System.ComponentModel.DataAnnotations;

    using global::EdFi.Ods.Entities.Common.Validation;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    public class ParentClass
    {
        [ValidateObject]
        public ChildClass Child { get; set; }
    }

    public class ChildClass
    {
        public int Property { get; set; }
    }

    [TestFixture]
    public class When_validating_an_object_with_a_validated_child_reference_that_is_null : TestFixtureBase
    {
        private ValidationResult validationResult;

        protected override void ExecuteBehavior()
        {
            var parentObject = new ParentClass();

            var validator = new ValidateObjectAttribute();

            this.validationResult = validator.GetValidationResult(parentObject, 
                new ValidationContext(parentObject, null, null));
        }

        [Test]
        public virtual void Should_indicate_success()
        {
            this.validationResult.ShouldEqual(ValidationResult.Success);
        }
    } 

}