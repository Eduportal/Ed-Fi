namespace EdFi.Ods.Tests.EdFi.Ods.CommonUtils.Extensions
{
    using global::EdFi.Ods.Common.Utils.Extensions;

    using NUnit.Framework;

    using Should;

    public class ObjectExtensionsTests
    {
        [TestFixture]
        public class When_type_is_object
        {
            [Test]
            public void Should_be_default_for_null()
            {
                ObjectExtensions.IsDefault(null, typeof(object)).ShouldBeTrue();
            }

            [Test]
            public void Should_not_be_default_for_nonnull()
            {
                new object().IsDefault(typeof(object)).ShouldBeFalse();
            }

            [Test]
            public void Should_not_be_default_for_integers()
            {
                1.IsDefault(typeof(object)).ShouldBeFalse();
                0.IsDefault(typeof(object)).ShouldBeFalse();
            }
        }

        [TestFixture]
        public class When_type_is_int
        {
            [Test]
            public void Should_be_default_for_zero()
            {
                0.IsDefault(typeof(int)).ShouldBeTrue();
            }

            [Test]
            public void Should_not_be_default_for_one()
            {
                1.IsDefault(typeof(int)).ShouldBeFalse();
            }

            [Test]
            public void Should_not_be_default_for_object_value()
            {
                new object().IsDefault(typeof(int)).ShouldBeFalse();
            }
        }
    }
}