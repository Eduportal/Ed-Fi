namespace EdFi.Ods.Tests.EdFi.Ods.Common.Database
{
    using global::EdFi.Ods.Common.Database;
    using global::EdFi.Ods.Common.Security;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Rhino.Mocks;

    using Should;

    public class SandboxDatabaseNameProviderTests : TestBase
    {
        [TestFixture]
        public class When_calling_SandboxDatabaseNameProvider_GetDatabaseName : TestBase
        {
            [Test]
            public void Should_return_the_database_name()
            {
                var apiKeyContextProvider = this.Stub<IApiKeyContextProvider>();
                apiKeyContextProvider.Expect(m => m.GetApiKeyContext()).Return(new ApiKeyContext("TheApiKey", null, null, null, null));
                var expected = "EdFi_Ods_Sandbox_TheApiKey";
                var provider = new SandboxDatabaseNameProvider(apiKeyContextProvider);
                var actual = provider.GetDatabaseName();
                actual.ShouldEqual(expected);
            }
        }
    }
}
