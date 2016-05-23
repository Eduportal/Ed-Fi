namespace EdFi.Ods.Tests.EdFi.Ods.CommonUtils.Configuration
{
    using global::EdFi.Common.Configuration;

    using NUnit.Framework;

    using Should;

    public class ConfigurationReaderTests
    {
        [TestFixture]
        public class When_configuration_value_is_not_present
        {
            [Test]
            public void Should_return_a_null_value()
            {
                new AppConfigValueProvider().GetValue("Non-existent Key").ShouldBeNull();
            }
        } 
    }
}