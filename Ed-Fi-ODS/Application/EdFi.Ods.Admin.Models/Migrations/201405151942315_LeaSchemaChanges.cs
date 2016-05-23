namespace EdFi.Ods.Admin.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LeaSchemaChanges : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ClientAccessTokens", "Client_Id", "dbo.Clients");
            DropForeignKey("dbo.ClientAuthorizationCodes", "Client_Id", "dbo.Clients");
            DropForeignKey("dbo.Clients", "UserProfile_UserId", "dbo.UserProfile");

            Sql(@"IF EXISTS (SELECT * FROM sys.objects WHERE name = 'webpages_UsersInRoles') 
                BEGIN
                    IF object_id(N'[dbo].[fk_UserId]', N'F') IS NOT NULL
                    BEGIN 
                        ALTER TABLE [dbo].[webpages_UsersInRoles] DROP CONSTRAINT [fk_UserId]
                    END 
                    IF EXISTS (SELECT * FROM sys.objects WHERE name = 'PK__webpages__AF2760AD1BFD2C07')
                    BEGIN 
                        ALTER TABLE [dbo].[webpages_UsersInRoles] DROP CONSTRAINT [PK__webpages__AF2760AD1BFD2C07]
                    END 
                END
                ");
            
            DropIndex("dbo.ClientAccessTokens", new[] { "Client_Id" });
            DropIndex("dbo.ClientAuthorizationCodes", new[] { "Client_Id" });
            DropIndex("dbo.Clients", new[] { "UserProfile_UserId" });
            CreateTable(
                "dbo.Applications",
                c => new
                    {
                        ApplicationId = c.Int(nullable: false, identity: true),
                        ApplicationName = c.String(),
                        Vendor_VendorId = c.Int(),
                    })
                .PrimaryKey(t => t.ApplicationId)
                .ForeignKey("dbo.Vendors", t => t.Vendor_VendorId)
                .Index(t => t.Vendor_VendorId);
            
            CreateTable(
                "dbo.Vendors",
                c => new
                    {
                        VendorId = c.Int(nullable: false, identity: true),
                        VendorName = c.String(),
                    })
                .PrimaryKey(t => t.VendorId);
            
            CreateTable(
                "dbo.ApiClients",
                c => new
                    {
                        ApiClientId = c.Int(nullable: false, identity: true),
                        Key = c.String(nullable: false, maxLength: 50),
                        Secret = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 50),
                        IsApproved = c.Boolean(nullable: false),
                        IsDedicated = c.Boolean(nullable: false),
                        SandboxType = c.Int(nullable: false),
                        Application_ApplicationId = c.Int(),
                        User_UserId = c.Int(),
                    })
                .PrimaryKey(t => t.ApiClientId)
                .ForeignKey("dbo.Applications", t => t.Application_ApplicationId)
                .ForeignKey("dbo.Users", t => t.User_UserId)
                .Index(t => t.Application_ApplicationId)
                .Index(t => t.User_UserId);
            
            CreateTable(
                "dbo.ApplicationLocalEducationAgencies",
                c => new
                    {
                        ApplicationLocalEducationAgencyId = c.Int(nullable: false, identity: true),
                        LocalEducationAgencyId = c.Int(nullable: false),
                        Application_ApplicationId = c.Int(),
                    })
                .PrimaryKey(t => t.ApplicationLocalEducationAgencyId)
                .ForeignKey("dbo.Applications", t => t.Application_ApplicationId)
                .Index(t => t.Application_ApplicationId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        Email = c.String(),
                        FullName = c.String(),
                        Vendor_VendorId = c.Int(),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.Vendors", t => t.Vendor_VendorId)
                .Index(t => t.Vendor_VendorId);
            
            CreateTable(
                "dbo.ApplicationLocalEducationAgencyApiClients",
                c => new
                    {
                        ApplicationLocalEducationAgency_ApplicationLocalEducationAgencyId = c.Int(nullable: false),
                        ApiClient_ApiClientId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ApplicationLocalEducationAgency_ApplicationLocalEducationAgencyId, t.ApiClient_ApiClientId })
                .ForeignKey("dbo.ApplicationLocalEducationAgencies", t => t.ApplicationLocalEducationAgency_ApplicationLocalEducationAgencyId, cascadeDelete: true)
                .ForeignKey("dbo.ApiClients", t => t.ApiClient_ApiClientId, cascadeDelete: true)
                .Index(t => t.ApplicationLocalEducationAgency_ApplicationLocalEducationAgencyId)
                .Index(t => t.ApiClient_ApiClientId);

            Sql("SET IDENTITY_INSERT dbo.ApiClients ON; INSERT INTO dbo.ApiClients ([ApiClientId], [Key], [Name], [Secret], [IsApproved], [IsDedicated], [SandboxType], [User_UserId]) SELECT [Id], [Key], [Name], [Secret], [Approved], [Dedicated], [SandboxType], [UserProfile_UserId] FROM dbo.Clients; SET IDENTITY_INSERT dbo.ApiClients OFF;");
            Sql("SET IDENTITY_INSERT dbo.[Users] ON; INSERT INTO dbo.[Users] ([UserId], [Email], [FullName]) SELECT [UserId], [Email], [FullName] FROM  dbo.UserProfile; SET IDENTITY_INSERT dbo.[Users] OFF;");

            AddColumn("dbo.ClientAccessTokens", "ApiClient_ApiClientId", c => c.Int());
            AddColumn("dbo.ClientAuthorizationCodes", "ApiClient_ApiClientId", c => c.Int());
            CreateIndex("dbo.ClientAccessTokens", "ApiClient_ApiClientId");
            CreateIndex("dbo.ClientAuthorizationCodes", "ApiClient_ApiClientId");
            AddForeignKey("dbo.ClientAccessTokens", "ApiClient_ApiClientId", "dbo.ApiClients", "ApiClientId");
            AddForeignKey("dbo.ClientAuthorizationCodes", "ApiClient_ApiClientId", "dbo.ApiClients", "ApiClientId");

            Sql(@"IF EXISTS (SELECT * FROM sys.objects WHERE name = 'webpages_UsersInRoles') 
                BEGIN
                    IF object_id(N'[dbo].[fk_UserId]', N'F') IS NULL
                    BEGIN 
                        ALTER TABLE [dbo].[webpages_UsersInRoles] ADD CONSTRAINT fk_UserId FOREIGN KEY (UserId) REFERENCES Users(UserId)
                    END 
                    IF EXISTS (SELECT * FROM sys.objects WHERE name = 'PK__webpages__AF2760AD1BFD2C07')
                    BEGIN 
                        ALTER TABLE [dbo].[webpages_UsersInRoles] ADD CONSTRAINT PK__webpages__AF2760AD1BFD2C07 PRIMARY KEY (UserId,RoleId)
                    END 
                END
                "); 

            DropColumn("dbo.ClientAccessTokens", "Client_Id");
            DropColumn("dbo.ClientAuthorizationCodes", "Client_Id");
            DropTable("dbo.Clients");
            DropTable("dbo.UserProfile");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UserProfile",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        Email = c.String(),
                        FullName = c.String(),
                    })
                .PrimaryKey(t => t.UserId);
            
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
                .PrimaryKey(t => t.Id);

            Sql("SET IDENTITY_INSERT dbo.Clients ON; INSERT INTO dbo.Clients ([Id], [Key], [Name], [Secret], [Approved], [Dedicated], [SandboxType], [UserProfile_UserId]) SELECT [ApiClientId], [Key], [Name], [Secret], [IsApproved], [IsDedicated], [SandboxType], [User_UserId] FROM dbo.ApiClients; SET IDENTITY_INSERT dbo.Clients OFF;");
            Sql("SET IDENTITY_INSERT dbo.UserProfile ON; INSERT INTO dbo.UserProfile ([UserId], [Email], [FullName]) SELECT [UserId], [Email], [FullName] FROM  dbo.[Users]; SET IDENTITY_INSERT dbo.UserProfile OFF;");

            AddColumn("dbo.ClientAuthorizationCodes", "Client_Id", c => c.Int());
            AddColumn("dbo.ClientAccessTokens", "Client_Id", c => c.Int());
            DropForeignKey("dbo.Users", "Vendor_VendorId", "dbo.Vendors");
            DropForeignKey("dbo.ApiClients", "User_UserId", "dbo.Users");
            DropForeignKey("dbo.ClientAuthorizationCodes", "ApiClient_ApiClientId", "dbo.ApiClients");
            DropForeignKey("dbo.ClientAccessTokens", "ApiClient_ApiClientId", "dbo.ApiClients");
            DropForeignKey("dbo.ApplicationLocalEducationAgencyApiClients", "ApiClient_ApiClientId", "dbo.ApiClients");
            DropForeignKey("dbo.ApplicationLocalEducationAgencyApiClients", "ApplicationLocalEducationAgency_ApplicationLocalEducationAgencyId", "dbo.ApplicationLocalEducationAgencies");
            DropForeignKey("dbo.ApplicationLocalEducationAgencies", "Application_ApplicationId", "dbo.Applications");
            DropForeignKey("dbo.ApiClients", "Application_ApplicationId", "dbo.Applications");
            DropForeignKey("dbo.Applications", "Vendor_VendorId", "dbo.Vendors");
            
            Sql(@"IF EXISTS (SELECT * FROM sys.objects WHERE name = 'webpages_UsersInRoles') 
                BEGIN
                    IF object_id(N'[dbo].[fk_UserId]', N'F') IS NOT NULL
                    BEGIN 
                        ALTER TABLE [dbo].[webpages_UsersInRoles] DROP CONSTRAINT [fk_UserId]
                    END 
                    IF EXISTS (SELECT * FROM sys.objects WHERE name = 'PK__webpages__AF2760AD1BFD2C07')
                    BEGIN 
                        ALTER TABLE [dbo].[webpages_UsersInRoles] DROP CONSTRAINT [PK__webpages__AF2760AD1BFD2C07]
                    END 
                END
                "); 
            DropIndex("dbo.Users", new[] { "Vendor_VendorId" });
            DropIndex("dbo.ApiClients", new[] { "User_UserId" });
            DropIndex("dbo.ClientAuthorizationCodes", new[] { "ApiClient_ApiClientId" });
            DropIndex("dbo.ClientAccessTokens", new[] { "ApiClient_ApiClientId" });
            DropIndex("dbo.ApplicationLocalEducationAgencyApiClients", new[] { "ApiClient_ApiClientId" });
            DropIndex("dbo.ApplicationLocalEducationAgencyApiClients", new[] { "ApplicationLocalEducationAgency_ApplicationLocalEducationAgencyId" });
            DropIndex("dbo.ApplicationLocalEducationAgencies", new[] { "Application_ApplicationId" });
            DropIndex("dbo.ApiClients", new[] { "Application_ApplicationId" });
            DropIndex("dbo.Applications", new[] { "Vendor_VendorId" });
            DropColumn("dbo.ClientAuthorizationCodes", "ApiClient_ApiClientId");
            DropColumn("dbo.ClientAccessTokens", "ApiClient_ApiClientId");
            DropTable("dbo.ApplicationLocalEducationAgencyApiClients");
            DropTable("dbo.Users");
            DropTable("dbo.ApplicationLocalEducationAgencies");
            DropTable("dbo.ApiClients");
            DropTable("dbo.Vendors");
            DropTable("dbo.Applications");
            CreateIndex("dbo.Clients", "UserProfile_UserId");
            CreateIndex("dbo.ClientAuthorizationCodes", "Client_Id");
            CreateIndex("dbo.ClientAccessTokens", "Client_Id");
            AddForeignKey("dbo.Clients", "UserProfile_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.ClientAuthorizationCodes", "Client_Id", "dbo.Clients", "Id");
            AddForeignKey("dbo.ClientAccessTokens", "Client_Id", "dbo.Clients", "Id");

            Sql(@"IF EXISTS (SELECT * FROM sys.objects WHERE name = 'webpages_UsersInRoles') 
                BEGIN
                    IF object_id(N'[dbo].[fk_UserId]', N'F') IS NULL
                    BEGIN 
                        ALTER TABLE [dbo].[webpages_UsersInRoles] ADD CONSTRAINT fk_UserId FOREIGN KEY (UserId) REFERENCES UserProfile(UserId)
                    END 
                    IF EXISTS (SELECT * FROM sys.objects WHERE name = 'PK__webpages__AF2760AD1BFD2C07')
                    BEGIN 
                        ALTER TABLE [dbo].[webpages_UsersInRoles] ADD CONSTRAINT PK__webpages__AF2760AD1BFD2C07 PRIMARY KEY (UserId,RoleId)
                    END 
                END
                ");
        }
    }
}
