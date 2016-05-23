namespace EdFi.Ods.Tests.EdFi.Ods.Admin.Services
{
    using System.Linq;

    using global::EdFi.Ods.Admin.Models;
    using global::EdFi.Ods.Admin.Models.Client;
    using global::EdFi.Ods.Admin.Services;
    using global::EdFi.Ods.Tests.EdFi.Ods.Admin.Models._Stubs;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Rhino.Mocks;

    using Should;

    public class ClientCreatorTests
    {
        [TestFixture]
        public class When_creating_a_sandbox_client : TestBase
        {
            private ApiClient _createdClient;
            private User _user;
            private TestSandboxProvisioner.Sandbox[] _sandboxes;

            [TestFixtureSetUp]
            public void Setup()
            {
                var vendorId = 444;
                var sandboxType = SandboxType.Minimal;
                var sandboxClientCreateModel = new SandboxClientCreateModel {Name = "SomeName", SandboxType = sandboxType};
                var user = new User{Vendor = new Vendor{VendorId = vendorId}};

                var stubUserUpdater = new StubUserUpdater();
                var sandboxProvisioner = new TestSandboxProvisioner();
                var defaultApplicationCreator = this.Stub<IDefaultApplicationCreator>();
                var application = new Application {ApplicationName = "My App"};
                application.CreateEducationOrganizationAssociation(111);
                application.CreateEducationOrganizationAssociation(222);
                application.CreateEducationOrganizationAssociation(333);

                defaultApplicationCreator.Stub(
                    x => x.FindOrCreateUpdatedDefaultSandboxApplication(vendorId, sandboxType))
                                         .Return(application);

                var creator = new ClientCreator(stubUserUpdater, sandboxProvisioner, defaultApplicationCreator);
                this._createdClient = creator.CreateNewSandboxClient(sandboxClientCreateModel, user);
                this._user = stubUserUpdater.LastUpdatedUser;
                this._sandboxes = sandboxProvisioner.AddedSandboxes;
            }

            [Test]
            public void Should_create_a_new_client()
            {
                var clients = this._user.ApiClients.ToArray();
                clients.Length.ShouldEqual(1);
                clients[0].ShouldBeSameAs(this._createdClient);
            }

            [Test]
            public void Should_update_the_user()
            {
                this._user.ShouldNotBeNull();
            }

            [Test]
            public void Should_provision_a_sandbox()
            {
                this._sandboxes.Length.ShouldEqual(1);
                this._sandboxes[0].Key.ShouldEqual(this._createdClient.Key);
                this._sandboxes[0].SandboxType.ShouldEqual(SandboxType.Minimal);
            }

            [Test]
            public void Should_set_the_default_application()
            {
                var client = this._user.ApiClients.Single();
                client.Application.ApplicationName.ShouldEqual("My App");
            }

            [Test]
            public void Should_set_the_local_education_agency_associations()
            {
                var client = this._user.ApiClients.Single();
                var leas = client.ApplicationEducationOrganizations.Select(x => x.EducationOrganizationId).ToArray();
                leas.Length.ShouldEqual(3);
                leas.ShouldContain(111);
                leas.ShouldContain(222);
                leas.ShouldContain(333);
            }
        }
    }
}