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
using Application = EdFi.Ods.Admin.Models.Application;
using Profile = EdFi.Ods.Admin.Models.Profile;
using VendorEntity = EdFi.Ods.Admin.Models.Vendor;
using VendorModel = EdFi.Ods.SecurityConfiguration.Services.Model.Vendor;

// ReSharper disable InconsistentNaming
namespace EdFi.Ods.SecurityConfiguration.Services.Tests
{
    public abstract class VendorServiceTestBase
    {
        protected readonly IUsersContext Context;

        protected IDbSet<VendorEntity> VendorDbSet;
        protected IDbSet<User> UserDbSet { get; set; }
        protected IDbSet<Application> ApplicationDbSet { get; set; }
        protected IDbSet<Profile> ProfileDbSet { get; set; }

        protected readonly VendorService VendorService;

        protected VendorServiceTestBase()
        {
            Context = MockRepository.GenerateMock<IUsersContext>();
            VendorDbSet = MockRepository.GenerateMock<IDbSet<VendorEntity>, IQueryable>();
            UserDbSet = MockRepository.GenerateMock<IDbSet<User>, IQueryable>();
            ApplicationDbSet = MockRepository.GenerateMock<IDbSet<Application>, IQueryable>();
            ProfileDbSet = MockRepository.GenerateMock<IDbSet<Profile>, IQueryable>();

            var adminContextFactory = MockRepository.GenerateMock<IAdminContextFactory>();
            adminContextFactory.Stub(x => x.Create()).Return(Context);

            VendorService = new VendorService(adminContextFactory);
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
    public class When_retrieve_vendors :VendorServiceTestBase
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            var vendorData = GetVendorData();

            SetupDbSet(VendorDbSet, vendorData);
            Context.Stub(x => x.Vendors).PropertyBehavior();
            Context.Vendors = VendorDbSet;
        }

        private static List<VendorEntity> GetVendorData()
        {
            return new List<VendorEntity> 
            { 
                new VendorEntity { VendorId = 1, VendorName = "Vendor 1" }, 
                new VendorEntity { VendorId = 2, VendorName = "Vendor 2" }, 
                new VendorEntity { VendorId = 3, VendorName = "Vendor 3" }, 
                new VendorEntity { VendorId = 4, VendorName = "Vendor 4" }, 
            };
        }

        [Test]
        public void Should_return_all_vendors_by_calling_GetAll()
        {
            var result = VendorService.GetAll();

            Assert.That(result.Count(), Is.EqualTo(4));
        }

        [Test]
        public void Should_return_single_vendor_when_called_by_id()
        {
            var result = VendorService.GetById(2);

            result.ShouldNotEqual(null);
            result.VendorName.ShouldEqual("Vendor 2");
        }

        [Test]
        public void Should_return_null_when_called_by_invalid_id()
        {
            var result = VendorService.GetById(100);
            result.ShouldEqual(null);
        }

        [Test]
        public void Should_return_error_trying_enter_duplicate_name()
        {
            var newVendor = new VendorModel
            {
                VendorName = "Vendor 1",
                MainContact = new VendorContact {ContactName = "Contact Name", ContactEmailAddress = "Email"}
            };

            Assert.Throws<DuplicateNameException>(() => VendorService.AddVendor(newVendor));
        }
    }

    [TestFixture]
    public class When_update_a_vendor :VendorServiceTestBase
    {
        private List<VendorEntity> _vendorData;

        [TestFixtureSetUp]
        public void Setup()
        {
            _vendorData = GetVendorData();

            SetupDbSet(VendorDbSet, _vendorData);
            Context.Stub(x => x.Vendors).PropertyBehavior();
            Context.Vendors = VendorDbSet;
        }

        private static List<VendorEntity> GetVendorData()
        {
            return new List<VendorEntity>
            {
                new VendorEntity
                {
                    VendorId = 111,
                    VendorName = "Vendor 1",
                    Users = new List<User>
                    {
                        new User{ UserId = 1, FullName = "name 1", Email = "email 1"},
                    },
                    NamespacePrefix = "prefix 1",
                },
            };
        }

        [Test]
        public void Should_update_vendor_entity()
        {
            const string newName = "Vendor 666777";
            const string newPrefix = "Prefix 666777";

            var vendor = VendorService.GetById(111);
            vendor.VendorName = newName;
            vendor.NamespacePrefix = newPrefix;

            VendorService.UpdateVendor(vendor);

            var entity = _vendorData.First();

            Assert.That(entity.VendorName, Is.EqualTo(newName));
            Assert.That(entity.NamespacePrefix, Is.EqualTo(newPrefix));
        }

