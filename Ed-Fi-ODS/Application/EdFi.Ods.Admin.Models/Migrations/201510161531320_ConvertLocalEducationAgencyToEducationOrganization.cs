using EdFi.Ods.Admin.Models.MigrationHelpers;
using EdFi.Ods.Admin.Models.Sql;
using EdFi.Ods.Common.Utils.Resources;

namespace EdFi.Ods.Admin.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConvertLocalEducationAgencyToEducationOrganization : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ApplicationEducationOrganizations",
                c => new
                    {
                        ApplicationEducationOrganizationId = c.Int(nullable: false, identity: true),
                        EducationOrganizationId = c.Int(nullable: false),
                        Application_ApplicationId = c.Int(),
                    })
                .PrimaryKey(t => t.ApplicationEducationOrganizationId)
                .ForeignKey("dbo.Applications", t => t.Application_ApplicationId)
                .Index(t => t.Application_ApplicationId);
            
            CreateTable(
                "dbo.ApiClientApplicationEducationOrganizations",
                c => new
                    {
                        ApiClient_ApiClientId = c.Int(nullable: false),
                        ApplicationEducationOrganization_ApplicationEducationOrganizationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ApiClient_ApiClientId, t.ApplicationEducationOrganization_ApplicationEducationOrganizationId })
                .ForeignKey("dbo.ApiClients", t => t.ApiClient_ApiClientId, cascadeDelete: true)
                .ForeignKey("dbo.ApplicationEducationOrganizations", t => t.ApplicationEducationOrganization_ApplicationEducationOrganizationId, cascadeDelete: true)
                .Index(t => t.ApiClient_ApiClientId)
                .Index(t => t.ApplicationEducationOrganization_ApplicationEducationOrganizationId);

            Sql(@"SET IDENTITY_INSERT dbo.ApplicationEducationOrganizations ON;
INSERT INTO dbo.ApplicationEducationOrganizations ([ApplicationEducationOrganizationId], [EducationOrganizationId], [Application_ApplicationId])
SELECT [ApplicationLocalEducationAgencyId], [LocalEducationAgencyId], [Application_ApplicationId] FROM dbo.ApplicationLocalEducationAgencies;
SET IDENTITY_INSERT dbo.ApplicationEducationOrganizations OFF;");

            Sql(@"INSERT INTO dbo.ApiClientApplicationEducationOrganizations ([ApiClient_ApiClientId], [ApplicationEducationOrganization_ApplicationEducationOrganizationId])
SELECT [ApiClient_ApiClientId], [ApplicationLocalEducationAgency_ApplicationLocalEducationAgencyId] FROM dbo.ApiClientApplicationLocalEducationAgencies;");

            DropForeignKey("dbo.ApiClientApplicationLocalEducationAgencies", "ApiClient_ApiClientId", "dbo.ApiClients");
            DropForeignKey("dbo.ApiClientApplicationLocalEducationAgencies", "ApplicationLocalEducationAgency_ApplicationLocalEducationAgencyId", "dbo.ApplicationLocalEducationAgencies");
            DropForeignKey("dbo.ApplicationLocalEducationAgencies", "Application_ApplicationId", "dbo.Applications");
            DropIndex("dbo.ApplicationLocalEducationAgencies", new[] { "Application_ApplicationId" });
            DropIndex("dbo.ApiClientApplicationLocalEducationAgencies", new[] { "ApiClient_ApiClientId" });
            DropIndex("dbo.ApiClientApplicationLocalEducationAgencies", new[] { "ApplicationLocalEducationAgency_ApplicationLocalEducationAgencyId" });

            DropTable("dbo.ApiClientApplicationLocalEducationAgencies");
            DropTable("dbo.ApplicationLocalEducationAgencies");

            Sql(SprocHelper.GenerateDropFor("AccessTokenIsValid"));
            Sql(EmbeddedResourceReader.GetResourceString<AdminModelsSqlMarker>("Sproc_AdminTokenIsValid_201510161531320.sql"));
        }
        
        public override void Down()
        {
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
                "dbo.ApiClientApplicationLocalEducationAgencies",
                c => new
                    {
                        ApiClient_ApiClientId = c.Int(nullable: false),
                        ApplicationLocalEducationAgency_ApplicationLocalEducationAgencyId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ApiClient_ApiClientId, t.ApplicationLocalEducationAgency_ApplicationLocalEducationAgencyId })
                .ForeignKey("dbo.ApiClients", t => t.ApiClient_ApiClientId, cascadeDelete: true)
                .ForeignKey("dbo.ApplicationLocalEducationAgencies", t => t.ApplicationLocalEducationAgency_ApplicationLocalEducationAgencyId, cascadeDelete: true)
                .Index(t => t.ApiClient_ApiClientId)
                .Index(t => t.ApplicationLocalEducationAgency_ApplicationLocalEducationAgencyId);

            Sql(@"SET IDENTITY_INSERT dbo.ApplicationLocalEducationAgencies ON;
INSERT INTO dbo.ApplicationLocalEducationAgencies ([ApplicationLocalEducationAgencyId], [LocalEducationAgencyId], [Application_ApplicationId])
SELECT [ApplicationEducationOrganizationId], [EducationOrganizationId], [Application_ApplicationId] FROM dbo.ApplicationEducationOrganizations;
SET IDENTITY_INSERT dbo.ApplicationLocalEducationAgencies OFF;");

            Sql(@"INSERT INTO dbo.ApiClientApplicationLocalEducationAgencies ([ApiClient_ApiClientId], [ApplicationLocalEducationAgency_ApplicationLocalEducationAgencyId])
SELECT [ApiClient_ApiClientId], [ApplicationEducationOrganization_ApplicationEducationOrganizationId] FROM dbo.ApiClientApplicationEducationOrganizations;");

            DropForeignKey("dbo.ApplicationEducationOrganizations", "Application_ApplicationId", "dbo.Applications");
            DropForeignKey("dbo.ApiClientApplicationEducationOrganizations", "ApplicationEducationOrganization_ApplicationEducationOrganizationId", "dbo.ApplicationEducationOrganizations");
            DropForeignKey("dbo.ApiClientApplicationEducationOrganizations", "ApiClient_ApiClientId", "dbo.ApiClients");
            DropIndex("dbo.ApiClientApplicationEducationOrganizations", new[] { "ApplicationEducationOrganization_ApplicationEducationOrganizationId" });
            DropIndex("dbo.ApiClientApplicationEducationOrganizations", new[] { "ApiClient_ApiClientId" });
            DropIndex("dbo.ApplicationEducationOrganizations", new[] { "Application_ApplicationId" });
            DropTable("dbo.ApiClientApplicationEducationOrganizations");
            DropTable("dbo.ApplicationEducationOrganizations");
            Sql(SprocHelper.GenerateDropFor("AccessTokenIsValid"));
            Sql(EmbeddedResourceReader.GetResourceString<AdminModelsSqlMarker>("Sproc_AdminTokenIsValid_201507200115139.sql"));
        }
    }
}
