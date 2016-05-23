using EdFi.Ods.Admin.Models.Sql;
using EdFi.Ods.Common.Utils.Resources;

namespace EdFi.Ods.Admin.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BaseMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClientAccessTokens",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Expiration = c.DateTime(nullable: false),
                        Scope = c.String(),
                        Client_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.Client_Id)
                .Index(t => t.Client_Id);
            
            CreateTable(
                "dbo.Clients",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Key = c.String(nullable: false, maxLength: 50),
                        Secret = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 50),
                        Approved = c.Boolean(nullable: false),
                        Dedicated = c.Boolean(nullable: false),
                        SandboxType = c.Int(nullable: false),
                        UserProfile_UserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserProfile", t => t.UserProfile_UserId)
                .Index(t => t.UserProfile_UserId);
            
            CreateTable(
                "dbo.ClientAuthorizationCodes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Expiration = c.DateTime(nullable: false),
                        Scope = c.String(),
                        Client_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.Client_Id)
                .Index(t => t.Client_Id);
            
            CreateTable(
                "dbo.UserProfile",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        Email = c.String(),
                        FullName = c.String(),
                    })
                .PrimaryKey(t => t.UserId);

            Sql(EmbeddedResourceReader.GetResourceString<AdminModelsSqlMarker>("Sproc_AdminTokenIsValid_2014_05_08.sql"));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Clients", "UserProfile_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.ClientAuthorizationCodes", "Client_Id", "dbo.Clients");
            DropForeignKey("dbo.ClientAccessTokens", "Client_Id", "dbo.Clients");
            DropIndex("dbo.Clients", new[] { "UserProfile_UserId" });
            DropIndex("dbo.ClientAuthorizationCodes", new[] { "Client_Id" });
            DropIndex("dbo.ClientAccessTokens", new[] { "Client_Id" });
            DropTable("dbo.UserProfile");
            DropTable("dbo.ClientAuthorizationCodes");
            DropTable("dbo.Clients");
            DropTable("dbo.ClientAccessTokens");
        }
    }
}
