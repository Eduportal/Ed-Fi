namespace EdFi.Ods.XsdParsing.Tests.Extensions
{
    using EdFi.Ods.CodeGen.XsdToWebApi.Extensions;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class StringExtensionsTests
    {
        [Test]
        public void Given_String_With_Extension_Suffix_Should_Remove_It()
        {
            var expected = "Staff";
            var withSuffix = "StaffExtension";
            withSuffix.StripExtensionNameValues().ShouldEqual(expected);
        }

        [Test]
        public void Given_String_With_Extension_Prefix_Should_Remove_It()
        {
            var expected = "Staff";
            var withPrefix = StringExtensions.ProjectExtension + "Staff";
            withPrefix.StripExtensionNameValues().ShouldEqual(expected);
        }

        [Test]
        public void Given_String_With_Restriction_Suffix_Should_Remove_It()
        {
            var expected = "Staff";
            var withSuffix = "StaffRestriction";
            withSuffix.StripExtensionNameValues().ShouldEqual(expected);
        }

        [Test]
        public void Given_String_With_Schemaa_Prefix_And_Restriction_Suffix_Should_Remove_Both()
        {
            var expected = "Staff";
            var allTheJunk = StringExtensions.ProjectExtension + "StaffRestriction";
            allTheJunk.StripExtensionNameValues().ShouldEqual(expected);
        }

        [Test]
        public void Given_String_With_Schemaa_Prefix_And_Extension_Suffix_Should_Remove_Both()
        {
            var expected = "Staff";
            var allTheJunk = StringExtensions.ProjectExtension + "StaffExtension";
            allTheJunk.StripExtensionNameValues().ShouldEqual(expected);
        }
    }
}