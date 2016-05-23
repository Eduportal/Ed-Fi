namespace EdFi.Ods.Tests.EdFi.Ods.Admin.Services
{
    using System.Configuration;

    using global::EdFi.Ods.Admin.Security;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    class ProtectedConfigurationTests
    {
        [Test]
        public void When_Accessing_Protected_Settings()
        {
            var dlpSettings = ConfigurationManager.GetSection("DlpProtectedSettings") as DlpProtectedSettings;
            if (dlpSettings==null) Assert.Fail("Expected DlpProectedSettings in web.config, but not found or could not read");
            dlpSettings.SendGridCredentials.UserName.ShouldEqual("Bingo");
            dlpSettings.SendGridCredentials.Password.ShouldEqual("Tingo");
        }
    }
}
