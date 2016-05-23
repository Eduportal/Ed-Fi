namespace EdFi.Ods.Tests.EdFi.Ods.Common
{
    using global::EdFi.Ods.Common;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class InterchangeTypeTest
    {
        [TestFixture]
        public class When_calling_get_by_name
        {
            [Test]
            public void Should_return_result_given_a_valid_name_but_improper_case()
            {
                var improperCaseNameName = InterchangeType.MasterSchedule.Name.ToUpper();
                InterchangeType.GetByName(improperCaseNameName).ShouldEqual(InterchangeType.MasterSchedule);
            }

            [Test]
            public void Should_return_null_given_an_invalid_name_but_improper_case()
            {
                InterchangeType.GetByName("I'm invalid").ShouldBeNull();
            }
        }

        [TestFixture]
        public class RequiredLoadOrder
        {
            [Test]
            public void Should_include_all_interchange_types()
            {
                CollectionAssert.AreEquivalent(InterchangeType.RequiredLoadOrder, InterchangeType.GetValues());
            }
        }
    }


}
