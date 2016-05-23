namespace EdFi.Ods.Tests.EdFi.Ods.Swagger
{
    using System.Linq;
    using System.Reflection;

    using global::EdFi.Ods.Swagger;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class PropertyInfoToModelPropertySpecTests
    {
        private class TestType
        {
            public string PropertyWithNoAttributes { get; set; }
            [System.ComponentModel.Description("This is a Description.")]
            [System.ComponentModel.DataAnnotations.Range(1, 100)]
            public int PropertyWithAttributes { get; set; }
        }

        [Test]
        public void Should_read_description_attribute()
        {
            var sut = typeof(TestType).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                       .Single(x => x.Name == "PropertyWithAttributes");

            var result = sut.ToModelPropertySpec();

            result.Description.ShouldEqual("This is a Description.");
        }

        [Test]
        public void Should_read_range_attribute()
        {
            var sut = typeof(TestType).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                       .Single(x => x.Name == "PropertyWithAttributes");

            var result = sut.ToModelPropertySpec();

            result.Minimum.ShouldEqual("1");
            result.Maximum.ShouldEqual("100");
        }

        [Test]
        public void Should_not_throw_if_properpty_has_no_attributes()
        {
            var sut = typeof (TestType).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                       .Single(x => x.Name == "PropertyWithNoAttributes");

            var result = sut.ToModelPropertySpec();

            result.ShouldNotBeNull();
        }
    }
}
