namespace EdFi.Ods.Api.Data
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Base : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BulkOperationExceptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentUploadFileId = c.String(maxLength: 128),
                        Code = c.Int(nullable: false),
                        Type = c.String(),
                        Element = c.String(),
                        Message = c.String(),
                        DateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UploadFiles", t => t.ParentUploadFileId)
                .Index(t => t.ParentUploadFileId);
            
            CreateTable(
                "dbo.UploadFiles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Format = c.String(),
                        InterchangeType = c.String(),
                        Status = c.Int(nullable: false),
                        Size = c.Long(nullable: false),
                        BulkOperation_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BulkOperations", t => t.BulkOperation_Id)
                .Index(t => t.BulkOperation_Id);
            
            CreateTable(
                "dbo.UploadFileChunks",
                c => new
                    {
                        UploadFile_Id = c.String(nullable: false, maxLength: 128),
                        Id = c.String(nullable: false, maxLength: 128),
                        Offset = c.Long(nullable: false),
                        Size = c.Long(nullable: false),
                        Chunk = c.Binary(),
                    })
                .PrimaryKey(t => new { t.UploadFile_Id, t.Id })
                .ForeignKey("dbo.UploadFiles", t => t.UploadFile_Id, cascadeDelete: true)
                .Index(t => t.UploadFile_Id);
            
            CreateTable(
                "dbo.BulkOperations",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ResetDistrictData = c.String(),
                        Status = c.Int(nullable: false),
                        DatabaseName = c.String(),
                        SchoolYear = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ApiEventLogEntries",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Event = c.String(nullable: false),
                        HttpMethod = c.String(),
                        Uri = c.String(),
                        Message = c.String(),
                        AggregateName = c.String(nullable: false),
                        AggregateKey = c.String(nullable: false),
                        ApplicationKey = c.String(nullable: false),
                        ETag = c.String(nullable: false),
                        Timestamp = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UploadFiles", "BulkOperation_Id", "dbo.BulkOperations");
            DropForeignKey("dbo.BulkOperationExceptions", "ParentUploadFileId", "dbo.UploadFiles");
            DropForeignKey("dbo.UploadFileChunks", "UploadFile_Id", "dbo.UploadFiles");
            DropIndex("dbo.UploadFiles", new[] { "BulkOperation_Id" });
            DropIndex("dbo.BulkOperationExceptions", new[] { "ParentUploadFileId" });
            DropIndex("dbo.UploadFileChunks", new[] { "UploadFile_Id" });
            DropTable("dbo.ApiEventLogEntries");
            DropTable("dbo.BulkOperations");
            DropTable("dbo.UploadFileChunks");
            DropTable("dbo.UploadFiles");
            DropTable("dbo.BulkOperationExceptions");
        }
    }
}
