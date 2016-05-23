namespace EdFi.Ods.Tests.EdFi.Ods.CommonUtils
{
    using global::EdFi.Ods.Common.Utils;

    using NUnit.Framework;

    using Should;

    public class RandomUtilTests
    {
        [TestFixture]
        public class When_generating_a_random_string
        {
            [Test]
            public void Should_generate_a_string_of_the_correct_length()
            {
                for (int i = 1; i < 32; i++)
                    RandomUtil.GenerateRandomBase64String(i).Length.ShouldEqual(i);
            }

            [Test]
            public void Should_not_ever_include_the_equal_sign_since_it_isnt_random()
            {
                for(int i = 1; i < 32; i++)
                    RandomUtil.GenerateRandomBase64String(i).ShouldNotContain("=");
            }
        }
    }
}