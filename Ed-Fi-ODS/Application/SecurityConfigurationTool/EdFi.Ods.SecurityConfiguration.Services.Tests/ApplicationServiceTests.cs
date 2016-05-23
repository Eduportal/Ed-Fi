using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.SecurityConfiguration.Services.Implementation;
using EdFi.Ods.SecurityConfiguration.Services.Model;
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
    public abstract class ApplicationServiceTestBase
    {
        protected readonly IUsersContext Context;

        protected IDbSet<VendorEntity> VendorDbSet;
        protected IDbSet<User> UserDbSet { get; set; }
        protected IDbSet<Application> ApplicationDbSet { get; set; }
        protected IDbSet<ApiClient> ApiClientDbSet { get; set; }

        protected readonly ApplicationService ApplicationService;
        protected ICredentialService CredentialService;

        protected ApplicationServiceTestBase()
        {
            Context = MockRepository.GenerateMock<IUsersContext>();
            VendorDbSet = MockRepository.GenerateMock<IDbSet<VendorEntity>, IQueryable>();
            UserDbSet = MockRepository.GenerateMock<IDbSet<User>, IQueryable>();
            ApplicationDbSet = MockRepository.GenerateMock<IDbSet<Application>, IQueryable>();
            ApiClientDbSet = MockRepository.GenerateMock<IDbSet<ApiClient>, IQueryable>();

            var adminContextFactory = MockRepository.GenerateMock<IAdminContextFactory>();
            adminContextFactory.Stub(x => x.Create()).Return(Context);

            CredentialService = MockRepository.GenerateMock<ICredentialService>();
            ApplicationService = new ApplicationService(
                context: adminContextFactory,
                credentialService: CredentialService,
                educationOrganizationProvider: null,
                emailService: null,
                keyRetrievalChallengeService: null
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
    public class When_retrieve_application : ApplicationServiceTestBase
    {
        List<User> _userData;
        List<Application> _applicationData;
        List<VendorEntity> _vendorData;
        List<ApiClient> _apiClientData;

        private const int Vendor1Id = 11;
        private const int Vendor2Id = 12;
        private const int Vendor3Id = 13;
        private const int NotExistingVendorId = 99;
        
        private const int Vendor1UserId = 811;
        private const int Vendor2UserId = 812;

        private const int Vendor1App1Id = 111;
        private const int Vendor1App2Id = 112;
        private const int Vendor2App1Id = 121;

        private const int Vendor1App1ApiId = 11100;
        private const int Vendor1App2ApiId = 11200;
        private const int Vendor2App1ApiId = 12100;

        private const string Vendor1App1ActivationCode = "RTY56";
        private const string Vendor2App1ActivationCode = "ASDF98";

        [TestFixtureSetUp]
        public void Setup()
        {
            // since we are testing applications, vendor should be initialized prior to application. Otherwise
            // the navigation property of Application.Vendor won't work right. 

            // mock order: User -> Vendor -> ApiClient -> Application

            _userData = GetUserData();
            SetupDbSet(UserDbSet, _userData);

            _vendorData = GetVendorData();
            SetupDbSet(VendorDbSet, _vendorData);

            _apiClientData = GetApiClientData();
            SetupDbSet(ApiClientDbSet, _apiClientData);

            _applicationData = GetApplicationData();
            SetupDbSet(ApplicationDbSet, _applicationData);

            Context.Stub(x => x.Users).PropertyBehavior();
            Context.Stub(x => x.Applications).PropertyBehavior();
            Context.Stub(x => x.Vendors).PropertyBehavior();
            Context.Stub(x => x.Clients).PropertyBehavior();

            Context.Users = UserDbSet;
            Context.Applications = ApplicationDbSet;
            Context.Vendors = VendorDbSet;
            Context.Clients = ApiClientDbSet;
        }

        private List<VendorEntity> GetVendorData()
        {
            return new List<VendorEntity>
            {
                new VendorEntity
                {
                    VendorId = Vendor1Id,
                    VendorName = "Vendor 1",
                    Users = UserDbSet.Where(u => u.UserId == Vendor1UserId).ToList()
                },
                new VendorEntity
                {
                    VendorId = Vendor2Id,
                    VendorName = "Vendor 2",
                    Users = UserDbSet.Where(u => u.UserId == Vendor2UserId).ToList()
                },
                new VendorEntity
                {
                    VendorId = Vendor3Id,
                    VendorName = "Vendor 3",
                    Users = new List<User>()
                },
            };
        }

        private List<Application> GetApplicationData()
        {
            return new List<Application>
            {
                new Application
                {
                    ApplicationId = Vendor1App1Id,
                    ApplicationName = "ApplicationName 1",
                    ApiClients = ApiClientDbSet.Where(a => a.ApiClientId == Vendor1App1ApiId).ToList(),
                    Vendor = VendorDbSet.Single(v => v.VendorId == Vendor1Id),
                },
                new Application
                {
                    ApplicationId = Vendor1App2Id,
                    ApplicationName = "ApplicationName 2",
                    ApiClients = ApiClientDbSet.Where(a => a.ApiClientId == Vendor1App2ApiId).ToList(),
                    Vendor = VendorDbSet.Single(v => v.VendorId == Vendor1Id),
                },
                new Application
                {
                    ApplicationId = Vendor2App1Id,
                    ApplicationName = "ApplicationName 3",
                    ApiClients = ApiClientDbSet.Where(a => a.ApiClientId == Vendor2App1ApiId).ToList(),
                    Vendor = VendorDbSet.Single(v => v.VendorId == Vendor2Id),
                },
            };
        }

        private List<ApiClient> GetApiClientData()
        {
            return new List<ApiClient>
            {
                new ApiClient
                {
                    ApiClientId = Vendor1App1ApiId,
                    ChallengeExpiry = new DateTime(2000, 01, 01, 00, 00, 00),
                    User = UserDbSet.Single(u => u.UserId == Vendor1UserId),
                    UseSandbox = false,
                    ActivationCode = Vendor1App1ActivationCode,
                    KeyStatus = KeyStatus.Sent,
                },
                new ApiClient
                {
                    ApiClientId = Vendor1App2ApiId,
                    ChallengeExpiry = null,
                    User = UserDbSet.Single(u => u.UserId == Vendor1UserId),
                    UseSandbox = false
                },
                new ApiClient
                {
                    ApiClientId = Vendor2App1ApiId,
                    User = UserDbSet.Single(u => u.UserId == Vendor2UserId),
                    UseSandbox = false,
                    ChallengeExpiry = DateTime.Now.AddHours(1),
                    ActivationCode = Vendor2App1ActivationCode,
                    KeyStatus = KeyStatus.Initialized,
                },
            };
        }

        private static List<User> GetUserData()
        {
            return new List<User>
            {
                new User {FullName = "User 1", UserId = Vendor1UserId},
                new User {FullName = "User 2", UserId = Vendor2UserId},
            };
        }

        [Test]
        public void Should_return_vendors_all_applications_by_calling_GetAll()
        {
            ApplicationService.GetVendorApplications(Vendor1Id).Count()
                .ShouldEqual(2);
        }

        [Test]
        public void Should_return_correct_application_calling_by_id()
        {
            ApplicationService.GetById(Vendor1Id, Vendor1App1Id)
                .ShouldNotEqual(null);
        }
        
        [Test]
        public void Should_return_proper_ActivationCode()
        {
            ApplicationService.GetById(Vendor2Id, Vendor2App1Id).ActivationCode
                .ShouldEqual(Vendor2App1ActivationCode);
        }
        
        [Test]
        public void Should_return_null_calling_by_wrong_id()
        {
            ApplicationService.GetById(Vendor1Id, Vendor2App1Id)
                .ShouldEqual(null);
        }

        [Test]
        public void Should_throw_exception_calling_by_a_vendor_with_no_user_associated_to_it()
        {
            Assert.Throws<DataException>(() =>
                ApplicationService.GetById(Vendor3Id, Vendor2App1Id));
        }

        [Test]
        public void Should_throw_exception_calling_by_wrong_vendor_id()
        {
            Assert.Throws<ArgumentException>(() =>
                ApplicationService.GetById(NotExistingVendorId, Vendor1App1Id));
        }

        [Test]
        public void Should_return_Expired_as_KeyStatus_for_the_applications_that_their_challenge_is_expired()
        {
            ApplicationService.GetById(Vendor1Id, Vendor1App1Id).KeyStatus
                .ShouldEqual(KeyStatus.Expired);
        }

        [Test]
        public void Should_return_NotSent_as_KeyStatus_for_the_applications_that_their_challenge_is_null()
        {
            ApplicationService.GetById(Vendor2Id, Vendor2App1Id).KeyStatus
                .ShouldEqual(KeyStatus.Initialized);
        }

        [Test]
        public void Should_return_null_as_ActivationCode_for_the_applications_that_their_challenge_is_expired()
        {
            ApplicationService.GetById(Vendor1Id, Vendor1App1Id).ActivationCode
                .ShouldEqual(null);
        }
    }

    [TestFixture]
    public class When_adding_application : ApplicationServiceTestBase
    {
        List<User> _userData;
        List<Application> _applicationData;
        List<VendorEntity> _vendorData;
        List<ApiClient> _apiClientData;

        private const int Vendor1Id = 11;
        private const int Vendor2Id = 12;
        private const int Vendor3Id = 13;
        private const int NotExistingVendorId = 99;
        
        private const int Vendor1UserId = 811;
        private const int Vendor2UserId = 812;

        private const int Vendor1App1Id = 111;
        private const int Vendor1App2Id = 112;
        private const int Vendor2App1Id = 121;

        private const int Vendor1App1ApiId = 11100;
        private const int Vendor1App2ApiId = 11200;
        private const int Vendor2App1ApiId = 12100;

        private const string Vendor1App1ActivationCode = "RTY56";
        private const string Vendor2App1ActivationCode = "ASDF98";

        [TestFixtureSetUp]
        public void Setup()
        {
            // since we are testing applications, vendor should be initialized prior to application. Otherwise
            // the navigation property of Application.Vendor won't work right. 

            // mock order: User -> Vendor -> ApiClient -> Application

            _userData = GetUserData();
            SetupDbSet(UserDbSet, _userData);

            _vendorData = GetVendorData();
            SetupDbSet(VendorDbSet, _vendorData);

            _apiClientData = GetApiClientData();
            SetupDbSet(ApiClientDbSet, _apiClientData);

            _applicationData = GetApplicationData();
            SetupDbSet(ApplicationDbSet, _applicationData);

            Context.Stub(x => x.Users).PropertyBehavior();
            Context.Stub(x => x.Applications).PropertyBehavior();
            Context.Stub(x => x.Vendors).PropertyBehavior();
            Context.Stub(x => x.Clients).PropertyBehavior();

            Context.Users = UserDbSet;
            Context.Applications = ApplicationDbSet;
            Context.Vendors = VendorDbSet;
            Context.Clients = ApiClientDbSet;

            CredentialService.Stub(x => x.GetNewCredentials()).Return(new Credentials {Key = "11111", Secret = "22222"});
        }

        private List<VendorEntity> GetVendorData()
        {
            return new List<VendorEntity>
            {
                new VendorEntity
                {
                    VendorId = Vendor1Id,
                    VendorName = "Vendor 1",
                    Users = UserDbSet.Where(u => u.UserId == Vendor1UserId).ToList()
                },
                new VendorEntity
                {
                    VendorId = Vendor2Id,
                    VendorName = "Vendor 2",
                    Users = UserDbSet.Where(u => u.UserId == Vendor2UserId).ToList()
                },
                new VendorEntity
                {
                    VendorId = Vendor3Id,
                    VendorName = "Vendor 3",
                    Users = new List<User>()
                },
            };
        }

        private List<Application> GetApplicationData()
        {
            return new List<Application>
            {
                new Application
                {
                    ApplicationId = Vendor1App1Id,
                    ApplicationName = "ApplicationName 1",
                    ApiClients = ApiClientDbSet.Where(a => a.ApiClientId == Vendor1App1ApiId).ToList(),
                    Vendor = VendorDbSet.Single(v => v.VendorId == Vendor1Id),
                },
                new Application
                {
                    ApplicationId = Vendor1App2Id,
                    ApplicationName = "ApplicationName 2",
                    ApiClients = ApiClientDbSet.Where(a => a.ApiClientId == Vendor1App2ApiId).ToList(),
                    Vendor = VendorDbSet.Single(v => v.VendorId == Vendor1Id),
                },
                new Application
                {
                    ApplicationId = Vendor2App1Id,
                    ApplicationName = "ApplicationName 3",
                    ApiClients = ApiClientDbSet.Where(a => a.ApiClientId == Vendor2App1ApiId).ToList(),
                    Vendor = VendorDbSet.Single(v => v.VendorId == Vendor2Id),
                },
            };
        }

        private List<ApiClient> GetApiClientData()
        {
            return new List<ApiClient>
            {
                new ApiClient
                {
                    ApiClientId = Vendor1App1ApiId,
                    ChallengeExpiry = new DateTime(2000, 01, 01, 00, 00, 00),
                    User = UserDbSet.Single(u => u.UserId == Vendor1UserId),
                    UseSandbox = false,
                    ActivationCode = Vendor1App1ActivationCode,
                    KeyStatus = KeyStatus.Sent,
                },
                new ApiClient
                {
                    ApiClientId = Vendor1App2ApiId,
                    ChallengeExpiry = null,
                    User = UserDbSet.Single(u => u.UserId == Vendor1UserId),
                    UseSandbox = false
                },
                new ApiClient
                {
                    ApiClientId = Vendor2App1ApiId,
                    User = UserDbSet.Single(u => u.UserId == Vendor2UserId),
                    UseSandbox = false,
                    ChallengeExpiry = DateTime.Now.AddHours(1),
                    ActivationCode = Vendor2App1ActivationCode,
                    KeyStatus = KeyStatus.Initialized,
                },
            };
        }

        private static List<User> GetUserData()
        {
            return new List<User>
            {
                new User {FullName = "User 1", UserId = Vendor1UserId},
                new User {FullName = "User 2", UserId = Vendor2UserId},
            };
        }

        [Test]
        public void Should_throw_exception_calling_by_wrong_id()
        {
            Assert.Throws<ArgumentException>(() => 
                ApplicationService.AddApplication(NotExistingVendorId, new Model.Application()));
        }

        [Test]
        public void Should_throw_exception_calling_by_a_vendor_with_no_user_associated_to_it()
        {
            Assert.Throws<DataException>(() =>
                ApplicationService.AddApplication(Vendor3Id, new Model.Application()));
        }

        [Test]
        public void Should_throw_error_if_an_application_with_the_same_name_exists_within_the_vendor_applications()
        {
            Assert.Throws<DuplicateNameException>(() =>
                ApplicationService.AddApplication(Vendor1Id, new Model.Application
                {
                    ApplicationName = "ApplicationName 1"
                }));
        }

        [Test]
        public void Should_not_throw_error_duplicate_name_for_different_vendors()
        {
            ApplicationService.AddApplication(Vendor2Id, new Model.Application
            {
                ApplicationName = "ApplicationName 1",
                EducationOrganizations = new List<EducationOrganization>(),
                AssociatedProfiles = new List<Model.Profile>()
            });

            Assert.Pass();
        }
    }

    [TestFixture]
    public class When_generating_key_for_an_application : ApplicationServiceTestBase
    {
        List<User> _userData;
        List<Application> _applicationData;
        List<VendorEntity> _vendorData;
        List<ApiClient> _apiClientData;

        private const int Vendor1Id = 11;
        private const int Vendor2Id = 12;
        private const int Vendor3Id = 13;
        private const int NotExistingVendorId = 99;
        
        private const int Vendor1UserId = 811;
        private const int Vendor2UserId = 812;

        private const int Vendor1App1Id = 111;
        private const int Vendor1App2Id = 112;
        private const int Vendor2App1Id = 121;

        private const int Vendor1App1ApiId = 11100;
        private const int Vendor1App2ApiId = 11200;
        private const int Vendor2App1ApiId = 12100;

        private const string Vendor1App1ActivationCode = "RTY56";
        private const string Vendor2App1ActivationCode = "ASDF98";

        [TestFixtureSetUp]
        public void Setup()
        {
            // since we are testing applications, vendor should be initialized prior to application. Otherwise
            // the navigation property of Application.Vendor won't work right. 

            // mock order: User -> Vendor -> ApiClient -> Application

            _userData = GetUserData();
            SetupDbSet(UserDbSet, _userData);

            _vendorData = GetVendorData();
            SetupDbSet(VendorDbSet, _vendorData);

            _apiClientData = GetApiClientData();
            SetupDbSet(ApiClientDbSet, _apiClientData);

            _applicationData = GetApplicationData();
            SetupDbSet(ApplicationDbSet, _applicationData);

            Context.Stub(x => x.Users).PropertyBehavior();
            Context.Stub(x => x.Applications).PropertyBehavior();
            Context.Stub(x => x.Vendors).PropertyBehavior();
            Context.Stub(x => x.Clients).PropertyBehavior();

            Context.Users = UserDbSet;
            Context.Applications = ApplicationDbSet;
            Context.Vendors = VendorDbSet;
            Context.Clients = ApiClientDbSet;
        }

        private List<VendorEntity> GetVendorData()
        {
            return new List<VendorEntity>
            {
                new VendorEntity
                {
                    VendorId = Vendor1Id,
                    VendorName = "Vendor 1",
                    Users = UserDbSet.Where(u => u.UserId == Vendor1UserId).ToList()
                },
                new VendorEntity
                {
                    VendorId = Vendor2Id,
                    VendorName = "Vendor 2",
                    Users = UserDbSet.Where(u => u.UserId == Vendor2UserId).ToList()
                },
                new VendorEntity
                {
                    VendorId = Vendor3Id,
                    VendorName = "Vendor 3",
                    Users = new List<User>()
                },
            };
        }

        private List<Application> GetApplicationData()
        {
            return new List<Application>
            {
                new Application
                {
                    ApplicationId = Vendor1App1Id,
                    ApplicationName = "ApplicationName 1",
                    ApiClients = ApiClientDbSet.Where(a => a.ApiClientId == Vendor1App1ApiId).ToList(),
                    Vendor = VendorDbSet.Single(v => v.VendorId == Vendor1Id),
                },
                new Application
                {
                    ApplicationId = Vendor1App2Id,
                    ApplicationName = "ApplicationName 2",
                    ApiClients = ApiClientDbSet.Where(a => a.ApiClientId == Vendor1App2ApiId).ToList(),
                    Vendor = VendorDbSet.Single(v => v.VendorId == Vendor1Id),
                },
                new Application
                {
                    ApplicationId = Vendor2App1Id,
                    ApplicationName = "ApplicationName 3",
                    ApiClients = ApiClientDbSet.Where(a => a.ApiClientId == Vendor2App1ApiId).ToList(),
                    Vendor = VendorDbSet.Single(v => v.VendorId == Vendor2Id),
                },
            };
        }

        private List<ApiClient> GetApiClientData()
        {
            return new List<ApiClient>
            {
                new ApiClient
                {
                    ApiClientId = Vendor1App1ApiId,
                    ChallengeExpiry = new DateTime(2000, 01, 01, 00, 00, 00),
                    User = UserDbSet.Single(u => u.UserId == Vendor1UserId),
                    UseSandbox = false,
                    ActivationCode = Vendor1App1ActivationCode,
                    KeyStatus = KeyStatus.Sent,
                },
                new ApiClient
                {
                    ApiClientId = Vendor1App2ApiId,
                    ChallengeExpiry = null,
                    User = UserDbSet.Single(u => u.UserId == Vendor1UserId),
                    UseSandbox = false
                },
                new ApiClient
                {
                    ApiClientId = Vendor2App1ApiId,
                    User = UserDbSet.Single(u => u.UserId == Vendor2UserId),
                    UseSandbox = false,
                    ChallengeExpiry = DateTime.Now.AddHours(1),
                    ActivationCode = Vendor2App1ActivationCode,
                    KeyStatus = KeyStatus.Initialized,
                },
            };
        }

        private static List<User> GetUserData()
        {
            return new List<User>
            {
                new User {FullName = "User 1", UserId = Vendor1UserId},
                new User {FullName = "User 2", UserId = Vendor2UserId},
            };
        }

        [Test]
        public void Should_throw_exception_calling_by_wrong_id()
        {
            Assert.Throws<ArgumentException>(() => 
                ApplicationService.GenerateApplicationKey(NotExistingVendorId, -1));
        }

        [Test]
        public void Should_throw_exception_calling_by_a_vendor_with_no_user_associated_to_it()
        {
            Assert.Throws<DataException>(() =>
                ApplicationService.GenerateApplicationKey(Vendor3Id, -1));
        }

        [Test]
        public void Should_throw_exception_calling_with_the_wrong_application_id()
        {
            Assert.Throws<ArgumentException>(() =>
                ApplicationService.GenerateApplicationKey(Vendor1Id, -1));
        }
    }
}
