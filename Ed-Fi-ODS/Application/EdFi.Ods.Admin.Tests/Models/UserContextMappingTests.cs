using System;
using System.Data.Entity;
using System.Linq;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.IntegrationTests._Extensions;
using NCrunch.Framework;
using NUnit.Framework;
using Should;
using Test.Common;

namespace EdFi.Ods.Admin.Tests.Models
{
    public class UserContextMappingTests
    {
        [TestFixture]
        [ExclusivelyUses(TestSingletons.EmptyAdminDatabase)]
        public class When_creating_a_user : UserContextTestBase
        {
            private string emailAddress;

            [TestFixtureSetUp]
            public void Setup()
            {
                emailAddress = string.Format("{0}@{1}.com", DateTime.Now.Ticks, (DateTime.Now.Ticks + 1));
            }

            [Test]
            public void Should_persist_the_user_to_the_database()
            {
                using (var context = new UsersContext())
                {
                    //Arrange
                    DeleteUser(emailAddress);

                    //Act
                    var user = new User { Email = emailAddress };
                    context.Users.Add(user);
                    context.SaveChangesForTest();
                    
                    //Assert
                    context.Users.Count(x => x.Email == emailAddress).ShouldEqual(1);
                }
            }
        }

        [TestFixture]
        [ExclusivelyUses(TestSingletons.EmptyAdminDatabase)]
        public class When_adding_an_lea_mapping_to_a_client : UserContextTestBase
        {
            private string clientName;
            private int leaId;
            
            [TestFixtureSetUp]
            public void Setup()
            {
                clientName = string.Format("{0}_TestData", DateTime.Now.Ticks);
                leaId = int.MaxValue - 1;
            }

            [Test]
            public void Should_persist_the_lea_mapping_without_explicitly_adding_that_mapping_to_the_databaseContext()
            {
                using (var context = new UsersContext())
                {
                    //Arrange
                    DeleteClient(clientName);
                    DeleteApplicationEducationOrganization(leaId);

                    var lea = new ApplicationEducationOrganization { EducationOrganizationId = leaId };

                    var client = new ApiClient(true) { Name = clientName };
                    client.ApplicationEducationOrganizations.Add(lea);

                    //Act
                    context.Clients.Add(client);
                    context.SaveChangesForTest();

                    //Assert
                    var clientFromDb = context.Clients.Where(x => x.Name == clientName)
                                        .Include(x => x.ApplicationEducationOrganizations)
                                        .Single();

                    int[] leas = clientFromDb.ApplicationEducationOrganizations.Select(x => x.EducationOrganizationId).ToArray();

                    leas.ShouldEqual(new[] { leaId });
                }
            }
        }

        [TestFixture]
        [ExclusivelyUses(TestSingletons.EmptyAdminDatabase)]
        public class When_adding_an_lea_mapping_to_an_application : UserContextTestBase
        {
            private string appName;
            private int leaId;

            [TestFixtureSetUp]
            public void Setup()
            {
                appName = string.Format("{0}_TestData", DateTime.Now.Ticks);
                leaId = int.MaxValue - 1;
            }

            [Test]
            public void Should_persist_the_lea_mapping_without_explicitly_adding_that_mapping_to_the_databaseContext()
            {
                using (var context = new UsersContext())
                {
                    //Arrange
                    DeleteApplicationEducationOrganization(leaId);
                    DeleteApplication(appName);
                    
                    var lea = new ApplicationEducationOrganization { EducationOrganizationId = leaId };

                    var application = new Application { ApplicationName = appName };
                    application.ApplicationEducationOrganizations.Add(lea);

                    //Act
                    context.Applications.Add(application);
                    context.SaveChangesForTest();
                    
                    //Assert
                    var applicationFromDb = context.Applications.Where(x => x.ApplicationName == appName)
                                             .Include(x => x.ApplicationEducationOrganizations)
                                             .Single();

                    int[] leas = applicationFromDb.ApplicationEducationOrganizations.Select(x => x.EducationOrganizationId).ToArray();

                    leas.ShouldEqual(new[] {leaId});
                }
            }
        }

        [TestFixture]
        [ExclusivelyUses(TestSingletons.EmptyAdminDatabase)]
        public class When_adding_an_application_to_a_vendor : UserContextTestBase
        {
            private string vendorName;
            private string appName;
            
            [TestFixtureSetUp]
            public void Setup()
            {
                vendorName = string.Format("{0}_TestData", DateTime.Now.Ticks);
                appName = string.Format("{0}_TestData", DateTime.Now.Ticks);
            }

            [Test]
            public void Should_create_application()
            {
                //Arrange
                DeleteApplication(appName);
                DeleteVendor(vendorName);

                var vendor = new Vendor { VendorName = vendorName };
                vendor.CreateApplication(appName);

                using (var context = new UsersContext())
                {
                    context.Vendors.Add(vendor);
                    context.SaveChangesForTest();

                    //Act
                    var vendorFromDb = context.Vendors.Where(v => v.VendorName == vendorName).Include(x => x.Applications).Single();
                    
                    //Assert
                    vendorFromDb.ShouldNotBeNull();
                    vendorFromDb.Applications.Count.ShouldEqual(1);
                    vendorFromDb.Applications.ToList()[0].ApplicationName.ShouldEqual(appName);
                }
            }
        }

        [TestFixture]
        public class When_adding_a_local_education_agency_to_an_application : UserContextTestBase
        {
            private string vendorName;
            private string appName;
            private int leaId;

            [TestFixtureSetUp]
            public void Setup()
            {
                vendorName = string.Format("{0}_TestData", DateTime.Now.Ticks);
                appName = string.Format("{0}_TestData", DateTime.Now.Ticks);
                leaId = int.MaxValue - 1;
            }

            [Test]
            public void Should_create_lea_association()
            {
                //Arrange
                DeleteApplicationEducationOrganization(leaId);
                DeleteApplication(appName);
                DeleteVendor(vendorName);

                var vendor = new Vendor { VendorName = vendorName };
                vendor.CreateApplication(appName);
                vendor.Applications.AsEnumerable().ElementAt(0).CreateEducationOrganizationAssociation(leaId);

                using (var context = new UsersContext())
                {
                    context.Vendors.Add(vendor);
                    context.SaveChangesForTest();

                    //Act
                    var application = context.Applications.Where(app => app.ApplicationName == appName).Include(x => x.ApplicationEducationOrganizations).Single();

                    var applicationLocalEducationAgencies = application.ApplicationEducationOrganizations.ToArray();
                    applicationLocalEducationAgencies.Length.ShouldEqual(1);
                    applicationLocalEducationAgencies[0].EducationOrganizationId.ShouldEqual(leaId);
                }                
            }
        }
    }
}