using EdFi.Ods.Admin.Models.MigrationHelpers;
using EdFi.Ods.Admin.Models.Sql;
using EdFi.Ods.Common.Utils.Resources;

namespace EdFi.Ods.Admin.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLeaToContext : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ApplicationLocalEducationAgencyApiClients", newName: "ApiClientApplicationLocalEducationAgencies");
            Sql(SprocHelper.GenerateDropFor("AccessTokenIsValid"));
            Sql(EmbeddedResourceReader.GetResourceString<AdminModelsSqlMarker>("Sproc_AdminTokenIsValid_201406022004546.sql"));
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.ApiClientApplicationLocalEducationAgencies", newName: "ApplicationLocalEducationAgencyApiClients");
            Sql(SprocHelper.GenerateDropFor("AccessTokenIsValid"));
            Sql(EmbeddedResourceReader.GetResourceString<AdminModelsSqlMarker>("Sproc_AdminTokenIsValid_2014_06_02.sql"));
        }
    }
}
