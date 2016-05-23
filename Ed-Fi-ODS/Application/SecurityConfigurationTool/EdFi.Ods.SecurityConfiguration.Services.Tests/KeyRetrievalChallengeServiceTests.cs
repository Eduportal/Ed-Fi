using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.SecurityConfiguration.Services.Implementation;
using NUnit.Framework;
using Rhino.Mocks;
using Should;
using ApplicationEntity = EdFi.Ods.Admin.Models.Application;
using Application = EdFi.Ods.Admin.Models.Application;
using VendorEntity = EdFi.Ods.Admin.Models.Vendor;
using VendorModel = EdFi.Ods.SecurityConfiguration.Services.Model.Vendor;

// ReSharper disable InconsistentNaming
namespace EdFi.Ods.SecurityConfiguration.Services.Tests
{
    public abstract class KeyRetrievalChallengeServiceTestBase
    {
        protected const int MaxRetrial = 5;

        protected readonly IUsersContext Context;

        protected IDbSet<VendorEntity> VendorDbSet;
        protected IDbSet<User> UserDbSet { get; set; }
        protected IDbSet<Application> ApplicationDbSet { get; set; }
        protected IDbSet<ApiClient> ApiClientDbSet { get; set; }

        protected readonly IKeyRetrievalChallengeService Service;

        protected KeyRetrievalChallengeServiceTestBase()
        {
            Context = MockRepository.GenerateMock<IUsersContext>();
            VendorDbSet = MockRepository.GenerateMock<IDbSet<VendorEntity>, IQueryable>();
            UserDbSet = MockRepository.GenerateMock<IDbSet<User>, IQueryable>();
            ApplicationDbSet = MockRepository.GenerateMock<IDbSet<Application>, IQueryable>();
            ApiClientDbSet = MockRepository.GenerateMock<IDbSet<ApiClient>, IQueryable>();

            var adminContextFactory = MockRepository.GenerateMock<IAdminContextFactory>();
            adminContextFactory.Stub(x => x.Create()).Return(Context);

            var configurationService = MockRepository.GenerateMock<IConfigurationService>();
            configurationService.Stub(x => x.GetChallengeExpiryInterval()).Return(new TimeSpan(24, 0, 0));
            configurationService.Stub(x => x.GetMaxRetries()).Return(MaxRetrial);

            Service = new KeyRetrievalChallengeService(
                context: adminContextFactory,
                configurationService:configurationService,
                credentialService:null
                );
        }

        protected static void SetupDbSet<T>(IDbSet<T> dbSet, List<T> data) where T : class
        {
            var queryable = data.AsQueryable();

            dbSet.Stub(m => m.Provider).Return(queryable.Provider);
            dbSet.Stub(m => m.Expression).Return(queryable.Expression);
            dbSet.Stub(m => m.ElementType).Return(queryable.ElementType);

            Func<T, T> removeEntity = entity =>
            {
                data.Remove(entity);
                return entity;
            };
            dbSet.Stub(m => m.Remove(null)).IgnoreArguments().Do(removeEntity);

            Func<T, T> addEntity = entity =>
            {
                data.Add(entity);
                return entity;
            };
            dbSet.Stub(m => m.Add(null)).IgnoreArguments().Do(addEntity);

            Func<IEnumerator<T>> enumerate = () =>
            {
                var enumerator = queryable.GetEnumerator();
                enumerator.Reset();
                return enumerator;
            };
            dbSet.Stub(m => m.GetEnumerator()).Do(enumerate);
        }
    }

    [TestFixture]
    public class When_validating_a_challenge_id : KeyRetrievalChallengeServiceTestBase
    {
        List<Application> _applicationData;
        List<ApiClient> _apiClientData;

        private const string ValidActivationCode = "AValidActivationCode";

        private const int App1Id = 111;
        private const int App2Id = 112;
        private const int App3Id = 113;
        private const int App4Id = 114;
        private const int App5Id = 115;
        private const int App6Id = 116;

        private const int App1ApiId = 11100;
        private const int App2ApiId = 11200;
        private const int App3ApiId = 11300;
        private const int App4ApiId = 11100;
        private const int App5ApiId = 11200;
        private const int App6ApiId = 11300;

        private const string App1ChallengeId = "---1";
        private const string App2ChallengeId = "---2";
        private const string App3ChallengeId = "---3";
        private const string App4ChallengeId = "---4";
        private const string App5ChallengeId = "---5";
        private const string App6ChallengeId = "---6";
        private const string InvalidChallengeId = "---xyz";


        private static DateTime Future
        {
            get { return DateTime.Now.AddDays(1); }
        }

        private static DateTime Past
        {
            get { return DateTime.Now.AddDays(-1); }
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            // since we are testing applications, vendor should be initialized prior to application. Otherwise
            // the navigation property of Application.Vendor won't work right. 

            // mock order: ApiClient -> Application
            _apiClientData = GetApiClientData();
            SetupDbSet(ApiClientDbSet, _apiClientData);

            _applicationData = GetApplicationData();
            SetupDbSet(ApplicationDbSet, _applicationData);

            Context.Stub(x => x.Applications).PropertyBehavior();
            Context.Stub(x => x.Clients).PropertyBehavior();

            Context.Applications = ApplicationDbSet;
            Context.Clients = ApiClientDbSet;
        }

