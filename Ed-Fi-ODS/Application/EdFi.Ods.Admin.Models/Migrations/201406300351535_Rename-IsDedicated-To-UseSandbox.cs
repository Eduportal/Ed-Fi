using System.Data.Entity.Migrations;

namespace EdFi.Ods.Admin.Models
{
    public partial class RenameIsDedicatedToUseSandbox : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.ApiClients", "IsDedicated", "UseSandbox");
        }

        public override void Down()
        {
            RenameColumn("dbo.ApiClients", "UseSandbox", "IsDedicated");
        }
    }
}