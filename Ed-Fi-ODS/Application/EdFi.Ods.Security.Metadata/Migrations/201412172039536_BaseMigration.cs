namespace EdFi.Ods.Security.Metadata.Contexts
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BaseMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Actions",
                c => new
                    {
                        ActionId = c.Int(nullable: false, identity: true),
                        ActionName = c.String(nullable: false, maxLength: 255),
                        ActionUri = c.String(nullable: false, maxLength: 2048),
                    })
                .PrimaryKey(t => t.ActionId);
            
            CreateTable(
                "dbo.Applications",
                c => new
                    {
                        ApplicationId = c.Int(nullable: false, identity: true),
                        ApplicationName = c.String(),
                    })
                .PrimaryKey(t => t.ApplicationId);
            
            CreateTable(
                "dbo.AuthorizationStrategies",
                c => new
                    {
                        AuthorizationStrategyId = c.Int(nullable: false, identity: true),
                        DisplayName = c.String(nullable: false, maxLength: 255),
                        AuthorizationStrategyName = c.String(nullable: false, maxLength: 255),
                        Application_ApplicationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AuthorizationStrategyId)
                .ForeignKey("dbo.Applications", t => t.Application_ApplicationId, cascadeDelete: true)
                .Index(t => t.Application_ApplicationId);
            
            CreateTable(
                "dbo.ClaimSetResourceClaims",
                c => new
                    {
                        ClaimSetResourceClaimId = c.Int(nullable: false, identity: true),
                        Action_ActionId = c.Int(),
                        ClaimSet_ClaimSetId = c.Int(),
                        ResourceClaim_ResourceClaimId = c.Int(),
                    })
                .PrimaryKey(t => t.ClaimSetResourceClaimId)
                .ForeignKey("dbo.Actions", t => t.Action_ActionId)
                .ForeignKey("dbo.ClaimSets", t => t.ClaimSet_ClaimSetId)
                .ForeignKey("dbo.ResourceClaims", t => t.ResourceClaim_ResourceClaimId)
                .Index(t => t.Action_ActionId)
                .Index(t => t.ClaimSet_ClaimSetId)
                .Index(t => t.ResourceClaim_ResourceClaimId);
            
            CreateTable(
                "dbo.ClaimSets",
                c => new
                    {
                        ClaimSetId = c.Int(nullable: false, identity: true),
                        ClaimSetName = c.String(nullable: false, maxLength: 255),
                        Application_ApplicationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ClaimSetId)
                .ForeignKey("dbo.Applications", t => t.Application_ApplicationId, cascadeDelete: true)
                .Index(t => t.Application_ApplicationId);
            
            CreateTable(
                "dbo.ResourceClaims",
                c => new
                    {
                        ResourceClaimId = c.Int(nullable: false, identity: true),
                        DisplayName = c.String(nullable: false, maxLength: 255),
                        ResourceName = c.String(nullable: false, maxLength: 2048),
                        ClaimName = c.String(nullable: false, maxLength: 2048),
                        ParentResourceClaimId = c.Int(),
                        Application_ApplicationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ResourceClaimId)
                .ForeignKey("dbo.Applications", t => t.Application_ApplicationId, cascadeDelete: true)
                .ForeignKey("dbo.ResourceClaims", t => t.ParentResourceClaimId)
                .Index(t => t.Application_ApplicationId)
                .Index(t => t.ParentResourceClaimId);
            
            CreateTable(
                "dbo.ResourceClaimAuthorizationStrategies",
                c => new
                    {
                        ResourceClaimAuthorizationStrategyId = c.Int(nullable: false, identity: true),
                        Scheme = c.String(maxLength: 255),
                        Action_ActionId = c.Int(),
                        AuthorizationStrategy_AuthorizationStrategyId = c.Int(),
                        ResourceClaim_ResourceClaimId = c.Int(),
                    })
                .PrimaryKey(t => t.ResourceClaimAuthorizationStrategyId)
                .ForeignKey("dbo.Actions", t => t.Action_ActionId)
                .ForeignKey("dbo.AuthorizationStrategies", t => t.AuthorizationStrategy_AuthorizationStrategyId)
                .ForeignKey("dbo.ResourceClaims", t => t.ResourceClaim_ResourceClaimId)
                .Index(t => t.Action_ActionId)
                .Index(t => t.AuthorizationStrategy_AuthorizationStrategyId)
                .Index(t => t.ResourceClaim_ResourceClaimId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ResourceClaimAuthorizationStrategies", "ResourceClaim_ResourceClaimId", "dbo.ResourceClaims");
            DropForeignKey("dbo.ResourceClaimAuthorizationStrategies", "AuthorizationStrategy_AuthorizationStrategyId", "dbo.AuthorizationStrategies");
            DropForeignKey("dbo.ResourceClaimAuthorizationStrategies", "Action_ActionId", "dbo.Actions");
            DropForeignKey("dbo.ClaimSetResourceClaims", "ResourceClaim_ResourceClaimId", "dbo.ResourceClaims");
            DropForeignKey("dbo.ResourceClaims", "ParentResourceClaimId", "dbo.ResourceClaims");
            DropForeignKey("dbo.ResourceClaims", "Application_ApplicationId", "dbo.Applications");
            DropForeignKey("dbo.ClaimSetResourceClaims", "ClaimSet_ClaimSetId", "dbo.ClaimSets");
            DropForeignKey("dbo.ClaimSets", "Application_ApplicationId", "dbo.Applications");
            DropForeignKey("dbo.ClaimSetResourceClaims", "Action_ActionId", "dbo.Actions");
            DropForeignKey("dbo.AuthorizationStrategies", "Application_ApplicationId", "dbo.Applications");
            DropIndex("dbo.ResourceClaimAuthorizationStrategies", new[] { "ResourceClaim_ResourceClaimId" });
            DropIndex("dbo.ResourceClaimAuthorizationStrategies", new[] { "AuthorizationStrategy_AuthorizationStrategyId" });
            DropIndex("dbo.ResourceClaimAuthorizationStrategies", new[] { "Action_ActionId" });
            DropIndex("dbo.ClaimSetResourceClaims", new[] { "ResourceClaim_ResourceClaimId" });
            DropIndex("dbo.ResourceClaims", new[] { "ParentResourceClaimId" });
            DropIndex("dbo.ResourceClaims", new[] { "Application_ApplicationId" });
            DropIndex("dbo.ClaimSetResourceClaims", new[] { "ClaimSet_ClaimSetId" });
            DropIndex("dbo.ClaimSets", new[] { "Application_ApplicationId" });
            DropIndex("dbo.ClaimSetResourceClaims", new[] { "Action_ActionId" });
            DropIndex("dbo.AuthorizationStrategies", new[] { "Application_ApplicationId" });
            DropTable("dbo.ResourceClaimAuthorizationStrategies");
            DropTable("dbo.ResourceClaims");
            DropTable("dbo.ClaimSets");
            DropTable("dbo.ClaimSetResourceClaims");
            DropTable("dbo.AuthorizationStrategies");
            DropTable("dbo.Applications");
            DropTable("dbo.Actions");
        }
    }
}