        [Test]
        public void Should_update_underlying_user_entity()
        {
            const string newName = "Name 65497";
            const string newEmail = "Email 65497";

            var vendor = VendorService.GetById(111);

            vendor.MainContact.ContactName = newName;
            vendor.MainContact.ContactEmailAddress = newEmail;

            VendorService.UpdateVendor(vendor);

            var entity = _vendorData.First();

            Assert.That(entity.Users.First().FullName, Is.EqualTo(newName));
            Assert.That(entity.Users.First().Email, Is.EqualTo(newEmail));
        }
    }

    [TestFixture]
    public class When_deleting_a_vendor : VendorServiceTestBase
    {
            List<User> _userData;
            List<Application> _applicationData;
            List<VendorEntity> _vendorData;
            List<Profile> _profileData;

        [TestFixtureSetUp]
        public void Setup()
        {
            _userData = GetUserData();
            SetupDbSet(UserDbSet, _userData);

            _profileData = GetProfilesData();
            SetupDbSet(ProfileDbSet, _profileData);

            _applicationData = GetApplicationData();
            SetupDbSet(ApplicationDbSet, _applicationData);

            _vendorData = GetVendorData();
            SetupDbSet(VendorDbSet, _vendorData);
            
            Context.Stub(x => x.Users).PropertyBehavior();
            Context.Stub(x => x.Applications).PropertyBehavior();
            Context.Stub(x => x.Vendors).PropertyBehavior();

            Context.Users = UserDbSet;
            Context.Applications = ApplicationDbSet;
            Context.Vendors = VendorDbSet;
            Context.Profiles = ProfileDbSet;


            // delete a vendor
            VendorService.DeleteVendor(1);
        }

        private List<VendorEntity> GetVendorData()
        {
            return new List<VendorEntity>
            {
                new VendorEntity
                {
                    VendorId = 1,
                    VendorName = "Vendor 1",
                    Applications = ApplicationDbSet.Where(a => a.ApplicationId == 1 || a.ApplicationId == 2).ToList(),
                    Users = UserDbSet.Where(u => u.UserId == 1).ToList()
                },
                new VendorEntity
                {
                    VendorId = 2,
                    VendorName = "Vendor 2",
                    Applications = ApplicationDbSet.Where(a => a.ApplicationId == 3).ToList(),
                    Users = UserDbSet.Where(u => u.UserId == 2).ToList()
                },
            };
        }

        private List<Application> GetApplicationData()
        {
            return new List<Application>
            {
                new Application
                {
                    ApplicationId = 1,
                    ApplicationName = "ApplicationName 1",
                    Profiles = ProfileDbSet.Where(p => p.ProfileId == 1).ToList()
                },
                new Application
                {
                    ApplicationId = 2,
                    ApplicationName = "ApplicationName 2",
                    Profiles = ProfileDbSet.Where(p => p.ProfileId == 2 || p.ProfileId == 3).ToList()
                },
                new Application
                {
                    ApplicationId = 3,
                    ApplicationName = "ApplicationName 3",
                    Profiles = ProfileDbSet.Where(p => p.ProfileId == 1 || p.ProfileId == 4).ToList()
                },
            };
        }

        private static List<Profile> GetProfilesData()
        {
            return new List<Profile>
            {
                new Profile {ProfileId = 1, ProfileName = "Profile 1"},
                new Profile {ProfileId = 2, ProfileName = "Profile 2"},
                new Profile {ProfileId = 3, ProfileName = "Profile 3"},
                new Profile {ProfileId = 4, ProfileName = "Profile 4"},
            };
        }

        private static List<User> GetUserData()
        {
            return new List<User>
            {
                new User {FullName = "User 1", UserId = 1},
                new User {FullName = "User 2", UserId = 2},
            };
        }

        [Test]
        public void Should_remove_associated_applications()
        {
            _applicationData.Count().ShouldEqual(1);
        }

        [Test]
        public void Should_not_remove_associated_users()
        {
            _userData.Count().ShouldEqual(2);
        }

        [Test]
        public void Should_not_remove_associated_profiles()
        {
            _profileData.Count().ShouldEqual(4);
        }
    }
}
