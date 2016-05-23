namespace EdFi.Ods.Tests.EdFi.Ods.Common.Database
{
    public class SandboxConnectionStringMetadataProviderTests
    {
        // TODO: GKM - I believe this test is obsolete now with the refactoring to PrototypeWithDatabaseNameOverride...
        //[TestFixture]
        //public class When_calling_SandboxConnectionStringMetadataProvider : TestBase
        //{
        //    private const string expectedInitialCatalog = "OverrideDatabaseName";

        //    [Test]
        //    public void Should_return_standard_metadata()
        //    {
        //        var sandboxDatabasNameProvider = S<IDatabaseNameProvider>();
        //        sandboxDatabasNameProvider.Expect(m => m.GetDatabaseName()).Return(expectedInitialCatalog);

        //        var provider = new SandboxDatabaseConnectionStringMetadataProvider("TestStandardConnectionString", sandboxDatabasNameProvider);
        //        var actual = provider.GetDatabaseConnectionStringMetadata();

        //        actual.DataSource.ShouldEqual("SuppliedServer");
        //        actual.InitialCatalog.ShouldEqual(expectedInitialCatalog);
        //        actual.UserId.ShouldEqual("SuppliedUser");
        //        actual.Password.ShouldEqual("SuppliedPassword");
        //        actual.IntegratedSecurity.ShouldBeFalse();
        //    }

        //    [Test]
        //    public void Should_return_integrated_metadata()
        //    {
        //        var sandboxDatabasNameProvider = S<IDatabaseNameProvider>();
        //        sandboxDatabasNameProvider.Expect(m => m.GetDatabaseName()).Return(expectedInitialCatalog);

        //        var provider = new SandboxDatabaseConnectionStringMetadataProvider("TestIntegratedConnectionString", sandboxDatabasNameProvider);
        //        var actual = provider.GetDatabaseConnectionStringMetadata();

        //        actual.DataSource.ShouldEqual("SuppliedServer");
        //        actual.InitialCatalog.ShouldEqual(expectedInitialCatalog);
        //        actual.IntegratedSecurity.ShouldBeTrue();
        //        actual.UserId.ShouldBeNull();
        //        actual.Password.ShouldBeNull();
        //    }
        //}
    }
}
