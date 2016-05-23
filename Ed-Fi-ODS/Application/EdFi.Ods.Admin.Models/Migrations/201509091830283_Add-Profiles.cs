namespace EdFi.Ods.Admin.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProfiles : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Profiles",
                c => new
                    {
                        ProfileId = c.Int(nullable: false, identity: true),
                        ProfileName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ProfileId);
            
            CreateTable(
                "dbo.ProfileApplications",
                c => new
                    {
                        Profile_ProfileId = c.Int(nullable: false),
                        Application_ApplicationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Profile_ProfileId, t.Application_ApplicationId })
                .ForeignKey("dbo.Profiles", t => t.Profile_ProfileId, cascadeDelete: true)
                .ForeignKey("dbo.Applications", t => t.Application_ApplicationId, cascadeDelete: true)
                .Index(t => t.Profile_ProfileId)
                .Index(t => t.Application_ApplicationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProfileApplications", "Application_ApplicationId", "dbo.Applications");
            DropForeignKey("dbo.ProfileApplications", "Profile_ProfileId", "dbo.Profiles");
            DropIndex("dbo.ProfileApplications", new[] { "Application_ApplicationId" });
            DropIndex("dbo.ProfileApplications", new[] { "Profile_ProfileId" });
            DropTable("dbo.ProfileApplications");
            DropTable("dbo.Profiles");
        }
    }
}
