using System;
using System.Linq;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.Admin.Services;
using EdFi.Ods.Admin.Tests.Models;
using NCrunch.Framework;
using NUnit.Framework;
using Test.Common;
using Should;
using Rhino.Mocks;
using System.Data.Entity;

namespace EdFi.Ods.Admin.Tests.Services
{
    public class DefaultApplicationCreatorTests
    {
        [TestFixture]
        [ExclusivelyUses(TestSingletons.EmptyAdminDatabase)]
        public class When_default_application_does_not_exist_for_vendor_and_sandbox_type : UserContextTestBase
        {
            private Application _createdApplication;
            private Application _loadedApplication;

            private string vendorName;
            private int leaId;

            [TestFixtureSetUp]
            public void Setup()
            {
                vendorName = string.Format("{0}_TestData", DateTime.Now.Ticks);
                leaId = int.MaxValue - 1;

                DeleteApplicationEducationOrganization(leaId);
                DeleteVendor(vendorName);

                var leaQuery = Stub<IDatabaseTemplateLeaQuery>();
                leaQuery.Stub(x => x.GetLocalEducationAgencyIds(SandboxType.Sample)).Return(new[] { leaId });

                using (var context = new UsersContext())
                {
                    var vendor = new Vendor { VendorName = vendorName };
                    context.Vendors.Add(vendor);
                    context.SaveChanges();

                    var creator = new DefaultApplicationCreator(context, leaQuery);
                    _createdApplication = creator.FindOrCreateUpdatedDefaultSandboxApplication(vendor.VendorId, SandboxType.Sample);
                    context.SaveChanges();

                    _loadedApplication =
                        context.Applications.Where(
                            a => a.ApplicationName == _createdApplication.ApplicationName && a.Vendor.VendorName == vendorName)
                            .Include(x => x.ApplicationEducationOrganizations).Single();
                }
            }

            [Test]
            public void Should_create_default_application()
            {
                _createdApplication.ApplicationName.ShouldEqual("Default Sandbox Application Sample");
            }

            [Test]
            public void Should_associate_application_with_vendor()
            {
                _createdApplication.Vendor.VendorName.ShouldEqual(vendorName);
            }

            [Test]
            public void Should_associate_all_available_LEAs_with_application()
            {
                var leas = _loadedApplication.ApplicationEducationOrganizations.Select(x => x.EducationOrganizationId).ToArray();
                leas.Length.ShouldEqual(1);
                leas.ShouldContain(leaId);
            }

            [TestFixtureTearDown]
            public void TearDown()
            {
                using (var context = new UsersContext())
                {
                    DeleteApplicationEducationOrganization(leaId);
                    DeleteApplication(_createdApplication.ApplicationName);
                    DeleteVendor(vendorName);
                }
            }
        }

        [TestFixture]
        [ExclusivelyUses(TestSingletons.EmptyAdminDatabase)]
        public class When_default_application_exists_for_vendor_and_sandbox_type_and_application_is_missing_an_LEA_association : UserContextTestBase
        {
            private Application _loadedApplication;
            private Application _foundApplication;
            private Application[] _applications;

            private string vendorName;
            private int leaId1;
            private int leaId2;

            [TestFixtureSetUp]
            public void Setup()
            {
                vendorName = string.Format("{0}_TestData", DateTime.Now.Ticks);
                leaId1 = int.MaxValue - 1;
                leaId2 = int.MaxValue - 2;

                DeleteApplicationEducationOrganization(leaId1);
                DeleteApplicationEducationOrganization(leaId2);
                DeleteVendor(vendorName);

                var leaQuery = Stub<IDatabaseTemplateLeaQuery>();
                leaQuery.Stub(x => x.GetLocalEducationAgencyIds(SandboxType.Sample)).Return(new[] { leaId1, leaId2 });

                using (var context = new UsersContext())
                {
                    var vendor = new Vendor { VendorName = vendorName };
                    var application = vendor.CreateApplication("Default Sandbox Application Sample");
                    application.CreateEducationOrganizationAssociation(leaId1);
                    context.Vendors.Add(vendor);
                    context.SaveChanges();

                    var creator = new DefaultApplicationCreator(context, leaQuery);
                    _foundApplication = creator.FindOrCreateUpdatedDefaultSandboxApplication(vendor.VendorId, SandboxType.Sample);
                    context.SaveChanges();

                    _applications = context.Applications.Where(a => a.Vendor.VendorName == vendorName).Include(x => x.ApplicationEducationOrganizations).ToArray();
                    _loadedApplication = _applications.FirstOrDefault();
                }
            }

            [Test]
            public void Should_find_existing_default_application()
            {
                _applications.Length.ShouldEqual(1);

                _foundApplication.ShouldNotBeNull();
                _foundApplication.ApplicationName.ShouldEqual("Default Sandbox Application Sample");
                _foundApplication.Vendor.VendorName.ShouldEqual(vendorName);
            }

            [Test]
            public void Should_add_lea_association_that_was_missing()
            {
                var leaIds = _loadedApplication.ApplicationEducationOrganizations.Select(x => x.EducationOrganizationId).ToArray();
                leaIds.Length.ShouldEqual(2);
                leaIds.ShouldContain(leaId1);
                leaIds.ShouldContain(leaId2);
            }

            [TestFixtureTearDown]
            public void TearDown()
            {
                using (var context = new UsersContext())
                {
                    DeleteApplicationEducationOrganization(leaId1);
                    DeleteApplicationEducationOrganization(leaId2);
                    DeleteApplication(_foundApplication.ApplicationName);
                    DeleteVendor(vendorName);
                }
            }
        }
    }
}