using System;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.IntegrationTests._Extensions;
using NUnit.Framework;

namespace EdFi.Ods.Admin.Tests.Models
{
    using EdFi.Ods.Tests._Bases;

    public abstract class UserContextTestBase : TestBase
    {
        private TransactionScope _transaction;

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            using (var usersContext = new UsersContext())
            {
                usersContext.Database.Delete();
                usersContext.Database.Initialize(false);
            }
        }

        [SetUp]
        public void Setup()
        {
            _transaction = new TransactionScope();
        }

        [TearDown]
        public void TearDown()
        {
            _transaction.Dispose();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            using (var usersContext = new UsersContext())
            {
                usersContext.Database.Delete();
            }
        }

        protected void DeleteUser(string emailAddress)
        {
            Delete((context => context.Users), (context => context.Users.Where(u => u.Email == emailAddress)));
        }

        protected void DeleteClient(string clientName)
        {
            Delete((context => context.Clients), (context => context.Clients.Where(c => c.Name == clientName)));
        }

        protected void DeleteApplicationEducationOrganization(int educationOrganizationId)
        {
            Delete((context => context.ApplicationEducationOrganizations), (context => context.ApplicationEducationOrganizations.Where(aeo => aeo.EducationOrganizationId == educationOrganizationId)));
        }

        protected void DeleteApplication(string appName)
        {
            Delete((context => context.Applications), (context => context.Applications.Where(app => app.ApplicationName == appName)));
        }

        protected void DeleteVendor(string vendorName)
        {
            Delete((context => context.Vendors), (context => context.Vendors.Where(app => app.VendorName == vendorName)));
        }

        protected void Delete<T>(Func<UsersContext, IDbSet<T>> dbObject, Func<UsersContext, IQueryable<T>> filter) where T : class
        {
            using (var context = new UsersContext())
            {
                foreach (var tDelete in filter(context))
                {
                    dbObject(context).Remove(tDelete);
                }
                context.SaveChangesForTest();
            }
        }
    }
}