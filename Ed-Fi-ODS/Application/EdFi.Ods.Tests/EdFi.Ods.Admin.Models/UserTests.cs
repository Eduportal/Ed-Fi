namespace EdFi.Ods.Tests.EdFi.Ods.Admin.Models
{
    using System.Linq;

    using global::EdFi.Ods.Admin.Models;

    using NUnit.Framework;

    using Should;

    public class UserTests
    {
        [TestFixture]
        public class When_creating_a_sandbox_client_without_specifying_a_key_and_secret
        {
            private User _user;
            private ApiClient _client;

            [TestFixtureSetUp]
            public void Setup()
            {
                this._user = new User();
                this._client = this._user.AddSandboxClient("MyClientName", SandboxType.Empty);
            }

            [Test]
            public void Should_set_the_client_name()
            {
                this._client.Name.ShouldEqual("MyClientName");
            }

            [Test]
            public void Should_set_approved_status_to_true()
            {
                this._client.IsApproved.ShouldBeTrue();
            }

            [Test]
            public void Should_set_sandbox_to_true()
            {
                this._client.UseSandbox.ShouldBeTrue();
            }

            [Test]
            public void Should_set_sandbox_type()
            {
                this._client.SandboxType.ShouldEqual(SandboxType.Empty);
            }

            [Test]
            public void Should_add_client_to_user()
            {
                var clients = this._user.ApiClients.ToArray();
                clients.Length.ShouldEqual(1);
                clients[0].ShouldBeSameAs(this._client);
            }

            [Test]
            public void Should_set_key_and_secret_to_a_random_value()
            {
                this._client.Key.ShouldNotBeNull();
                this._client.Key.Length.ShouldBeGreaterThan(0);
                this._client.Secret.ShouldNotBeNull();
                this._client.Secret.Length.ShouldBeGreaterThan(0);
            }
        }
    }
}