namespace EdFi.Ods.Admin.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCredentialRetrievalManagement : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApiClients", "KeyStatus", c => c.String());
            AddColumn("dbo.ApiClients", "ChallengeId", c => c.String());
            AddColumn("dbo.ApiClients", "ChallengeExpiry", c => c.DateTime());
            AddColumn("dbo.ApiClients", "ActivationCode", c => c.String());
            AddColumn("dbo.ApiClients", "ActivationRetried", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApiClients", "ActivationRetried");
            DropColumn("dbo.ApiClients", "ActivationCode");
            DropColumn("dbo.ApiClients", "ChallengeExpiry");
            DropColumn("dbo.ApiClients", "ChallengeId");
            DropColumn("dbo.ApiClients", "KeyStatus");
        }
    }
}
