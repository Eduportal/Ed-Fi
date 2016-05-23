namespace EdFi.Ods.Tests.EdFi.Ods.Entities.Common
{
    using System;

    using global::EdFi.Ods.Entities.Common.Validation;

    using NUnit.Framework;

    using Should;

    public class SqlServerDateTimeRangeAttributeTests
    {
        [TestFixture]
        public class When_validating_a_sql_date_within_sql_server_range
        {
            [Test]
            public void Should_return_success()
            {
                var result = new SqlServerDateTimeRangeAttribute().IsValid(DateTime.Now);
                result.ShouldBeTrue();
            }
        }

        [TestFixture]
        public class When_validating_a_null_sql_date
        {
            [Test]
            public void Should_return_succes()
            {
                var result = new SqlServerDateTimeRangeAttribute().IsValid(null);
                result.ShouldBeTrue();
            }
        }

        [TestFixture]
        public class When_validating_a_sql_date_outside_of_sql_server_range
        {
            [Test]
            public void Should_return_failure()
            {
                var result = new SqlServerDateTimeRangeAttribute().IsValid(new DateTime(1600, 01, 01));
                result.ShouldBeFalse();               
            }
        }

        [TestFixture]
        public class When_validating_a_non_datetime_type
        {
            [Test]
            [ExpectedException(typeof (ArgumentException))]
            public void Should_throw_argument_exception()
            {
                new SqlServerDateTimeRangeAttribute().IsValid(1);
            }
        }
    }
}
