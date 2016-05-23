using EdFi.Ods.Security.Metadata.Models;
using System.Data.Entity;
using System.Data.Entity.Migrations;


namespace EdFi.Ods.Security.Metadata.Contexts
{
    public class SecurityContext : DbContext
    {
        public SecurityContext() : base("EdFi_Security")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<SecurityContext, Configuration>());
        }

        public DbSet<Application> Applications { get; set; }
        public DbSet<Action> Actions { get; set; }
        public DbSet<AuthorizationStrategy> AuthorizationStrategies { get; set; }
        public DbSet<ClaimSet> ClaimSets { get; set; }
        public DbSet<ClaimSetResourceClaim> ClaimSetResourceClaims { get; set; }
        public DbSet<ResourceClaim> ResourceClaims { get; set; }
        public DbSet<ResourceClaimAuthorizationStrategy> ResourceClaimAuthorizationStrategies { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ResourceClaim>().HasOptional(rc => rc.ParentResourceClaim).WithMany().HasForeignKey(fk => fk.ParentResourceClaimId);
            
            base.OnModelCreating(modelBuilder);
        }
    }


    internal sealed class Configuration : DbMigrationsConfiguration<SecurityContext>
    {
        protected override void Seed(SecurityContext context)
        {
            var seedData = new SecurityContextSeed();
            seedData.SeedActions(context);
            seedData.SeedApplication(context);
            seedData.SeedAuthorizationStrategies(context);
            seedData.SeedClaimSets(context);
            seedData.SeedResourceClaims(context);
            seedData.SeedClaimSetResourceClaims(context);
            seedData.SeedResourceClaimsAuthorizationStrategies(context);

            base.Seed(context);
        }
        
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
        }
    }
}
