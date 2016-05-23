using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using EdFi.Common.Extensions;
using EdFi.Ods.Admin.Models.Extensions;
//using EdFi.Ods.Admin.Models.Models;
using EdFi.Ods.Common.Utils.Extensions;

namespace EdFi.Ods.Admin.Models
{
    public interface IUsersContext : IDisposable
    {
        IDbSet<User> Users { get; set; }
        IDbSet<Vendor> Vendors { get; set; }
        IDbSet<Application> Applications { get; set; }
        IDbSet<Profile> Profiles { get; set; }
        IDbSet<ApplicationEducationOrganization> ApplicationEducationOrganizations { get; set; }
        IDbSet<ApiClient> Clients { get; set; }

        int SaveChanges();
    }
    public class UsersContext : DbContext, IUsersContext
    {
        public UsersContext()
            : base("EdFi_Admin")
        {
        }

        public IDbSet<User> Users { get; set; }
        public IDbSet<ApiClient> Clients { get; set; }
        public DbSet<ClientAccessToken> ClientAccessTokens { get; set; }
        public DbSet<ClientAuthorizationCode> ClientAuthorizationCodes { get; set; }
        public IDbSet<Vendor> Vendors { get; set; }
        public IDbSet<Application> Applications { get; set; }
        public IDbSet<Profile> Profiles { get; set; }

        //TODO:  This should really be removed from being directly on the context.  Application should own
        //TODO:  these instances, and deleting an application should delete the associated LEA's
        public IDbSet<ApplicationEducationOrganization> ApplicationEducationOrganizations { get; set; }

        public static string UserTableName
        {
            get { return new UsersContext().GetTableName<User>().Replace("[dbo].[", "").Replace("]", ""); }
        }

        public static string UserNameColumn
        {
            get
            {
                return UserMemberName(x => x.Email);
            }
        }

        public static string UserIdColumn
        {
            get
            {
                return UserMemberName(x => x.UserId);
            }
        }

        private static string UserMemberName(Expression<Func<User, object>> emailExpression)
        {
            return emailExpression.MemberName();
        }
    }


    public sealed class Configuration : DbMigrationsConfiguration<UsersContext>
    {
        protected override void Seed(UsersContext context)
        {
            base.Seed(context);
            SeedVendors(context);
        }

        private void SeedVendors(UsersContext context)
        {
            var users = context.Users.AsEnumerable();
            foreach (var user in users)
            {
                var vendorName = user.FullName.Split(',')[0].Trim();
                var namePrefix = "http://" + user.Email.Split('@')[1].ToLower();
                var vendor = context.Vendors.SingleOrDefault(v => v.VendorName == vendorName);
                if (vendor == null)
                {
                    vendor = new Vendor { VendorName = vendorName, NamespacePrefix = namePrefix };
                    context.Vendors.AddOrUpdate(vendor);
                }
                SeedDefaultApplications(vendor, context);
            }
            context.SaveChanges();
            foreach (var user in context.Users.Where(u => u.Vendor == null).ToList())
            {
                var vendorName = user.FullName.Split(',')[0].Trim();
                var vendor = context.Vendors.Single(v => v.VendorName == vendorName);
                var app = context.Applications.SingleOrDefault(a => a.Vendor.VendorId == vendor.VendorId);
                foreach (var client in user.ApiClients.Where(c => c.Application == null))
                {
                    client.Application = app;
                }
                user.Vendor = vendor;
            }
            context.SaveChanges();
        }

        private void SeedDefaultApplications(Vendor vendor, UsersContext context)
        {
            const string defaultAppName = "Default Application";
            const string claimSetName = "SIS Vendor";
            var app =
                context.Applications.SingleOrDefault(
                    a => a.ApplicationName == defaultAppName && a.Vendor.VendorId == vendor.VendorId);
            if (app != null) return;

            app = new Application
            {
                ApplicationName = defaultAppName,
                Vendor = vendor,
                ClaimSetName = claimSetName,
            };
            context.Applications.AddOrUpdate(app);

            context.SaveChanges();
        }

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
        }
    }
}
