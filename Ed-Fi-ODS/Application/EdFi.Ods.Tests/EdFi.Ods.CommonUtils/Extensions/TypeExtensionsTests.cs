namespace EdFi.Ods.Tests.EdFi.Ods.CommonUtils.Extensions
{
    using System.Collections.Generic;

    using global::EdFi.Ods.Common.Utils.Extensions;

    using NUnit.Framework;

    using Should;

    public class TypeExtensionsTests
    {
        [TestFixture]
        public class When_type_can_be_cast_to_another_type
        {
            [Test]
            public void Should_report_that_it_can_be_cast()
            {
                typeof (List<string>).CanBeCastTo<IEnumerable<string>>().ShouldBeTrue();
            }

            [Test]
            public void Should_not_report_that_the_type_cannot_be_cast_to_the_other_type()
            {
                typeof(List<string>).CannotBeCastTo<IEnumerable<string>>().ShouldBeFalse();
            }
        }

        [TestFixture]
        public class When_type_cannot_be_cast_to_another_type
        {
            [Test]
            public void Should_report_that_it_cannot_be_cast()
            {
                typeof (List<string>).CannotBeCastTo<IEnumerable<int>>().ShouldBeTrue();
            }

            [Test]
            public void Should_not_report_that_the_type_can_be_cast_to_the_other_type()
            {
                typeof(List<string>).CanBeCastTo<IEnumerable<int>>().ShouldBeFalse();
            }
        }
    }
}