namespace EdFi.Ods.Admin.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddClaimSetNameToApplication : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Applications", "ClaimSetName", c => c.String(maxLength: 255));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Applications", "ClaimSetName");
        }
    }
}
