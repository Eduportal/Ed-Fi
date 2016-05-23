namespace EdFi.Ods.Tests.EdFi.Ods.Admin.Extensions
{
    using global::EdFi.Ods.Admin.Extensions;

    using NUnit.Framework;

    using Should;

    public class StringExtensionsTests
    {
        [TestFixture]
        public class When_email_address_is_valid
        {
            [Test]
            public void Should_indicate_address_is_valid()
            {
                var address = "foo.bar@example.com";
                address.IsValidEmailAddress().ShouldBeTrue();
            }
        }

        [TestFixture]
        public class When_email_address_is_invalid
        {
            [Test]
            public void Should_indicate_address_is_invalid()
            {
                "missingdomain".IsValidEmailAddress().ShouldBeFalse();
                "missingdomain@".IsValidEmailAddress().ShouldBeFalse();
                "@example.com".IsValidEmailAddress().ShouldBeFalse();
            }
        }

    }
}