        private List<Application> GetApplicationData()
        {
            return new List<Application>
            {
                new Application
                {
                    ApplicationId = App1Id,
                    ApplicationName = "ApplicationName 1",
                    ApiClients = ApiClientDbSet.Where(a => a.ApiClientId == App1ApiId).ToList(),
                },
                new Application
                {
                    ApplicationId = App2Id,
                    ApplicationName = "ApplicationName 2",
                    ApiClients = ApiClientDbSet.Where(a => a.ApiClientId == App2ApiId).ToList(),
                },
                new Application
                {
                    ApplicationId = App3Id,
                    ApplicationName = "ApplicationName 3",
                    ApiClients = ApiClientDbSet.Where(a => a.ApiClientId == App3ApiId).ToList(),
                },
                new Application
                {
                    ApplicationId = App4Id,
                    ApplicationName = "ApplicationName 4",
                    ApiClients = ApiClientDbSet.Where(a => a.ApiClientId == App4ApiId).ToList(),
                },
                new Application
                {
                    ApplicationId = App5Id,
                    ApplicationName = "ApplicationName 5",
                    ApiClients = ApiClientDbSet.Where(a => a.ApiClientId == App5ApiId).ToList(),
                },
                new Application
                {
                    ApplicationId = App6Id,
                    ApplicationName = "ApplicationName 6",
                    ApiClients = ApiClientDbSet.Where(a => a.ApiClientId == App6ApiId).ToList(),
                },
            };
        }

        private List<ApiClient> GetApiClientData()
        {
            return new List<ApiClient>
            {
                new ApiClient // valid
                {
                    ApiClientId = App1ApiId,
                    ChallengeId = App1ChallengeId,
                    ChallengeExpiry = Future,
                    UseSandbox = false,
                    ActivationCode = ValidActivationCode,
                    KeyStatus = KeyStatus.Sent,
                    ActivationRetried = MaxRetrial - 1
                },
                new ApiClient // challenge expired
                {
                    ApiClientId = App2ApiId,
                    ChallengeId = App2ChallengeId,
                    ChallengeExpiry = Past,
                    UseSandbox = false,
                    ActivationCode = ValidActivationCode,
                    KeyStatus = KeyStatus.Sent,
                    ActivationRetried = MaxRetrial - 1
                },
                new ApiClient // challenge retried too many times
                {
                    ApiClientId = App3ApiId,
                    ChallengeId = App3ChallengeId,
                    ChallengeExpiry = Future,
                    UseSandbox = false,
                    ActivationCode = ValidActivationCode,
                    KeyStatus = KeyStatus.Sent,
                    ActivationRetried = MaxRetrial
                },
                new ApiClient // activation code is null
                {
                    ApiClientId = App4ApiId,
                    ChallengeId = App4ChallengeId,
                    ChallengeExpiry = Future,
                    UseSandbox = false,
                    ActivationCode = null,
                    KeyStatus = KeyStatus.Sent,
                    ActivationRetried = MaxRetrial - 1
                },
                new ApiClient // activation code is white spaces
                {
                    ApiClientId = App5ApiId,
                    ChallengeId = App5ChallengeId,
                    ChallengeExpiry = Future,
                    UseSandbox = false,
                    ActivationCode = "   ",
                    KeyStatus = KeyStatus.Sent,
                    ActivationRetried = MaxRetrial - 1
                },
                new ApiClient // KeyStatus is not Sent
                {
                    ApiClientId = App6ApiId,
                    ChallengeId = App6ChallengeId,
                    ChallengeExpiry = Future,
                    UseSandbox = false,
                    ActivationCode = ValidActivationCode,
                    KeyStatus = KeyStatus.Initialized,
                    ActivationRetried = MaxRetrial - 1
                },
            };
        }

        [Test]
        public void Should_validate_valid_application()
        {
            Service.IsValid(App1ChallengeId)
                .ShouldBeTrue();
        }

        [Test]
        public void Should_not_validate_invalid_challenge_id()
        {
            Service.IsValid(InvalidChallengeId)
                .ShouldBeFalse();
        }

        [Test]
        public void Should_not_validate_expired_challenge()
        {
            Service.IsValid(App2ChallengeId)
                .ShouldBeFalse();
        }

        [Test]
        public void Should_not_validate_if_retried_more_than_limit()
        {
            Service.IsValid(App3ChallengeId)
                .ShouldBeFalse();
        }

        [Test]
        public void Should_not_validate_if_the_status_is_not_sent()
        {
            Service.IsValid(App6ChallengeId)
                .ShouldBeFalse();
        }

        [Test]
        public void Should_not_validate_if_the_activation_code_has_no_value()
        {
            Service.IsValid(App5ChallengeId)
                .ShouldBeFalse();
        }

        [Test]
        public void Should_not_validate_if_the_activation_code_is_null()
        {
            Service.IsValid(App4ChallengeId)
                .ShouldBeFalse();
        }
    }
}
