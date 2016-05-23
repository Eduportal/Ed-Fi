using System.Data.Entity.Migrations;
using EdFi.Ods.Api.Data.Contexts;

namespace EdFi.Ods.Api.Data
{
    internal sealed class Configuration : DbMigrationsConfiguration<BulkOperationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
        }
    }
}
