using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdFi.Common.Configuration;
using EdFi.Ods.Admin.Models;
using NUnit.Framework;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.Admin.Models
{
    public class ClientAppRepoTests
    {
        [TestFixture]
        public class When_calling_the_clientAppRepo
        {
            private ClientAppRepo _clientAppRepo;
            private DbContextTransaction _transaction;
            private ApiClient _testClient;

            [TestFixtureSetUp]
            public void Setup()
            {
                var usersContext = new UsersContext();
                _transaction = usersContext.Database.BeginTransaction();

                var sandboxProvisionerStub = MockRepository.GenerateStub<ISandboxProvisioner>();
                var configValueProviderStub = MockRepository.GenerateStub<IConfigValueProvider>();

                _clientAppRepo = new ClientAppRepo(usersContext, sandboxProvisionerStub, configValueProviderStub);
                _testClient = new ApiClient(true) { Name = "ClientAppRepoTest" + Guid.NewGuid().ToString("N") };
                _clientAppRepo.AddClient(_testClient);
            }

            [Test]
            public void Should_get_client_with_key_only()
            {
                var tmpClient = _clientAppRepo.GetClient(_testClient.Key);
                tmpClient.ShouldNotBeNull();
                tmpClient.Name.ShouldEqual(_testClient.Name);
            }

            [Test]
            public void Should_get_client_with_key_and_secret()
            {
                var tmpClient = _clientAppRepo.GetClient(_testClient.Key, _testClient.Secret);
                tmpClient.ShouldNotBeNull();
                tmpClient.Name.ShouldEqual(_testClient.Name);
            }

            [Test]
            public void Should_not_get_client_with_key_and_null_secret()
            {
                var tmpClient = _clientAppRepo.GetClient(_testClient.Key, null);
                tmpClient.ShouldBeNull();
            }

            [Test]
            public void Should_not_get_client_with_key_and_empty_secret()
            {
                var tmpClient = _clientAppRepo.GetClient(_testClient.Key, string.Empty);
                tmpClient.ShouldBeNull();
            }

            [TestFixtureTearDown]
            public void TearDown()
            {
                _transaction.Rollback();
            }
        }
    }
}
