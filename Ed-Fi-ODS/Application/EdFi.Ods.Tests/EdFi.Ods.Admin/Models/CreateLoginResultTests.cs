namespace EdFi.Ods.Tests.EdFi.Ods.Admin.Models
{
    using global::EdFi.Ods.Admin.Models.Results;

    using NUnit.Framework;

    using Should;

    public class CreateLoginResultTests
    {
        [TestFixture]
        public class When_adding_failures
        {
            private string[] _failingFields;

            [TestFixtureSetUp]
            public void Setup()
            {
                var result = new CreateLoginResult()
                    .AddFailingField(x => x.Email)
                    .AddFailingField(x => x.Name)
                    .AddFailingField(x => x.Name) //duplicate
                    .AddFailingField(x => x.Name); //duplicate

                this._failingFields = result.FailingFields;
            }

            [Test]
            public void Should_persist_field_names_as_all_lowercase()
            {
                this._failingFields.ShouldContain("email");
                this._failingFields.ShouldContain("name");
            }

            [Test]
            public void Should_keep_unique_failing_fields()
            {
                this._failingFields.Length.ShouldEqual(2);
            }
        }

        [TestFixture]
        public class When_result_has_message
        {
            [Test]
            public void Should_indicate_that_message_exists()
            {
                new CreateLoginResult().HasMessage.ShouldBeFalse();

                new CreateLoginResult {Message = "Foo"}.HasMessage.ShouldBeTrue();
            }
        }
    }
}