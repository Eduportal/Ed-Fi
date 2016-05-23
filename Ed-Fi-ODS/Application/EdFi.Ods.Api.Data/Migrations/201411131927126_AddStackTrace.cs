namespace EdFi.Ods.Api.Data
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStackTrace : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BulkOperationExceptions", "StackTrace", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BulkOperationExceptions", "StackTrace");
        }
    }
}
